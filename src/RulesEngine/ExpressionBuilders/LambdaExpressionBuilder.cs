// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using RulesEngine.ExpressionBuilders;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace RulesEngine.ExpressionBuilders
{
    /// <summary>
    /// This class will build the list expression
    /// </summary>
    internal sealed class LambdaExpressionBuilder : RuleExpressionBuilderBase
    {
        private readonly ReSettings _reSettings;

        internal LambdaExpressionBuilder(ReSettings reSettings)
        {
            _reSettings = reSettings;
        }
        internal override RuleFunc<RuleResultTree> BuildExpressionForRule(Rule rule, IEnumerable<ParameterExpression> typeParamExpressions)
        {
            try
            {
                var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
                var e = DynamicExpressionParser.ParseLambda(config, true, typeParamExpressions.ToArray(),typeof(bool), rule.Expression);
                var ruleDelegate = e.Compile();
                bool func(object[] paramList) => (bool)ruleDelegate.DynamicInvoke(paramList);
                return Helpers.ToResultTree(rule, null, func);
            }
             catch (Exception ex)
            {
                bool func(object[] param) => false;
                var exceptionMessage = ex.Message;
                return Helpers.ToResultTree(rule, null, func, exceptionMessage);
            }           
        }

        /// <summary>Builds the expression for rule parameter.</summary>
        /// <param name="param">The parameter.</param>
        /// <param name="typeParamExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns>Expression.</returns>
        internal override Expression BuildExpressionForRuleParam(LocalParam param, IEnumerable<ParameterExpression> typeParamExpressions)
        {
            var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
            var e = DynamicExpressionParser.ParseLambda(config, typeParamExpressions.ToArray(), null, param.Expression);
            return e.Body;
        }

    }
}
