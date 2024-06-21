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
            var workflows = new List<Workflow>();
            var workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            var rules = new List<Rule> {
                new Rule() {
                    RuleName = "Test Rule",
                    SuccessMessage = "doSomething ran successfully",
                    ErrorMessage = "doSomething failed",
                    Expression = "Utils.CheckContains(string1, \"bye,seeya,hello\") == true",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                }
            };
            
            workflow.Rules = rules;
            workflows.Add(workflow);

            var reSettings = new ReSettings {
                CustomTypes = [ typeof(Utils) ]
            };

            var bre = new RulesEngine.RulesEngine(workflows.ToArray(), reSettings);

            var string1 = new RuleParameter("string1", "hello");
            
            var ruleResultTree = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", [string1], ct);
            ruleResultTree.OnSuccess((eventName) => {
                Console.WriteLine($"Result '{eventName}' is as expected.");
            });
            ruleResultTree.OnFail((eventName) => {
                Console.WriteLine($"Test outcome: false");
            });

            var actionRuleResult = await bre.ExecuteActionWorkflowAsync("Test Workflow Rule 1", "Test Rule", [string1], ct);
            actionRuleResult.Results.OnSuccess((eventName) => {
                Console.WriteLine($"Result '{eventName}' is as expected.");
            });
            actionRuleResult.Results.OnFail((eventName) => {
                Console.WriteLine($"Test outcome: false");
            });
        }
    }
}
