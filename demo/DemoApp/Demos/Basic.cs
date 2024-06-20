﻿// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class Basic
    {
        public void Run()
        {
            Console.WriteLine($"Running {nameof(Basic)}....");
            var workflows = new List<Workflow>();
            var workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            var rules = new List<Rule>();

            var rule = new Rule();
            rule.RuleName = "Test Rule";
            rule.SuccessMessage = "Count is within tolerance.";
            rule.ErrorMessage = "Over expected.";
            rule.Expression = "count < 3";
            rule.RuleExpressionType = RuleExpressionType.LambdaExpression;

            rules.Add(rule);

            workflow.Rules = rules;

            workflows.Add(workflow);

            var bre = new RulesEngine.RulesEngine(workflows.ToArray(), null);

            dynamic datas = new ExpandoObject();
            datas.count = 1;
            var inputs = new dynamic[]
            {
                datas
            };

            var resultList = bre.ExecuteAllRulesAsync("Test Workflow Rule 1", inputs).Result;

            var outcome = false;

            //Different ways to show test results:
            outcome = resultList.TrueForAll(r => r.IsSuccess);

            resultList.OnSuccess((eventName) => {
                Console.WriteLine($"Result '{eventName}' is as expected.");
                outcome = true;
            });

            resultList.OnFail(() => {
                outcome = false;
            });

            Console.WriteLine($"Test outcome: {outcome}.");
        }
    }
}
