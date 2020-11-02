using Microsoft.Extensions.Caching.Memory;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace RulesEngine.ExpressionBuilders
{
    public class RuleExpressionParser
    {
        private readonly ReSettings _reSettings;
        private static IMemoryCache _memoryCache;

        public RuleExpressionParser(ReSettings reSettings)
        {
            _reSettings = reSettings;
            _memoryCache = new MemoryCache(new MemoryCacheOptions{
                SizeLimit = 1000
            });
        }

        public Delegate Compile(string expression, RuleParameter[] ruleParams, Type returnType = null)
        {
            var cacheKey = GetCacheKey(expression,ruleParams,returnType);
            return _memoryCache.GetOrCreate(cacheKey,(entry) => {
                entry.SetSize(1);
                var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
                var typeParamExpressions = GetParameterExpression(ruleParams).ToArray();
                var e = DynamicExpressionParser.ParseLambda(config, true, typeParamExpressions.ToArray(), returnType, expression);
                return e.Compile();
            });
            
        }

        public object Evaluate(string expression, RuleParameter[] ruleParams, Type returnType = null)
        {
            var func = Compile(expression, ruleParams, returnType);
            return func.DynamicInvoke(ruleParams.Select(c => c.Value).ToArray());
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
        private IEnumerable<ParameterExpression> GetParameterExpression(params RuleParameter[] ruleParams)
        {
            foreach (var ruleParam in ruleParams)
            {
                if (ruleParam == null)
                {
                    throw new ArgumentException($"{nameof(ruleParam)} can't be null.");
                }

                yield return Expression.Parameter(ruleParam.Type, ruleParam.Name);
            }
        }

        private string GetCacheKey(string expression, RuleParameter[] ruleParameters,Type returnType){
            var paramKey = string.Join("|",ruleParameters.Select(c => c.Type.ToString()));
            var returnTypeKey = returnType?.ToString() ?? "null"; 
            var combined = $"Expression:{expression}-Params:{paramKey}-ReturnType:{returnTypeKey}";
            return combined;
        }
    }
}
