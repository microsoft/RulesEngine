using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public abstract class ActionBase
    {
        internal virtual async ValueTask<ActionRuleResult> ExecuteAndReturnResultAsync(ActionContext context, RuleParameter[] ruleParameters){
            var output = await Run(context,ruleParameters);
            return new ActionRuleResult{
                Output = output,
                Results = new List<RuleResultTree>{
                    context.GetParentRuleResult()
                }
            };
        }
        public abstract ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters);
    }
}
