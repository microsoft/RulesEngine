// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class EF
    {
        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(EF)}....");
            
            var workflows = new Workflow[] {
                new Workflow {
                    WorkflowName = "Test Workflow1",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule1",
                            SuccessMessage = "Count is less",
                            ErrorMessage = "Over Expected",
                            Expression = "count < 3",
                        },
                        new Rule {
                            RuleName = "Test Rule2",
                            SuccessMessage = "Count is more",
                            ErrorMessage = "Under Expected",
                            Expression = "count > 3",
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "Test Workflow2",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule3",
                            SuccessMessage = "Count is greater",
                            ErrorMessage = "Under Expected",
                            Expression = "count > 3",
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "Test Workflow3",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule4",
                            Expression = "1 == 1",
                            Actions = new RuleActions() {
                                OnSuccess = new ActionInfo {
                                    Name = "OutputExpression",
                                    Context =  new Dictionary<string, object> {
                                        {"expression", "2*2"}
                                    }
                                }
                            }
                        }
                    }
                },
                new Workflow {
                    WorkflowName = "Test Workflow4",
                    Rules = new List<Rule> {
                        new Rule {
                            RuleName = "Test Rule5",
                            Expression = "1 == 1",
                            Actions = new RuleActions() {
                                OnSuccess = new ActionInfo {
                                    Name = "OutputExpression",
                                    Context =  new Dictionary<string, object> {
                                        {"expression", "4*4"}
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Workflow[] wfr = null;
            using (RulesEngineContext db = new RulesEngineContext())
            {
                if (await db.Database.EnsureCreatedAsync(ct))
                {
                    await db.Workflows.AddRangeAsync(workflows, ct);
                    await db.SaveChangesAsync(ct);
                }

                wfr = db.Workflows.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();
            }

            if (wfr != null)
            {
                var rp = new RuleParameter[] {
                    new RuleParameter("input1", new { count = 1 })
                };

                var bre = new RulesEngine.RulesEngine(wfr, null);

                await foreach (var rrt in bre.ExecuteAllWorkflows(rp, ct))
                {
                    rrt.OnSuccess((eventName) => {
                        Console.WriteLine($"Discount offered is {eventName} % over MRP.");
                    });

                    rrt.OnFail(() => {
                        Console.WriteLine("The user is not eligible for any discount.");
                    });
                }
            }
        }
    }
}
