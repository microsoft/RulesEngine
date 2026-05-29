// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System;
using System.Linq.Dynamic.Core.Exceptions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RulesEngine.Actions
{
    public class OutputExpressionAction : ActionBase
    {
        private static readonly Regex CSharpAnonymousObjectPattern =
            new Regex(@"\bnew\s*\{", RegexOptions.Compiled);

        private readonly RuleExpressionParser _ruleExpressionParser;

        public OutputExpressionAction(RuleExpressionParser ruleExpressionParser)
        {
            _ruleExpressionParser = ruleExpressionParser;
        }

        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var expression = context.GetContext<string>("expression");
            try
            {
                return new ValueTask<object>(_ruleExpressionParser.Evaluate<object>(expression, ruleParameters));
            }
            catch (ParseException ex) when (CSharpAnonymousObjectPattern.IsMatch(expression ?? string.Empty))
            {
                throw new ParseException(
                    "OutputExpression failed to parse. It looks like the expression uses C#-style anonymous-object syntax " +
                    "(`new { Name = value, ... }`), which is not supported by System.Linq.Dynamic.Core. " +
                    "Use the Dynamic.Core form instead: `new (value as Name, ...)` — parentheses, and each field needs an `as Alias`. " +
                    "Original parser error: " + ex.Message,
                    ex.Position);
            }
        }
    }
}
