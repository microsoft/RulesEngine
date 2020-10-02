using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public abstract class ActionBase
    {
        internal virtual async ValueTask<ActionRuleResult> ExecuteAndReturnResultAsync(ActionContext context, RuleParameter[] ruleParameters){
            ActionRuleResult result = new ActionRuleResult();
            try
            {
                result.Output = await Run(context, ruleParameters);
            }
            catch(Exception ex)
            {
                result.Exception = new Exception($"Exception while executing {this.GetType().Name}: {ex.Message}",ex);
            }
            finally
            {
                result.Results = new List<RuleResultTree>()
                {
                    context.GetParentRuleResult()
                };
            }
            return result;
        }
        public abstract ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters);
    }
}
