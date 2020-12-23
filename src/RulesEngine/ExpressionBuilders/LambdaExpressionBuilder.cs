// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;

namespace RulesEngine.ExpressionBuilders
{
    /// <summary>
    /// This class will build the list expression
    /// </summary>
    internal sealed class LambdaExpressionBuilder : RuleExpressionBuilderBase
    {
        private readonly ReSettings _reSettings;
        private readonly RuleExpressionParser _ruleExpressionParser;

        internal LambdaExpressionBuilder(ReSettings reSettings, RuleExpressionParser ruleExpressionParser)
        {
            _reSettings = reSettings;
            _ruleExpressionParser = ruleExpressionParser;
        }

        internal override RuleFunc<RuleResultTree> BuildDelegateForRule(Rule rule, RuleParameter[] ruleParams)
        {
            try
            {
                var ruleDelegate = _ruleExpressionParser.Compile<bool>(rule.Expression, ruleParams);
                bool func(object[] paramList) => ruleDelegate(paramList);
                return Helpers.ToResultTree(rule, null, func);
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(rule.RuleName), rule.RuleName);
                ex.Data.Add(nameof(rule.Expression), rule.Expression);

                if (!_reSettings.EnableExceptionAsErrorMessage) throw;
                bool func(object[] param) => false;
                var exceptionMessage = $"Exception while parsing expression `{rule?.Expression}` - {ex.Message}";
                return Helpers.ToResultTree(rule, null, func, exceptionMessage);
            }
        }
    }
}
