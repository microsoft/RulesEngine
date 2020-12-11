using Microsoft.Extensions.Caching.Memory;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using FastExpressionCompiler;

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

        public Func<object[],T> Compile<T>(string expression, RuleParameter[] ruleParams)
        {
            var cacheKey = GetCacheKey(expression,ruleParams,typeof(T));
            return _memoryCache.GetOrCreate(cacheKey,(entry) => {
                entry.SetSize(1);
                var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
                var typeParamExpressions = GetParameterExpression(ruleParams).ToArray();
                var e = DynamicExpressionParser.ParseLambda(config, true, typeParamExpressions.ToArray(), typeof(T), expression);
                var wrappedExpression = WrapExpression<T>(e,typeParamExpressions);
                return wrappedExpression.CompileFast<Func<object[],T>>();
            });
            
        }

        private Expression<Func<object[],T>> WrapExpression<T>(LambdaExpression expression, ParameterExpression[] parameters){
            var argExp = Expression.Parameter(typeof(object[]),"args");
            IEnumerable<Expression> paramExps = parameters.Select((c, i) => {
                var arg = Expression.ArrayAccess(argExp, Expression.Constant(i));
                return (Expression)Expression.Assign(c, Expression.Convert(arg, c.Type));
            });
            var blockExpSteps = paramExps.Concat(new List<Expression> { expression.Body });
            var blockExp = Expression.Block(parameters, blockExpSteps);
            return Expression.Lambda<Func<object[],T>>(blockExp, argExp);
        }


        public T Evaluate<T>(string expression, RuleParameter[] ruleParams)
        {
            var func = Compile<T>(expression, ruleParams);
            return func(ruleParams.Select(c => c.Value).ToArray());
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
