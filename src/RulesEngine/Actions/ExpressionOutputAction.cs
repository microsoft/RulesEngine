using System.Collections.Generic;
using System.Threading.Tasks;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;

namespace RulesEngine.Actions
{
    public class OutputExpressionAction : ActionBase
    {
        private readonly RuleExpressionParser _ruleExpressionParser;

        public OutputExpressionAction(RuleExpressionParser ruleExpressionParser)
        {
            _ruleExpressionParser = ruleExpressionParser;
        }

        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var expression = context.GetContext<string>("expression");
            return new ValueTask<object>(_ruleExpressionParser.Evaluate<object>(expression, ruleParameters));
        }
    }
}
