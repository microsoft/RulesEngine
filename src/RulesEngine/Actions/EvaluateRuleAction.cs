using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public class EvaluateRuleAction : ActionBase
    {
        private readonly ReSettings _reSettings;
        private readonly RulesEngine _ruleEngine;

        public EvaluateRuleAction(ReSettings reSettings,RulesEngine ruleEngine)
        {
            _reSettings = reSettings;
            _ruleEngine = ruleEngine;
        }

        internal override async ValueTask<ActionRuleResult> ExecuteAndReturnResultAsync(ActionContext context, RuleParameter[] ruleParameters){
            var output = await Run(context,ruleParameters) as ActionRuleResult;
            var parentResult = context.GetParentRuleResult();

            var resultList = new List<RuleResultTree>(){
                parentResult
            };
            resultList.AddRange(output.Results);

            return new ActionRuleResult{
                Output = output.Output,
                Results = resultList
            };

        }

        public override async ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var workflowName = context.GetContext<string>("workflowName");
            var ruleName = context.GetContext<string>("ruleName");
            var ruleResult = await _ruleEngine.ExecuteActionWorkflowAsync(workflowName,ruleName,ruleParameters);
            return ruleResult;
        }
    }
}
