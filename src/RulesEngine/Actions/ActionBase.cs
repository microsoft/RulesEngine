using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public abstract class ActionBase
    {
        public abstract object Run(ActionContext context, RuleParameter[] ruleParameters);
    }
}
