// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class Basic
    {
        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(Basic)}....");

            var workflows = new Workflow[] {
                new Workflow {
                    WorkflowName = "Test Workflow Rule 1",
                    Rules = [
                        new Rule {
                            RuleName = "Test Rule",
                            SuccessMessage = "Count is within tolerance",
                            ErrorMessage = "Over expected",
                            Expression = "count < 3"
                        }
                    ]
                }
            };
            
            var bre = new RulesEngine.RulesEngine(workflows);

            var rp = new RuleParameter[] {
                new RuleParameter("input1", new { count = 1 })
            };

            await foreach (var async_rrt in bre.ExecuteAllWorkflows(rp, ct))
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
