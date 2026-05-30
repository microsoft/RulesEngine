// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue573Test
    {
        // Demonstrates the correct way to pass computed additionalInputs into an EvaluateRule
        // action. The target rule can reference the additionalInput by its Name. The key detail
        // (the source of the "Unknown identifier" confusion in #573) is that the additionalInput
        // Name must match the identifier used in the target rule's expression.
        [Fact]
        public async Task EvaluateRuleAction_AdditionalInputs_AreAvailableToTargetRule()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[]
                {
                    new Rule
                    {
                        RuleName = "Parent",
                        Expression = "input1.Value > 0",
                        Actions = new RuleActions
                        {
                            OnSuccess = new ActionInfo
                            {
                                Name = "EvaluateRule",
                                Context = new Dictionary<string, object>
                                {
                                    { "workflowName", "wf" },
                                    { "ruleName", "Child" },
                                    // Compute a new input named "doubled" from input1 and pass it on.
                                    { "additionalInputs", new List<ScopedParam>
                                        {
                                            new ScopedParam { Name = "doubled", Expression = "input1.Value * 2" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Rule
                    {
                        RuleName = "Child",
                        // References the additionalInput by name.
                        Expression = "doubled == 20"
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });
            var result = await engine.ExecuteActionWorkflowAsync("wf", "Parent",
                new[] { RuleParameter.Create("input1", new { Value = 10 }) });

            // Child rule succeeded because "doubled" (10*2=20) was available to it.
            Assert.NotNull(result.Results);
            Assert.Contains(result.Results, r => r.Rule.RuleName == "Child" && r.IsSuccess);
        }
    }
}
