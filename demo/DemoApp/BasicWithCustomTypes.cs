// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class BasicWithCustomTypes : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(BasicWithCustomTypes)}....");

        var workflows = new Workflow[] {
            new() {
                WorkflowName = "Test Workflow Rule 1",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "Test Rule",
                        SuccessEvent = "doSomething ran successfully",
                        ErrorMessage = "doSomething failed",
                        Expression = @"Utils.CheckContains(string1, ""bye,seeya,hello"") == true"
                    }
                }
            }
        };

        var reSettings = new ReSettings {CustomTypes = new[] {typeof(Utils)}};

        var bre = new RulesEngine.RulesEngine(workflows, reSettings);

        var rp = new RuleParameter[] {new("string1", "hello")};

        var ret = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", cancellationToken, rp);

        ret.OnSuccess(eventName => {
            Console.WriteLine($"Result '{eventName}' is as expected.");
        });
        ret.OnFail(() => {
            Console.WriteLine("Test outcome: false");
        });
    }

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
}