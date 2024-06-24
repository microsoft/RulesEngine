// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Demos
{
    public class NestedInput
    {
        internal class ListItem
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(NestedInput)}....");

            var rp = new RuleParameter[] {
                new RuleParameter("input1", new 
                {
                    SimpleProp = "simpleProp",
                    NestedProp = new {
                        SimpleProp = "nestedSimpleProp",
                        ListProp = new List<ListItem>
                        {
                            new ListItem
                            {
                                Id = 1,
                                Value = "first"
                            },
                            new ListItem
                            {
                                Id = 2,
                                Value = "second"
                            }
                        }
                    }
                })
            };

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Workflows";
            var files = Directory.GetFiles(dir, "NestedInput.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");
            
            var fileData = await File.ReadAllTextAsync(files[0]);
            var Workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

            var bre = new RulesEngine.RulesEngine(Workflows.ToArray(), null);

            await foreach (var async_ret in bre.ExecuteAllWorkflows(rp, ct))
            {
                async_ret.OnSuccess((eventName) => {
                    Console.WriteLine($"evaluation resulted in success - {eventName}");
                }).OnFail(() => {
                    Console.WriteLine($"evaluation resulted in failure");
                });
            }
        }
    }
}