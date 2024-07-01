// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System.Text.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

internal class ListItem
{
    public int Id { get; set; }
    public string Value { get; set; }
}

public class NestedInputDemo : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(NestedInputDemo)}....");
        var nestedInput = new {
            SimpleProp = "simpleProp",
            NestedProp = new {
                SimpleProp = "nestedSimpleProp",
                ListProp = new List<ListItem> {new() {Id = 1, Value = "first"}, new() {Id = 2, Value = "second"}}
            }
        };

        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "NestedInputDemo.json",
            SearchOption.AllDirectories);
        if (files == null || files.Length == 0)
        {
            throw new Exception("Rules not found.");
        }

        var fileData = await File.ReadAllTextAsync(files[0], cancellationToken);
        var workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData)!;

        var bre = new RulesEngine.RulesEngine(workflows.ToArray());
        foreach (var workflow in workflows.Select(x => x.WorkflowName))
        {
            var resultList = await bre.ExecuteAllRulesAsync(workflow, cancellationToken, nestedInput);

            resultList.OnSuccess(eventName => {
                Console.WriteLine($"{workflow} evaluation resulted in success - {eventName}");
            }).OnFail(() => {
                Console.WriteLine($"{workflow} evaluation resulted in failure");
            });
        }
    }
}