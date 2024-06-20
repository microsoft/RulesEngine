// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Exceptions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest;

[ExcludeFromCodeCoverage]
public class EmptyRulesTest
{
    [Fact]
    private async Task EmptyRules_ReturnsExepectedResults()
    {
        var workflow = GetEmptyWorkflow();
        var reSettings = new ReSettings();
        var rulesEngine = new RulesEngine();

        var action = () => {
            new RulesEngine(workflow, reSettings);
            return Task.CompletedTask;
        };

        Exception ex = await Assert.ThrowsAsync<RuleValidationException>(action);

        Assert.Contains("Atleast one of Rules or WorkflowsToInject must be not empty", ex.Message);
    }

    [Fact]
    private async Task NestedRulesWithEmptyNestedActions_ReturnsExepectedResults()
    {
        var workflow = GetEmptyNestedWorkflows();
        var reSettings = new ReSettings();
        var rulesEngine = new RulesEngine();

        var action = () => {
            new RulesEngine(workflow, reSettings);
            return Task.CompletedTask;
        };

        Exception ex = await Assert.ThrowsAsync<RuleValidationException>(action);

        Assert.Contains("Atleast one of Rules or WorkflowsToInject must be not empty", ex.Message);
    }

    private Workflow[] GetEmptyWorkflow()
    {
        return new[] { new Workflow { WorkflowName = "EmptyRulesTest", Rules = new Rule[] { } } };
    }

    private Workflow[] GetEmptyNestedWorkflows()
    {
        return new[] {
            new Workflow {
                WorkflowName = "EmptyNestedRulesTest",
                Rules = new Rule[] {
                    new() {
                        RuleName = "AndRuleTrueFalse",
                        Operator = "And",
                        Rules =
                            new Rule[] {
                                new() { RuleName = "trueRule1", Expression = "input1.TrueValue == true" },
                                new() { RuleName = "falseRule1", Expression = "input1.TrueValue == false" }
                            }
                    },
                    new() {
                        RuleName = "OrRuleTrueFalse",
                        Operator = "Or",
                        Rules =
                            new Rule[] {
                                new() { RuleName = "trueRule2", Expression = "input1.TrueValue == true" },
                                new() { RuleName = "falseRule2", Expression = "input1.TrueValue == false" }
                            }
                    },
                    new() {
                        RuleName = "AndRuleFalseTrue",
                        Operator = "And",
                        Rules =
                            new Rule[] {
                                new() { RuleName = "trueRule3", Expression = "input1.TrueValue == false" },
                                new() { RuleName = "falseRule4", Expression = "input1.TrueValue == true" }
                            }
                    },
                    new() {
                        RuleName = "OrRuleFalseTrue",
                        Operator = "Or",
                        Rules = new Rule[] {
                            new() { RuleName = "trueRule3", Expression = "input1.TrueValue == false" },
                            new() { RuleName = "falseRule4", Expression = "input1.TrueValue == true" }
                        }
                    }
                }
            },
            new Workflow {
                WorkflowName = "EmptyNestedRulesActionsTest",
                Rules = new Rule[] {
                    new() {
                        RuleName = "AndRuleTrueFalse",
                        Operator = "And",
                        Rules = new Rule[] { },
                        Actions = new RuleActions {
                            OnFailure = new ActionInfo {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> { { "Expression", "input1.TrueValue" } }
                            }
                        }
                    }
                }
            }
        };
    }
}