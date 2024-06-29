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
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule",
                            SuccessEvent = "Count is within tolerance",
                            ErrorMessage = "Over expected",
                            Expression = "count < 3"
                        }
                    }
                }
            };

            var bre = new RulesEngine.RulesEngine(workflows);

            var rp = new RuleParameter[] {
                new RuleParameter("input1", new { count = 1 })
            };

            var ret = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", rp);

            var outcome = false;

            //Different ways to show test results:
            outcome = ret.TrueForAll(r => r.IsSuccess);

            ret.OnSuccess((eventName) => {
                Console.WriteLine($"Result '{eventName}' is as expected.");
                outcome = true;
            });

            ret.OnFail(() => {
                outcome = false;
            });

            Console.WriteLine($"Test outcome: {outcome}.");
        }
    }
}