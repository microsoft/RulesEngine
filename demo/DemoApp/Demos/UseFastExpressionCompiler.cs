// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Demos
{
    public class UseFastExpressionCompiler
    {
        public async Task Run(CancellationToken ct = default)
        {
            var reSettingsWithCustomTypes = new ReSettings {
                UseFastExpressionCompiler = true
            };

            var rule = new Rule {
                RuleName = "check local param with plus operator",
                Expression = "Total > 0",
                LocalParams = new List<ScopedParam>() {
                new() {
                    Name = "Field1",
                    Expression = "AppData.Details.Sum(l => l.Amount.Value)"
                },
                new() {
                    Name = "Field2",
                    Expression = "AppData.Details.Sum(l => l.Amount.Value)"
                },
                new() {
                    Name = "Field3",
                    Expression = "AppData.Details.Sum(l => l.Amount.Value)"
                },
                new() { Name = "Total", Expression = "Field1 + Field2 + Field3" }
            }
            };

            var worflow = new Workflow {
                WorkflowName = "UseFastExpressionCompilerTest",
                Rules = [rule]
            };

            var input = new AppData() {
                Details = new List<Detail>
                {
                new Detail { Amount = 1.0m },
                new Detail { Amount = 2.0m },
                new Detail { Amount = 3.0m }
            }
            };

            var appData = new RuleParameter("AppData", input);

            var re = new RulesEngine.RulesEngine([worflow], reSettingsWithCustomTypes);
            var result = await re.ExecuteAllRulesAsync("UseFastExpressionCompilerTest", [appData], ct);

            Console.WriteLine(result[0].IsSuccess);
        }

        internal class AppData
        {
            public List<Detail> Details { get; set; } = new List<Detail>();
        }
        internal class Detail
        {
            public decimal? Amount { get; set; }
        }
    }
}
