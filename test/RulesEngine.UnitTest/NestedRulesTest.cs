﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Models;
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
    public class NestedRulesTest
    {

        [Theory]
        [InlineData(NestedRuleExecutionMode.All)]
        [InlineData(NestedRuleExecutionMode.Performance)]
        public async Task NestedRulesShouldFollowExecutionMode(NestedRuleExecutionMode mode)
        {
            var workflows = GetWorkflows();
            var reSettings = new ReSetting { NestedRuleExecutionMode = mode };
            var rulesEngine = new RulesEngine(workflows, reSettings: reSettings);
            dynamic input1 = new ExpandoObject();
            input1.trueValue = true;

            List<RuleResultTree> result = await rulesEngine.ExecuteAllRulesAsync("NestedRulesTest", input1);
            var andResults = result.Where(c => c.Rule.Operator == "And").ToList();
            var orResults = result.Where(c => c.Rule.Operator == "Or").ToList();
            Assert.All(andResults,
                c => Assert.False(c.IsSuccess)
                );
            Assert.All(orResults,
                c => Assert.True(c.IsSuccess));

            if (mode == NestedRuleExecutionMode.All)
            {
                Assert.All(andResults,
                    c => Assert.Equal(c.Rule.Rules.Count(), c.ChildResults.Count()));
                Assert.All(orResults,
                    c => Assert.Equal(c.Rule.Rules.Count(), c.ChildResults.Count()));
            }
            else if (mode == NestedRuleExecutionMode.Performance)
            {
                Assert.All(andResults,
                    c => {
                        Assert.Equal(c.IsSuccess, c.ChildResults.Last().IsSuccess);
                        Assert.Single(c.ChildResults.Where(d => c.IsSuccess == d.IsSuccess));
                        Assert.True(c.ChildResults.SkipLast(1).All(d => d.IsSuccess == true));
                    });

                Assert.All(orResults,
                    c => {
                        Assert.Equal(c.IsSuccess, c.ChildResults.Last().IsSuccess);
                        Assert.Single(c.ChildResults.Where(d => c.IsSuccess == d.IsSuccess));
                        Assert.True(c.ChildResults.SkipLast(1).All(d => d.IsSuccess == false));
                    });

            }


        }

        [Fact]
        private async Task NestedRulesWithNestedActions_ReturnsCorrectResults()
        {
            var workflows = GetWorkflows();
            var reSettings = new ReSetting { };
            var rulesEngine = new RulesEngine(workflows, reSettings: reSettings);
            dynamic input1 = new ExpandoObject();
            input1.trueValue = true;

            List<RuleResultTree> result = await rulesEngine.ExecuteAllRulesAsync("NestedRulesActionsTest", input1);

            Assert.False(result[0].IsSuccess);
            Assert.Equal(input1.trueValue, result[0].ActionResult.Output);
            Assert.All(result[0].ChildResults, (childResult) => Assert.Equal(input1.trueValue, childResult.ActionResult.Output));
        }

        [Fact]
        private async Task NestedRulesWithNestedActions_WorkflowParsedWithSystemTextJson_ReturnsCorrectResults()
        {
            var workflows = GetWorkflows();
            var workflowStr = JsonConvert.SerializeObject(workflows);

            var serializationOptions = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };


            var workflowsViaTextJson = System.Text.Json.JsonSerializer.Deserialize<WorkflowRule[]>(workflowStr, serializationOptions);

            var reSettings = new ReSetting { };
            var rulesEngine = new RulesEngine(workflowsViaTextJson, reSettings: reSettings);
            dynamic input1 = new ExpandoObject();
            input1.trueValue = true;

            List<RuleResultTree> result = await rulesEngine.ExecuteAllRulesAsync("NestedRulesActionsTest", input1);

            Assert.False(result[0].IsSuccess);
            Assert.Equal(input1.trueValue, result[0].ActionResult.Output);
            Assert.All(result[0].ChildResults, (childResult) => Assert.Equal(input1.trueValue, childResult.ActionResult.Output));


        }



        private WorkflowRule[] GetWorkflows()
        {
            return new[] {
                new WorkflowRule {
                    WorkflowName = "NestedRulesTest",
                    Rules = new Rule[] {
                        new Rule {
                            RuleName = "AndRuleTrueFalse",
                            Operator = "And",
                            Rules = new Rule[] {
                                new Rule{
                                    RuleName = "trueRule1",
                                    Expression = "input1.TrueValue == true",
                                },
                                new Rule {
                                    RuleName = "falseRule1",
                                    Expression = "input1.TrueValue == false"
                                }

                            }
                        },
                        new Rule {
                            RuleName = "OrRuleTrueFalse",
                            Operator = "Or",
                            Rules = new Rule[] {
                                new Rule{
                                    RuleName = "trueRule2",
                                    Expression = "input1.TrueValue == true",
                                },
                                new Rule {
                                    RuleName = "falseRule2",
                                    Expression = "input1.TrueValue == false"
                                }

                            }
                        },
                        new Rule {
                            RuleName = "AndRuleFalseTrue",
                            Operator = "And",
                            Rules = new Rule[] {
                                new Rule{
                                    RuleName = "trueRule3",
                                    Expression = "input1.TrueValue == false",
                                },
                                new Rule {
                                    RuleName = "falseRule4",
                                    Expression = "input1.TrueValue == true"
                                }

                            }
                        },
                         new Rule {
                            RuleName = "OrRuleFalseTrue",
                            Operator = "Or",
                            Rules = new Rule[] {
                                new Rule{
                                    RuleName = "trueRule3",
                                    Expression = "input1.TrueValue == false",
                                },
                                new Rule {
                                    RuleName = "falseRule4",
                                    Expression = "input1.TrueValue == true"
                                }

                            }
                         }
                    }
                },
                new WorkflowRule {
                    WorkflowName = "NestedRulesActionsTest",
                    Rules = new Rule[] {
                        new Rule {
                            RuleName = "AndRuleTrueFalse",
                            Operator = "And",
                            Rules = new Rule[] {
                                new Rule{
                                    RuleName = "trueRule1",
                                    Expression = "input1.TrueValue == true",
                                    Actions =  new RuleAction {
                                        OnSuccess = new ActionInfo{
                                            Name = "OutputExpression",
                                            Context = new Dictionary<string, object> {
                                                { "Expression", "input1.TrueValue" }
                                            }
                                        }
                                    }
                                },
                                new Rule {
                                    RuleName = "falseRule1",
                                    Expression = "input1.TrueValue == false",
                                    Actions =  new RuleAction {
                                        OnFailure = new ActionInfo{
                                            Name = "OutputExpression",
                                            Context = new Dictionary<string, object> {
                                                { "Expression", "input1.TrueValue" }
                                            }
                                        }
                                    }
                                }
                            },
                            Actions =  new RuleAction {
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
