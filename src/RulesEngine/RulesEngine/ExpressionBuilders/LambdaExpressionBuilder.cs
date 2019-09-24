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
        internal override Expression<Func<RuleInput, RuleResultTree>> BuildExpressionForRule(Rule rule, IEnumerable<ParameterExpression> typeParamExpressions, ParameterExpression ruleInputExp)
        {
            try
            {
                var config = new ParsingConfig { CustomTypeProvider = new CustomTypeProvider(_reSettings.CustomTypes) };
                var e = DynamicExpressionParser.ParseLambda(config, typeParamExpressions.ToArray(), null, rule.Expression);
                var body = (BinaryExpression)e.Body;
                return Helpers.ToResultTreeExpression(rule, null, body, typeParamExpressions, ruleInputExp);
            }
            catch (Exception ex)
            {
                var binaryExpression = Expression.And(Expression.Constant(true), Expression.Constant(false));
                var exceptionMessage = ex.Message;
                return Helpers.ToResultTreeExpression(rule, null, binaryExpression, typeParamExpressions, ruleInputExp, exceptionMessage);
            }           
        }
    }
}
