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

        public RuleExpressionParser(ReSettings reSettings)
        {
            _reSettings = reSettings;
        }

        public Delegate Compile(string expression, RuleParameter[] ruleParams, Type returnType = null)
        {
            var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
            var typeParamExpressions = GetParameterExpression(ruleParams).ToArray();
            var e = DynamicExpressionParser.ParseLambda(config, true, typeParamExpressions.ToArray(), returnType, expression);
            return e.Compile();
        }

        public object Evaluate(string expression, RuleParameter[] ruleParams, Type returnType = null)
        {
            var func = Compile(expression, ruleParams, returnType);
            return func.DynamicInvoke(ruleParams.Select(c => c.Value));
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
            if (ruleParams == null || !ruleParams.Any())
            {
                throw new ArgumentException($"{nameof(ruleParams)} can't be null/empty.");
            }

            foreach (var ruleParam in ruleParams)
            {
                if (ruleParam == null)
                {
                    throw new ArgumentException($"{nameof(ruleParam)} can't be null.");
                }

                yield return Expression.Parameter(ruleParam.Type, ruleParam.Name);
            }
        }
    }
}
