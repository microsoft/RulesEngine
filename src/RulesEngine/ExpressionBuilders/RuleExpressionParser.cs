// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using FastExpressionCompiler;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RulesEngine.ExpressionBuilders
{
    public class RuleExpressionParser
    {
        private readonly ReSettings _reSettings;
        private readonly IDictionary<string, MethodInfo> _methodInfo;
        private readonly MemCache _compiledDelegateCache;
        private readonly string _settingsFingerprint;

        public RuleExpressionParser(ReSettings reSettings = null)
        {
            _reSettings = reSettings ?? new ReSettings();
            _methodInfo = new Dictionary<string, MethodInfo>();
            PopulateMethodInfo();

            var cacheConfig = _reSettings.CacheConfig ?? new MemCacheConfig();
            if (cacheConfig.SizeLimit > 0)
            {
                _compiledDelegateCache = new MemCache(cacheConfig);
                _settingsFingerprint = BuildSettingsFingerprint(_reSettings);
            }
        }

        private static string BuildSettingsFingerprint(ReSettings settings)
        {
            var customTypesKey = settings.CustomTypes == null
                ? "<null>"
                : string.Join(",", settings.CustomTypes.Where(t => t != null).Select(t => t.AssemblyQualifiedName));
            return $"cs={settings.IsExpressionCaseSensitive};fast={settings.UseFastExpressionCompiler};types={customTypesKey}";
        }

        private string BuildCompileCacheKey(string expression, RuleParameter[] ruleParams, Type returnType)
        {
            var paramKey = ruleParams == null
                ? string.Empty
                : string.Join("|", ruleParams.Select(c => c == null ? "<null>" : c.Name + "_" + (c.Type?.AssemblyQualifiedName ?? "<null>")));
            var returnTypeKey = returnType?.AssemblyQualifiedName ?? "<null>";
            return $"S[{_settingsFingerprint}]|R[{returnTypeKey}]|P[{paramKey}]|E[{expression}]";
        }

        private string BuildScopedParamsCacheKey(RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams)
        {
            var paramKey = ruleParams == null
                ? string.Empty
                : string.Join("|", ruleParams.Select(c => c == null ? "<null>" : c.Name + "_" + (c.Type?.AssemblyQualifiedName ?? "<null>")));
            var scopedKey = ruleExpParams == null
                ? string.Empty
                : string.Join("|", ruleExpParams.Select(c => DescribeScopedParam(c)));
            return $"SCOPED|S[{_settingsFingerprint}]|P[{paramKey}]|X[{scopedKey}]";
        }

        private static string DescribeScopedParam(RuleExpressionParameter expParam)
        {
            if (expParam?.ParameterExpression == null)
            {
                return "<null>";
            }
            var name = expParam.ParameterExpression.Name;
            var type = expParam.ParameterExpression.Type?.AssemblyQualifiedName ?? "<null>";
            var value = expParam.ValueExpression?.ToString() ?? "<null>";
            return $"{name}_{type}=[{value}]";
        }

        private void PopulateMethodInfo()
        {
            var dict_add = typeof(Dictionary<string, object>).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(object) }, null);
            _methodInfo.Add("dict_add", dict_add);
        }

        private ParsingConfig _cachedParsingConfig;
        private Type[] _cachedParsingConfigCustomTypes;

        private ParsingConfig GetParsingConfig()
        {
            // Building a CustomTypeProvider is expensive: System.Linq.Dynamic.Core's
            // DefaultDynamicLinqCustomTypeProvider scans all AppDomain assemblies for
            // [DynamicLinqType] and only caches per provider instance. Reuse one config
            // until ReSettings.CustomTypes is swapped (AutoRegisterInputType does this
            // on workflow registration).
            var customTypes = _reSettings.CustomTypes;
            var config = _cachedParsingConfig;
            if (config == null || !ReferenceEquals(_cachedParsingConfigCustomTypes, customTypes))
            {
                config = new ParsingConfig {
                    CustomTypeProvider = new CustomTypeProvider(customTypes),
                    IsCaseSensitive = _reSettings.IsExpressionCaseSensitive
                };
                _cachedParsingConfigCustomTypes = customTypes;
                _cachedParsingConfig = config;
            }
            return config;
        }

        public Expression Parse(string expression, ParameterExpression[] parameters, Type returnType)
        {
            var config = GetParsingConfig();

            // Instead of immediately returning default values, allow for expression parsing to handle dynamic evaluation.
            try
            {
                return new ExpressionParser(parameters, expression, Array.Empty<object>(), config).Parse(returnType);
            }
            catch (ParseException)
            {
                if (_reSettings.EnableExceptionAsErrorMessageForRuleExpressionParsing)
                {
                    throw;
                }
                return Expression.Constant(GetDefaultValueForType(returnType));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private object GetDefaultValueForType(Type type)
        {
            if (type == typeof(bool))
                return false;
            if (type == typeof(int) || type == typeof(float) || type == typeof(double))
                return int.MinValue;
            return null;
        }

        public Func<object[], T> Compile<T>(string expression, RuleParameter[] ruleParams)
        {
            if (_compiledDelegateCache == null)
            {
                return CompileInternal<T>(expression, ruleParams);
            }

            var cacheKey = BuildCompileCacheKey(expression, ruleParams, typeof(T));
            return _compiledDelegateCache.GetOrCreate(cacheKey, () => CompileInternal<T>(expression, ruleParams));
        }

        private Func<object[], T> CompileInternal<T>(string expression, RuleParameter[] ruleParams)
        {
            var rtype = typeof(T);
            if (rtype == typeof(object))
            {
                rtype = null;
            }
            var parameterExpressions = GetParameterExpression(ruleParams).ToArray();

            var e = Parse(expression, parameterExpressions, rtype);
            if (rtype == null)
            {
                e = Expression.Convert(e, typeof(T));
            }
            var expressionBody = new List<Expression>() { e };
            var wrappedExpression = WrapExpression<T>(expressionBody, parameterExpressions, new ParameterExpression[] { });
            return CompileExpression(wrappedExpression);
        }

        private Func<object[], T> CompileExpression<T>(Expression<Func<object[], T>> expression)
        {
            if (_reSettings.UseFastExpressionCompiler)
            {
                return expression.CompileFast();
            }
            return expression.Compile();
        }

        private Expression<Func<object[], T>> WrapExpression<T>(List<Expression> expressionList, ParameterExpression[] parameters, ParameterExpression[] variables)
        {
            var argExp = Expression.Parameter(typeof(object[]), "args");
            var paramExps = parameters.Select((c, i) => {
                var arg = Expression.ArrayAccess(argExp, Expression.Constant(i));
                return (Expression)Expression.Assign(c, Expression.Convert(arg, c.Type));
            });
            var blockExpSteps = paramExps.Concat(expressionList);
            var blockExp = Expression.Block(parameters.Concat(variables), blockExpSteps);
            return Expression.Lambda<Func<object[], T>>(blockExp, argExp);
        }

        internal Func<object[], Dictionary<string, object>> CompileRuleExpressionParameters(RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams = null)
        {
            ruleExpParams = ruleExpParams ?? new RuleExpressionParameter[] { };

            if (_compiledDelegateCache == null)
            {
                return CompileExpression(CreateDictionaryExpression(ruleParams, ruleExpParams));
            }

            var cacheKey = BuildScopedParamsCacheKey(ruleParams, ruleExpParams);
            return _compiledDelegateCache.GetOrCreate(
                cacheKey,
                () => CompileExpression(CreateDictionaryExpression(ruleParams, ruleExpParams)));
        }

        public T Evaluate<T>(string expression, RuleParameter[] ruleParams)
        {
            var func = Compile<T>(expression, ruleParams);
            return func(ruleParams.Select(c => c.Value).ToArray());
        }

        private IEnumerable<Expression> CreateAssignedParameterExpression(RuleExpressionParameter[] ruleExpParams)
        {
            return ruleExpParams.Select((c, i) => {
                return Expression.Assign(c.ParameterExpression, c.ValueExpression);
            });
        }

        // <summary>
        /// Gets the parameter expression.
        /// </summary>
        /// <param name="ruleParams">The types.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// types
        /// or
        /// type
        /// </exception>
        private IEnumerable<ParameterExpression> GetParameterExpression(RuleParameter[] ruleParams)
        {
            foreach (var ruleParam in ruleParams)
            {
                if (ruleParam == null)
                {
                    throw new ArgumentException($"{nameof(ruleParam)} can't be null.");
                }

                yield return ruleParam.ParameterExpression;
            }
        }

        private Expression<Func<object[], Dictionary<string, object>>> CreateDictionaryExpression(RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams)
        {
            var body = new List<Expression>();
            var paramExp = new List<ParameterExpression>();
            var variableExp = new List<ParameterExpression>();


            var variableExpressions = CreateAssignedParameterExpression(ruleExpParams);

            body.AddRange(variableExpressions);

            var dict = Expression.Variable(typeof(Dictionary<string, object>));
            var add = _methodInfo["dict_add"];

            body.Add(Expression.Assign(dict, Expression.New(typeof(Dictionary<string, object>))));
            variableExp.Add(dict);

            for (var i = 0; i < ruleParams.Length; i++)
            {
                paramExp.Add(ruleParams[i].ParameterExpression);
            }
            for (var i = 0; i < ruleExpParams.Length; i++)
            {
                var key = Expression.Constant(ruleExpParams[i].ParameterExpression.Name);
                var value = Expression.Convert(ruleExpParams[i].ParameterExpression, typeof(object));
                variableExp.Add(ruleExpParams[i].ParameterExpression);
                body.Add(Expression.Call(dict, add, key, value));

            }
            // Return value
            body.Add(dict);

            return WrapExpression<Dictionary<string, object>>(body, paramExp.ToArray(), variableExp.ToArray());
        }
    }
}