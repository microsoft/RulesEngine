// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Demos
{
    public class MultipleWorkflows
    {
        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(MultipleWorkflows)}....");

            var workflows = new Workflow[] {
                new Workflow {
                    WorkflowName = "Test Workflow1",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule1",
                            SuccessMessage = "Count is less",
                            ErrorMessage = "Over Expected",
                            Expression = "count < 3",
                        },
                        new Rule {
                            RuleName = "Test Rule2",
                            SuccessMessage = "Count is more",
                            ErrorMessage = "Under Expected",
                            Expression = "count > 3",
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "Test Workflow2",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule",
                            SuccessMessage = "Count is greater",
                            ErrorMessage = "Under Expected",
                            Expression = "count > 3",
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "Test Workflow3",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule",
                            Expression = "1 == 1",
                            Actions = new RuleActions() {
                                OnSuccess = new ActionInfo {
                                    Name = "OutputExpression",
                                    Context =  new Dictionary<string, object> {
                                        {"expression", "2*2"}
                                    }
                                }
                            }
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "Test Workflow4",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule",
                            Expression = "1 == 1",
                            Actions = new RuleActions() {
                                OnSuccess = new ActionInfo {
                                    Name = "OutputExpression",
                                    Context =  new Dictionary<string, object> {
                                        {"expression", "4*4"}
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var bre = new RulesEngine.RulesEngine(workflows, null);

            var inputs = new RuleParameter[] {
                new RuleParameter("input1", new { count = 1 })
            };

            await foreach (var async_rrt in bre.ExecuteAllWorkflows(inputs, ct))
            {
                var outcome = false;

                //Different ways to show test results:
                outcome = async_rrt.TrueForAll(r => r.IsSuccess);

                async_rrt.OnSuccess((eventName) => {
                    Console.WriteLine($"Result '{eventName}' is as expected.");
                    outcome = true;
                });

                async_rrt.OnFail(() => {
                    outcome = false;
                });

                Console.WriteLine($"Test outcome: {outcome}.");
            }
        }
    }
}
