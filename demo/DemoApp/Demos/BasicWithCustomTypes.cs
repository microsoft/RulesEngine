// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class BasicWithCustomTypes
    {
        internal static class Utils
        {
            public static bool CheckContains(string check, string valList)
            {
                if (String.IsNullOrEmpty(check) || String.IsNullOrEmpty(valList))
                    return false;

                var list = valList.Split(',').ToList();
                return list.Contains(check);
            }
        }
        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(BasicWithCustomTypes)}....");

            var workflows = new Workflow[] {
                new Workflow {
                    WorkflowName = "Test Workflow Rule 1",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule",
                            SuccessMessage = "doSomething ran successfully",
                            ErrorMessage = "doSomething failed",
                            Expression = "Utils.CheckContains(string1, \"bye,seeya,hello\") == true"
                        }
                    }
                }
            };

            var reSettings = new ReSettings {
                CustomTypes = [ typeof(Utils) ]
            };

            var bre = new RulesEngine.RulesEngine(workflows, reSettings);

            var rp = new RuleParameter[] {
                new RuleParameter("string1", "hello")
            };

            await foreach(var async_rrt in bre.ExecuteAllWorkflows(rp, ct))
            {
                async_rrt.OnSuccess((eventName) => {
                    Console.WriteLine($"Result '{eventName}' is as expected.");
                });
                async_rrt.OnFail(() => {
                    Console.WriteLine($"Test outcome: false");
                });
            }
        }
    }
}
