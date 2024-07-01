// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp;

public class Basic : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(Basic)}....");

        var workflows = new Workflow[] {
            new() {
                WorkflowName = "Test Workflow Rule 1",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "Test Rule",
                        SuccessEvent = "Count is within tolerance",
                        ErrorMessage = "Over expected",
                        Expression = "count < 3"
                    }
                }
            }
        };

        var bre = new RulesEngine.RulesEngine(workflows);

        var rp = new RuleParameter[] {new("input1", new {count = 1})};

        var ret = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", cancellationToken, rp);

        //Different ways to show test results:
        var outcome = ret.TrueForAll(r => r.IsSuccess);

        ret.OnSuccess(eventName => {
            Console.WriteLine($"Result '{eventName}' is as expected.");
            outcome = true;
        });

        ret.OnFail(() => {
            outcome = false;
        });

        Console.WriteLine($"Test outcome: {outcome}.");
    }
}