using System.Threading.Tasks;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public class OutputExpressionAction : ActionBase
    {
        private readonly ReSettings _reSettings;
        public OutputExpressionAction(ReSettings reSettings)
        {
            _reSettings = reSettings;
        }

        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var expression = context.GetContext<string>("expression");
            var parser = new RuleExpressionParser(_reSettings);
            return new ValueTask<object>(parser.Evaluate(expression, ruleParameters));
        }
    }
}
