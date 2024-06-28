// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System;

namespace RulesEngine
{
    internal class RuleExpressionBuilderFactory
    {
        private readonly ReSettings _reSettings;
        private readonly LambdaExpressionBuilder _lambdaExpressionBuilder;
        public RuleExpressionBuilderFactory(ReSettings reSettings, RuleExpressionParser expressionParser)
        {
            _reSettings = reSettings;
            _lambdaExpressionBuilder = new LambdaExpressionBuilder(_reSettings, expressionParser);
        }
        public RuleExpressionBuilderBase RuleGetExpressionBuilder(RuleExpressionType ruleExpressionType)
        {
            switch (ruleExpressionType)
            {
                case RuleExpressionType.LambdaExpression:
                    return _lambdaExpressionBuilder;
                default:
                    throw new InvalidOperationException($"{nameof(ruleExpressionType)} has not been supported yet.");
            }
        }
    }
}
