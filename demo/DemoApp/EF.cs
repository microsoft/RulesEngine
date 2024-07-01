// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class EF : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(EF)}....");

        var workflows = new Workflow[] {
            new() {
                WorkflowName = "Test Workflow1",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "Test Rule1",
                        SuccessEvent = "Count is less",
                        ErrorMessage = "Over Expected",
                        Expression = "count < 3"
                    },
                    new() {
                        RuleName = "Test Rule2",
                        SuccessEvent = "Count is more",
                        ErrorMessage = "Under Expected",
                        Expression = "count > 3"
                    }
                }
            },
            new() {
                WorkflowName = "Test Workflow2",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "Test Rule3",
                        SuccessEvent = "Count is greater",
                        ErrorMessage = "Under Expected",
                        Expression = "count > 3"
                    }
                }
            },
            new() {
                WorkflowName = "Test Workflow3",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "Test Rule4",
                        Expression = "1 == 1",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> {{"expression", "2*2"}}
                            }
                        }
                    }
                }
            },
            new() {
                WorkflowName = "Test Workflow4",
                Rules = new List<Rule> {
                    new() {
                        RuleName = "Test Rule5",
                        Expression = "1 == 1",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> {{"expression", "4*4"}}
                            }
                        }
                    }
                }
            }
        };

        await using var db = new RulesEngineContext();

        if (await db.Database.EnsureCreatedAsync(cancellationToken))
        {
            await db.Workflows.AddRangeAsync(workflows, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        var wfr = db.Workflows.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();

        var rp = new RuleParameter[] {new("input1", new {count = 1})};

        var bre = new RulesEngine.RulesEngine(wfr);

        foreach (var workflow in wfr)
        {
            var ret = await bre.ExecuteAllRulesAsync(workflow.WorkflowName, cancellationToken, rp);

            ret.OnSuccess(eventName => {
                Console.WriteLine($"Discount offered is {eventName} % over MRP.");
            });

            ret.OnFail(() => {
                Console.WriteLine("The user is not eligible for any discount.");
            });
        }
    }
}