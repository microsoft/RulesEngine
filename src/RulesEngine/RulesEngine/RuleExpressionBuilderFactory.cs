// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Rules.ExpressionBuilders;
using Microsoft.Rules.Models;
using System;

namespace Microsoft.Rules
{
    internal class RuleExpressionBuilderFactory
    {
        private ReSettings _reSettings;
        public RuleExpressionBuilderFactory(ReSettings reSettings)
        {
            _reSettings = reSettings;
        }
        public RuleExpressionBuilderBase RuleGetExpressionBuilder(RuleExpressionType ruleExpressionType)
        {
            switch (ruleExpressionType)
            {
                case RuleExpressionType.LambdaExpression:
                    return new LambdaExpressionBuilder(_reSettings);
                default:
                    throw new InvalidOperationException($"{nameof(ruleExpressionType)} has not been supported yet.");
            }
        }
    }
}
