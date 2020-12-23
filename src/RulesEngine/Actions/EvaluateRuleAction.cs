// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RulesEngine.Actions
{
    public class EvaluateRuleAction : ActionBase
    {
        private readonly RulesEngine _ruleEngine;

        public EvaluateRuleAction(RulesEngine ruleEngine)
        {
            _ruleEngine = ruleEngine;
        }

        internal async override ValueTask<ActionRuleResult> ExecuteAndReturnResultAsync(ActionContext context, RuleParameter[] ruleParameters, bool includeRuleResults = false)
        {
            var innerResult = await base.ExecuteAndReturnResultAsync(context, ruleParameters, includeRuleResults);
            var output = innerResult.Output as ActionRuleResult;
            List<RuleResultTree> resultList = null;
            if (includeRuleResults)
            {
                resultList = new List<RuleResultTree>(output.Results);
                resultList.AddRange(innerResult.Results);
            }
            return new ActionRuleResult {
                Output = output.Output,
                Exception = innerResult.Exception,
                Results = resultList
            };
        }

        public async override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var workflowName = context.GetContext<string>("workflowName");
            var ruleName = context.GetContext<string>("ruleName");
            var ruleResult = await _ruleEngine.ExecuteActionWorkflowAsync(workflowName, ruleName, ruleParameters);
            return ruleResult;
        }
    }
}
