// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
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
                if (string.IsNullOrEmpty(check) || string.IsNullOrEmpty(valList))
                {
                    return false;
                }

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
                            SuccessEvent = "doSomething ran successfully",
                            ErrorMessage = "doSomething failed",
                            Expression = "Utils.CheckContains(string1, \"bye,seeya,hello\") == true"
                        }
                    }
                }
            };

            var reSettings = new ReSettings {
                CustomTypes = new[] { typeof(Utils) }
            };

            var bre = new RulesEngine.RulesEngine(workflows, reSettings);

            var rp = new RuleParameter[] {
                new RuleParameter("string1", "hello")
            };

            var ret = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", rp);

            ret.OnSuccess((eventName) => {
                Console.WriteLine($"Result '{eventName}' is as expected.");
            });
            ret.OnFail(() => {
                Console.WriteLine($"Test outcome: false");
            });
        }
    }
}