// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using FastExpressionCompiler;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Reflection;

namespace RulesEngine.ExpressionBuilders
{
    public class RuleExpressionParser
    {
        private readonly ReSettings _reSettings;
        private static MemCache _memoryCache;
        private readonly IDictionary<string, MethodInfo> _methodInfo;

        public RuleExpressionParser(ReSettings reSettings)
        {
            _reSettings = reSettings;
            _memoryCache = _memoryCache ?? new MemCache(new MemCacheConfig {
                SizeLimit = 1000
            });
            _methodInfo = new Dictionary<string, MethodInfo>();
            PopulateMethodInfo();
        }

        private void PopulateMethodInfo()
        {
            var dict_add = typeof(Dictionary<string, object>).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(object) }, null);
            _methodInfo.Add("dict_add", dict_add);
        }
        public Expression Parse(string expression, ParameterExpression[] parameters, Type returnType)
        {
            var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
            return new ExpressionParser(parameters, expression, new object[] { }, config).Parse(returnType);

        }

        public Func<object[], T> Compile<T>(string expression, RuleParameter[] ruleParams)
        {
            var rtype = typeof(T);
            if(rtype == typeof(object))
            {
                rtype = null;
            }
            var cacheKey = GetCacheKey(expression, ruleParams, typeof(T));
            return _memoryCache.GetOrCreate(cacheKey, () => {
                var parameterExpressions = GetParameterExpression(ruleParams).ToArray();
            
                var e = Parse(expression, parameterExpressions, rtype);
                if(rtype == null)
                {
                    e = Expression.Convert(e, typeof(T));
                }
                var expressionBody = new List<Expression>() { e };
                var wrappedExpression = WrapExpression<T>(expressionBody, parameterExpressions, new ParameterExpression[] { });
                return wrappedExpression.CompileFast();
            });
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

        internal Func<object[],Dictionary<string,object>> CompileRuleExpressionParameters(RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams = null)
        {
            ruleExpParams = ruleExpParams ?? new RuleExpressionParameter[] { };
            var expression = CreateDictionaryExpression(ruleParams, ruleExpParams);
            return expression.CompileFast();
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

        private Expression<Func<object[],Dictionary<string,object>>> CreateDictionaryExpression(RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams)
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

            for(var i = 0; i < ruleParams.Length; i++)
            {
                paramExp.Add(ruleParams[i].ParameterExpression);
            }
            for(var i = 0; i < ruleExpParams.Length; i++)
            {
                var key = Expression.Constant(ruleExpParams[i].ParameterExpression.Name);
                var value = Expression.Convert(ruleExpParams[i].ParameterExpression, typeof(object));
                variableExp.Add(ruleExpParams[i].ParameterExpression);
                body.Add(Expression.Call(dict, add, key, value));
            
            }
            // Return value
            body.Add(dict);

            return WrapExpression<Dictionary<string,object>>(body, paramExp.ToArray(), variableExp.ToArray());
        }

        private string GetCacheKey(string expression, RuleParameter[] ruleParameters, Type returnType)
        {
            var paramKey = string.Join("|", ruleParameters.Select(c => c.Name + "_" + c.Type.ToString()));
            var returnTypeKey = returnType?.ToString() ?? "null";
            var combined = $"Expression:{expression}-Params:{paramKey}-ReturnType:{returnTypeKey}";
            return combined;
        }
    }
}
