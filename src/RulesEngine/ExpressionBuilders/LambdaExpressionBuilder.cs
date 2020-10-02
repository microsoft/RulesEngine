// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        private readonly RuleExpressionParser _ruleExpressionParser;

        internal LambdaExpressionBuilder(ReSettings reSettings)
        {
            _reSettings = reSettings;
            _ruleExpressionParser = new RuleExpressionParser(_reSettings);
        }

        internal override RuleFunc<RuleResultTree> BuildDelegateForRule(Rule rule, RuleParameter[] ruleParams)
        {
            try
            {
                var ruleDelegate = _ruleExpressionParser.Compile(rule.Expression, ruleParams,typeof(bool));
                bool func(object[] paramList) => (bool)ruleDelegate.DynamicInvoke(paramList);
                return Helpers.ToResultTree(rule, null, func);
            }
             catch (Exception ex)
            {
                bool func(object[] param) => false;
                var exceptionMessage = $"Exception while parsing expression `{rule?.Expression}` - ex.Message";
                return Helpers.ToResultTree(rule, null, func, exceptionMessage);
            }           
        }
    }
}
