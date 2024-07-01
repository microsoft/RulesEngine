// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class BasicDemo : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(BasicDemo)}....");
        var workflows = new List<Workflow>();
        var workflow = new Workflow {WorkflowName = "Test Workflow Rule 1"};

        var rules = new List<Rule>();

        var rule = new Rule {
            RuleName = "Test Rule",
            SuccessEvent = "Count is within tolerance.",
            ErrorMessage = "Over expected.",
            Expression = "count < 3",
            RuleExpressionType = RuleExpressionType.LambdaExpression
        };

        rules.Add(rule);

        workflow.Rules = rules;

        workflows.Add(workflow);

        var bre = new RulesEngine.RulesEngine(workflows.ToArray());

        dynamic datas = new ExpandoObject();
        datas.count = 1;
        var inputs = new[] {datas};

        var resultList = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", cancellationToken, inputs);

        //Different ways to show test results:
        var outcome = resultList.TrueForAll(r => r.IsSuccess);

        resultList.OnSuccess(eventName => {
            Console.WriteLine($"Result '{eventName}' is as expected.");
            outcome = true;
        });

        resultList.OnFail(() => {
            outcome = false;
        });

        Console.WriteLine($"Test outcome: {outcome}.");
    }
}