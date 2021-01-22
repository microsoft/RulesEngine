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
        public async Task BasicWorkflowRules_ReturnsTrue(string workflowName)
        {
            var workflows = GetWorkflowRulesList();

            var engine = new RulesEngine(null, null);
            engine.AddWorkflow(workflows);

            var input1 = new {
                trueValue = true,
                falseValue = false
            };

            var result = await engine.ExecuteAllRulesAsync(workflowName, input1);
            Assert.True(result.All(c => c.IsSuccess));

        }

        private WorkflowRules[] GetWorkflowRulesList()
        {
            return new WorkflowRules[] {
                new WorkflowRules {
                    WorkflowName = "NoLocalAndGlobalParams",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "TruthTest",
                            Expression = "input1.trueValue"
                        }
                    }
                },
                new WorkflowRules {
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
                new WorkflowRules {
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
                new WorkflowRules {
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
                new WorkflowRules {
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
                new WorkflowRules {
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
                new WorkflowRules {
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
                new WorkflowRules {
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
                }
            };
        }
    }
}
