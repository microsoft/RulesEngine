﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Castle.Core.Internal;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class RulesEnabledTests
    {
        public RulesEnabledTests()
        {

        }

        [Theory]
        [InlineData("RuleEnabledFeatureTest",new bool[] { true,true})]
        [InlineData("RuleEnabledNestedFeatureTest",new bool[] { true, true, false })]
        public async Task RulesEngine_ShouldOnlyExecuteEnabledRules(string workflowName, bool[] expectedRuleResults)
        {
            var workflows = GetWorkflows();
            var rulesEngine = new RulesEngine(workflows);
            var input1 = new {
                TrueValue = true
            };
            var result = await rulesEngine.ExecuteAllRulesAsync(workflowName, input1);
            Assert.NotNull(result);
            Assert.True(NestedEnabledCheck(result));

            Assert.Equal(expectedRuleResults.Length, result.Count);
            for(var i = 0; i < expectedRuleResults.Length; i++)
            {
                Assert.Equal(expectedRuleResults[i], result[i].IsSuccess);
            }
        }

        private bool NestedEnabledCheck(IEnumerable<RuleResultTree> ruleResults)
        {
            var areAllRulesEnabled = ruleResults.All(c => c.Rule.Enabled);
            if (areAllRulesEnabled)
            {
                foreach(var ruleResult in ruleResults)
                {
                    if (ruleResult.ChildResults?.Any() == true)
                    {
                        var areAllChildRulesEnabled = NestedEnabledCheck(ruleResult.ChildResults);
                        if (areAllChildRulesEnabled == false)
                        {
                            return false;
                        }
                    }
                }
            }
            return areAllRulesEnabled;
        }

        private WorkflowRules[] GetWorkflows()
        {
            return new[] {
                new WorkflowRules {
                    WorkflowName = "RuleEnabledFeatureTest",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "RuleWithoutEnabledFieldMentioned",
                            Expression = "input1.TrueValue == true"
                        },
                        new Rule {
                            RuleName = "RuleWithEnabledSetToTrue",
                            Expression = "input1.TrueValue == true",
                            Enabled = true
                        },
                        new Rule {
                            RuleName = "RuleWithEnabledSetToFalse",
                            Expression = "input1.TrueValue == true",
                            Enabled = false
                        }

                    }
                },
                new WorkflowRules {
                    WorkflowName = "RuleEnabledNestedFeatureTest",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "RuleWithoutEnabledFieldMentioned",
                            Operator = "And",
                            Rules = new List<Rule> {
                                new Rule {
                                    RuleName = "RuleWithoutEnabledField",
                                    Expression = "input1.TrueValue"
                                }
                            }
                        },
                        new Rule {
                            RuleName = "RuleWithOneChildSetToFalse",
                            Expression = "input1.TrueValue == true",
                            Operator = "And",
                            Rules = new List<Rule>{
                                new Rule {
                                    RuleName = "RuleWithEnabledFalse",
                                    Expression = "input1.TrueValue",
                                    Enabled = false,
                                },
                                new Rule {
                                    RuleName = "RuleWithEnabledTrue",
                                    Expression = "input1.TrueValue",
                                    Enabled = true
                                }
                            }

                        },
                        new Rule {
                            RuleName = "RuleWithParentSetToFalse",
                            Operator = "And",
                            Enabled = false,
                            Rules = new List<Rule>{
                                new Rule {
                                    RuleName = "RuleWithEnabledTrue",
                                    Expression = "input1.TrueValue",
                                    Enabled = true
                                }
                            }
                        },
                        new Rule {
                            RuleName = "RuleWithAllChildSetToFalse",
                            Operator = "And",
                            Enabled = true,
                            Rules  = new List<Rule>{
                                new Rule {
                                    RuleName = "ChildRuleWithEnabledFalse",
                                    Expression = "input1.TrueValue",
                                    Enabled = false
                                }
                            }
                        }

                    }
                }
            };
        }

    }
}
