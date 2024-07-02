// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class UseFastExpressionCompiler : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var worflow = new Workflow[] {
            new() {
                WorkflowName = "UseFastExpressionCompilerTest",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "check local param with plus operator",
                        Expression = "Total > 0",
                        LocalParams = new List<ScopedParam> {
                            new() {Name = "Field1", Expression = "AppData.Details.Sum(l => l.Amount.Value)"},
                            new() {Name = "Field2", Expression = "AppData.Details.Sum(l => l.Amount.Value)"},
                            new() {Name = "Field3", Expression = "AppData.Details.Sum(l => l.Amount.Value)"},
                            new() {Name = "Total", Expression = "Field1 + Field2 + Field3"}
                        }
                    }
                }
            }
        };

        var appData = new RuleParameter[] {
            new("AppData",
                new AppData {
                    Details = new List<Detail> {new() {Amount = 1.0m}, new() {Amount = 2.0m}, new() {Amount = 3.0m}}
                })
        };

        var reSettingsWithCustomTypes = new ReSettings {
            UseFastExpressionCompiler = true //default setting is true
        };

        var bre = new RulesEngine.RulesEngine(worflow, reSettingsWithCustomTypes);

        var ret = await bre.ExecuteAllRulesAsync("UseFastExpressionCompilerTest", cancellationToken, appData);

        if (ret is {Count: > 0})
        {
            Console.WriteLine(ret[0].IsSuccess);
        }
    }

    internal class AppData
    {
        public List<Detail> Details { get; set; } = new();
    }

    internal class Detail
    {
        public decimal? Amount { get; set; }
    }
}