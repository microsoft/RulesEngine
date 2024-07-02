using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class CustomParameterName : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(Basic)}....");

        var workflows = new Workflow[] {
            new() {
                WorkflowName = "my_workflow",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "MatchesFabrikam",
                        SuccessEvent = "does match",
                        ErrorMessage = "does not match",
                        Expression = @"myValue.Value1 == ""Fabrikam"""
                    }
                }
            }
        };

        var bre = new RulesEngine.RulesEngine(workflows);

        var rp = new RuleParameter[] { new("myValue", new { Value1 = "Fabrikam" }) };

        var ret = await bre.ExecuteAllRulesAsync("my_workflow", cancellationToken, rp);

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