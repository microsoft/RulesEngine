// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using FastExpressionCompiler;
using Microsoft.Extensions.Caching.Memory;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace RulesEngine.ExpressionBuilders
{
    public class RuleExpressionParser
    {
        private readonly ReSettings _reSettings;
        private static IMemoryCache _memoryCache;

        public RuleExpressionParser(ReSettings reSettings)
        {
            _reSettings = reSettings;
            _memoryCache = new MemoryCache(new MemoryCacheOptions {
                SizeLimit = 1000
            });
        }

        public LambdaExpression Parse(string expression, ParameterExpression[] parameters, Type returnType)
        {
            var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };

            return DynamicExpressionParser.ParseLambda(config, false, parameters, returnType, expression);
        }

        public Func<object[], T> Compile<T>(string expression, RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams)
        {
            ruleExpParams = ruleExpParams ?? new RuleExpressionParameter[] { };
            var cacheKey = GetCacheKey(expression, ruleParams, typeof(T));
            return _memoryCache.GetOrCreate(cacheKey, (entry) => {
                entry.SetSize(1);
                var parameterExpressions = GetParameterExpression(ruleParams).ToArray();
                var extendedParamExpressions = ruleExpParams.Select(c => c.ParameterExpression).ToArray();
                var e = Parse(expression, parameterExpressions.Concat(extendedParamExpressions).ToArray(), typeof(T));
                var expressionBody = CreateAssignedParameterExpression(ruleExpParams).ToList();
                expressionBody.Add(e.Body);
                var wrappedExpression = WrapExpression<T>(expressionBody, parameterExpressions, extendedParamExpressions);
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


        public T Evaluate<T>(string expression, RuleParameter[] ruleParams, RuleExpressionParameter[] ruleExpParams = null)
        {
            var func = Compile<T>(expression, ruleParams, ruleExpParams);
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

        private string GetCacheKey(string expression, RuleParameter[] ruleParameters, Type returnType)
        {
            var paramKey = string.Join("|", ruleParameters.Select(c => c.Type.ToString()));
            var returnTypeKey = returnType?.ToString() ?? "null";
            var combined = $"Expression:{expression}-Params:{paramKey}-ReturnType:{returnTypeKey}";
            return combined;
        }
    }
}
