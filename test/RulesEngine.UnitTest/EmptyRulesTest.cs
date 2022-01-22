// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class EmptyRulesTest
    {
        [Fact]
        private async Task EmptyRules_ReturnsExepectedResults()
        {
            var workflow = GetEmptyWorkflow();
            var reSettings = new ReSettings { };
            RulesEngine rulesEngine = new RulesEngine();

            Func<Task> action = () => {
                new RulesEngine(workflow, reSettings: reSettings);
                return Task.CompletedTask;
            };

            Exception ex = await Assert.ThrowsAsync<Exceptions.RuleValidationException>(action);

            Assert.Contains("Atleast one of Rules or WorkflowsToInject must be not empty", ex.Message);
        }
        [Fact]
        private async Task NestedRulesWithEmptyNestedActions_ReturnsExepectedResults()
        {
            var workflow = GetEmptyNestedWorkflows();
            var reSettings = new ReSettings { };
            RulesEngine rulesEngine = new RulesEngine();

            Func<Task> action = () => {
                new RulesEngine(workflow, reSettings: reSettings);
                return Task.CompletedTask;
            };

            Exception ex = await Assert.ThrowsAsync<Exceptions.RuleValidationException>(action);

            Assert.Contains("Atleast one of Rules or WorkflowsToInject must be not empty", ex.Message);
        }

        private Workflow[] GetEmptyWorkflow()
        {
            return new[] {
                new Workflow {
                    Name = "EmptyRulesTest",
                    Rules = new Rule[] {
                    }
                }
            };
        }

        private Workflow[] GetEmptyNestedWorkflows()
        {
            return new[] {
                new Workflow {
                    Name = "EmptyNestedRulesTest",
                    Rules = new Rule[] {
                        new Rule {
                            Name = "AndRuleTrueFalse",
                            Operator = "And",
                            Rules = new Rule[] {
                                new Rule{
                                    Name = "trueRule1",
                                    Expression = "input1.TrueValue == true",
                                },
                                new Rule {
                                    Name = "falseRule1",
                                    Expression = "input1.TrueValue == false"
                                }

                            }
                        },
                        new Rule {
                            Name = "OrRuleTrueFalse",
                            Operator = "Or",
                            Rules = new Rule[] {
                                new Rule{
                                    Name = "trueRule2",
                                    Expression = "input1.TrueValue == true",
                                },
                                new Rule {
                                    Name = "falseRule2",
                                    Expression = "input1.TrueValue == false"
                                }

                            }
                        },
                        new Rule {
                            Name = "AndRuleFalseTrue",
                            Operator = "And",
                            Rules = new Rule[] {
                                new Rule{
                                    Name = "trueRule3",
                                    Expression = "input1.TrueValue == false",
                                },
                                new Rule {
                                    Name = "falseRule4",
                                    Expression = "input1.TrueValue == true"
                                }

                            }
                        },
                         new Rule {
                            Name = "OrRuleFalseTrue",
                            Operator = "Or",
                            Rules = new Rule[] {
                                new Rule{
                                    Name = "trueRule3",
                                    Expression = "input1.TrueValue == false",
                                },
                                new Rule {
                                    Name = "falseRule4",
                                    Expression = "input1.TrueValue == true"
                                }

                            }
                         }
                    }
                },
                new Workflow {
                    Name = "EmptyNestedRulesActionsTest",
                    Rules = new Rule[] {
                        new Rule {
                            Name = "AndRuleTrueFalse",
                            Operator = "And",
                            Rules = new Rule[] {

                            },
                            Actions =  new RuleActions {
                                        OnFailure = new ActionInfo{
                                            Name = "OutputExpression",
                                            Context = new Dictionary<string, object> {
                                                { "Expression", "input1.TrueValue" }
                                            }
                                        }
                                    }
                        }
                    }
                }

            };
        }
    }
}
