// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System.Text.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class NestedInput : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(NestedInput)}....");

        var rp = new RuleParameter[] {
            new("input1",
                new {
                    SimpleProp = "simpleProp",
                    NestedProp = new {
                        SimpleProp = "nestedSimpleProp",
                        ListProp = new List<ListItem> {
                            new() {Id = 1, Value = "first"}, new() {Id = 2, Value = "second"}
                        }
                    }
                })
        };

        var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Workflows");
        var files = Directory.GetFiles(dir, "NestedInput.json", SearchOption.AllDirectories);
        if (files == null || files.Length == 0)
        {
            throw new FileNotFoundException("Rules not found.");
        }

        var fileData = await File.ReadAllTextAsync(files[0], cancellationToken);
        var workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData)!;

        var bre = new RulesEngine.RulesEngine(workflows.ToArray());

        foreach (var workflow in workflows)
        {
            var ret = await bre.ExecuteAllRulesAsync(workflow.WorkflowName, cancellationToken, rp);

            ret.OnSuccess(eventName => {
                Console.WriteLine($"evaluation resulted in success - {eventName}");
            }).OnFail(() => {
                Console.WriteLine("evaluation resulted in failure");
            });
        }
    }

    internal class ListItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}