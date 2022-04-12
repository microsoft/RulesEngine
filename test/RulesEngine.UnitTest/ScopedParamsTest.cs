// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class ScopedParamsTest
    {
        [Theory]
        [InlineData("NoLocalAndGlobalParams")]
        [InlineData("LocalParamsOnly")]
        [InlineData("GlobalParamsOnly")]
        [InlineData("GlobalAndLocalParams")]
        [InlineData("GlobalParamReferencedInLocalParams")]
        [InlineData("GlobalParamReferencedInNextGlobalParams")]
        [InlineData("LocalParamReferencedInNextLocalParams")]
        [InlineData("GlobalParamAndLocalParamsInNestedRules")]
        public async Task BasicWorkflows_ReturnsTrue(string workflowName)
        {
            var workflow = GetWorkflowList();

            var engine = new RulesEngine();
            engine.AddWorkflow(workflow);

            var input1 = new {
                trueValue = true,
                falseValue = false
            };

            var result = await engine.ExecuteAllRulesAsync(workflowName, input1);
            Assert.True(result.All(c => c.IsSuccess));

            CheckResultTreeContainsAllInputs(workflowName, result);

        }

        [Theory]
        [InlineData("GlobalAndLocalParams")]
        public async Task WorkflowUpdate_GlobalParam_ShouldReflect(string workflowName)
        {
            var workflow = GetWorkflowList();

            var engine = new RulesEngine();
            engine.AddWorkflow(workflow);

            var input1 = new {
                trueValue = true,
                falseValue = false
            };

            var result = await engine.ExecuteAllRulesAsync(workflowName, input1);
            Assert.True(result.All(c => c.IsSuccess));

            var workflowToUpdate = workflow.Single(c => c.WorkflowName == workflowName);
            engine.RemoveWorkflow(workflowName);
            workflowToUpdate.GlobalParams.First().Expression = "true == false";
            engine.AddWorkflow(workflowToUpdate);

            var result2 = await engine.ExecuteAllRulesAsync(workflowName, input1);

            Assert.True(result2.All(c => c.IsSuccess == false));
        }


        [Theory]
        [InlineData("GlobalParamsOnly", new[] { false })]
        [InlineData("LocalParamsOnly", new[] { false, true })]
        [InlineData("GlobalAndLocalParams", new[] { false })]
        public async Task DisabledScopedParam_ShouldReflect(string workflowName, bool[] outputs)
        {
            var workflow = GetWorkflowList();

            var engine = new RulesEngine(new string[] { }, new ReSettings {
                EnableScopedParams = false
            });
            engine.AddWorkflow(workflow);

            var input1 = new {
                trueValue = true,
                falseValue = false
            };

            var result = await engine.ExecuteAllRulesAsync(workflowName, input1);
            for (var i = 0; i < result.Count; i++)
            {
                Assert.Equal(result[i].IsSuccess, outputs[i]);
                if (result[i].IsSuccess == false)
                {
                    Assert.StartsWith("Exception while parsing expression", result[i].ExceptionMessage);
                }
            }
        }

        [Theory]
        [InlineData("GlobalParamsOnly")]
        [InlineData("LocalParamsOnly2")]
        public async Task ErrorInScopedParam_ShouldAppearAsErrorMessage(string workflowName)
        {
            var workflow = GetWorkflowList();

            var engine = new RulesEngine(new string[] { }, null);
            engine.AddWorkflow(workflow);

            var input = new { };
            var result = await engine.ExecuteAllRulesAsync(workflowName, input);

            Assert.All(result, c => { 
                Assert.False(c.IsSuccess);
                Assert.StartsWith("Error while compiling rule", c.ExceptionMessage);
            });

        }

        [Theory]
        [InlineData("GlobalParamsOnlyWithComplexInput")]
        [InlineData("LocalParamsOnlyWithComplexInput")]
        public async Task RuntimeErrorInScopedParam_ShouldAppearAsErrorMessage(string workflowName)
        {
            var workflow = GetWorkflowList();

            var engine = new RulesEngine(new string[] { }, null);
            engine.AddWorkflow(workflow);



            var input = new RuleTestClass();
            var result = await engine.ExecuteAllRulesAsync(workflowName, input);

            Assert.All(result, c => {
                Assert.False(c.IsSuccess);
                Assert.StartsWith("Error while executing scoped params for rule", c.ExceptionMessage);
            });


        }

        private void CheckResultTreeContainsAllInputs(string workflowName, List<RuleResultTree> result)
        {
            var workflow = GetWorkflowList().Single(c => c.WorkflowName == workflowName);
            var expectedInputs = new List<string>() { "input1" };
            expectedInputs.AddRange(workflow.GlobalParams?.Select(c => c.Name) ?? new List<string>());


            foreach (var resultTree in result)
            {
                CheckInputs(expectedInputs, resultTree);
            }

        }

        private static void CheckInputs(IEnumerable<string> expectedInputs, RuleResultTree resultTree)
        {
            Assert.All(expectedInputs, input => Assert.True(resultTree.Inputs.ContainsKey(input)));

            var localParamNames = resultTree.Rule.LocalParams?.Select(c => c.Name) ?? new List<string>();
            Assert.All(localParamNames, input => Assert.True(resultTree.Inputs.ContainsKey(input)));

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.All(localParamNames, lp => Assert.Contains(resultTree.RuleEvaluatedParams, c => c.Name == lp));
#pragma warning restore CS0618 // Type or member is obsolete

            if (resultTree.ChildResults?.Any() == true)
            {
                foreach (var childResultTree in resultTree.ChildResults)
                {
                    CheckInputs(expectedInputs.Concat(localParamNames), childResultTree);
                }

            }

        }
        private Workflow[] GetWorkflowList()
        {
            return new Workflow[] {
                new Workflow {
                    WorkflowName = "NoLocalAndGlobalParams",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "TruthTest",
                            Expression = "input1.trueValue"
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "LocalParamsOnly",
                    Rules = new List<Rule> {
                        new Rule {

                            RuleName = "WithLocalParam",
                            LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = "input1.trueValue"
                                }
                            },
                            Expression = "localParam1 == true"
                        },
                        new Rule {

                            RuleName = "WithoutLocalParam",
                            Expression = "input1.falseValue == false"
                        },
                    }
                },
                new Workflow {
                    WorkflowName = "LocalParamsOnly2",
                    Rules = new List<Rule> {
                        new Rule {

                            RuleName = "WithLocalParam",
                            LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = "input1.trueValue"
                                }
                            },
                            Expression = "localParam1 == true"
                        }
                    }
                },

                new Workflow {
                    WorkflowName = "GlobalParamsOnly",
                    GlobalParams = new List<ScopedParam> {
                        new ScopedParam {
                            Name = "globalParam1",
                            Expression = "input1.falseValue == false"
                        }
                    },
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "TrueTest",
                            Expression = "globalParam1 == true"
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "GlobalAndLocalParams",
                    GlobalParams = new List<ScopedParam> {
                        new ScopedParam {
                            Name = "globalParam1",
                            Expression = "input1.falseValue == false"
                        }
                    },
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "WithLocalParam",
                            LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = "input1.trueValue"
                                }
                            },
                            Expression = "globalParam1 == true && localParam1 == true"
                        },
                    }

                },
                new Workflow {
                    WorkflowName = "GlobalParamReferencedInLocalParams",
                    GlobalParams = new List<ScopedParam> {
                        new ScopedParam {
                            Name = "globalParam1",
                            Expression = "\"testString\""
                        }
                    },
                    Rules = new List<Rule> {
                        new Rule {

                            RuleName = "WithLocalParam",
                            LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = "globalParam1.ToUpper()"
                                }
                            },
                            Expression = "globalParam1 == \"testString\" && localParam1 == \"TESTSTRING\""
                        },
                    }
                },
                new Workflow {
                    WorkflowName = "GlobalParamReferencedInNextGlobalParams",
                    GlobalParams = new List<ScopedParam> {
                        new ScopedParam {
                            Name = "globalParam1",
                            Expression = "\"testString\""
                        },
                        new ScopedParam {
                            Name = "globalParam2",
                            Expression = "globalParam1.ToUpper()"
                        }
                    },
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "WithLocalParam",
                            Expression = "globalParam1 == \"testString\" && globalParam2 == \"TESTSTRING\""
                        },
                    }
                },
                new Workflow {
                    WorkflowName = "LocalParamReferencedInNextLocalParams",
                    Rules = new List<Rule> {
                        new Rule {
                            LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = "\"testString\""
                                },
                                new ScopedParam {
                                    Name = "localParam2",
                                    Expression = "localParam1.ToUpper()"
                                }
                            },
                            RuleName = "WithLocalParam",
                            Expression = "localParam1 == \"testString\" && localParam2 == \"TESTSTRING\""
                        },
                    }
                },
                new Workflow {
                    WorkflowName = "GlobalParamAndLocalParamsInNestedRules",
                    GlobalParams = new List<ScopedParam> {
                        new ScopedParam {
                            Name = "globalParam1",
                            Expression = @"""hello"""
                        }
                    },
                    Rules = new List<Rule> {
                        new Rule {
                           RuleName = "NestedRuleTest",
                           Operator = "And",
                           LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = @"""world"""
                                }
                           },
                           Rules =  new List<Rule>{
                               new Rule{
                                   RuleName = "NestedRule1",
                                   Expression = "globalParam1 == \"hello\" && localParam1 == \"world\""
                               },
                               new Rule {
                                   RuleName = "NestedRule2",
                                   LocalParams = new List<ScopedParam> {
                                       new ScopedParam {
                                           Name = "nestedLocalParam1",
                                           Expression = "globalParam1 + \" \" + localParam1"
                                       }
                                   },
                                   Expression = "nestedLocalParam1 == \"hello world\""
                               }

                           }

                        }
                    }
                },
                new Workflow {
                    WorkflowName = "LocalParamsOnlyWithComplexInput",
                    Rules = new List<Rule> {
                        new Rule {

                            RuleName = "WithLocalParam",
                            LocalParams = new List<ScopedParam> {
                                new ScopedParam {
                                    Name = "localParam1",
                                    Expression = "input1.Country.ToLower()"
                                }
                            },
                            Expression = "localParam1 == \"hello\""
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "GlobalParamsOnlyWithComplexInput",
                    GlobalParams = new List<ScopedParam> {
                        new ScopedParam {
                            Name = "globalParam1",
                            Expression = "input1.Country.ToLower()"
                        }
                    },
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "TrueTest",
                            Expression = "globalParam1 == \"hello\""
                        }
                    }
                },
            };
        }
    }
}
