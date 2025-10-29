// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RulesEngine.Actions
{
    public class EvaluateRuleAction : ActionBase
    {
        private readonly RulesEngine _ruleEngine;
        private readonly RuleExpressionParser _ruleExpressionParser;

        public EvaluateRuleAction(RulesEngine ruleEngine, RuleExpressionParser ruleExpressionParser)
        {
            _ruleEngine = ruleEngine;
            _ruleExpressionParser = ruleExpressionParser;
        }

        internal async override ValueTask<ActionRuleResult> ExecuteAndReturnResultAsync(ActionContext context, RuleParameter[] ruleParameters, bool includeRuleResults = false)
        {
            var innerResult = await base.ExecuteAndReturnResultAsync(context, ruleParameters, includeRuleResults);
            var output = innerResult.Output as ActionRuleResult;
            List<RuleResultTree> resultList = null;
            if (includeRuleResults)
            {
                // Avoid exponential copying by only including the immediate results from this execution
                // The chained rule results are already included in output?.Results
                resultList = new List<RuleResultTree>();
                
                // Add the chained rule results (from the EvaluateRule action execution)
                if (output?.Results != null)
                {
                    resultList.AddRange(output.Results);
                }
                
                // Add the parent rule that triggered this chain (but avoid duplicating it)
                if (innerResult.Results != null)
                {
                    foreach (var result in innerResult.Results)
                    {
                        // Only add if it's not already in the output results to avoid duplication
                        if (output?.Results == null || !output.Results.Any(r => ReferenceEquals(r, result)))
                        {
                            resultList.Add(result);
                        }
                    }
                }
            }
            return new ActionRuleResult {
                Output = output?.Output,
                Exception = innerResult.Exception,
                Results = resultList
            };
        }

        public async override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var workflowName = context.GetContext<string>("workflowName");
            var ruleName = context.GetContext<string>("ruleName");
            var filteredRuleParameters = new List<RuleParameter>(ruleParameters);
            if(context.TryGetContext<List<string>>("inputFilter",out var inputFilter))
            {
                filteredRuleParameters = ruleParameters.Where(c => inputFilter.Contains(c.Name)).ToList();
            }
            if (context.TryGetContext<List<ScopedParam>>("additionalInputs", out var additionalInputs))
            {
                foreach(var additionalInput in additionalInputs)
                {
                    dynamic value = _ruleExpressionParser.Evaluate<object>(additionalInput.Expression, ruleParameters);
                    filteredRuleParameters.Add(new RuleParameter(additionalInput.Name, value));
                    
                }
            }
   
            var ruleResult = await _ruleEngine.ExecuteActionWorkflowAsync(workflowName, ruleName, filteredRuleParameters.ToArray());
            return ruleResult;
        }
    }
}
