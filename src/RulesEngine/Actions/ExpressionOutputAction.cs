using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public class ExpressionOutputAction : ActionBase
    {
        private readonly ReSettings _reSettings;
        public ExpressionOutputAction(ReSettings reSettings)
        {
            _reSettings = reSettings;
        }

        public override object Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var expression = context.GetContext<string>("expression");
            var parser = new RuleExpressionParser(_reSettings);
            return parser.Evaluate(expression, ruleParameters);
        }
    }
}
