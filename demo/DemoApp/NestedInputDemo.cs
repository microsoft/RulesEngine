// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace DemoApp
{
    internal class ListItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class NestedInputDemo
    {
        public void Run()
        {
            Console.WriteLine($"Running {nameof(NestedInputDemo)}....");
            var nestedInput = new {
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

            };

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "NestedInputDemo.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                throw new Exception("Rules not found.");
            }

            var fileData = File.ReadAllText(files[0]);
            var Workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

            var bre = new RulesEngine.RulesEngine(Workflows.ToArray(), null);
            foreach (var workflow in Workflows)
            {
                var resultList = bre.ExecuteAllRulesAsync(workflow.WorkflowName, nestedInput).Result;

                resultList.OnSuccess((eventName) => {
                    Console.WriteLine($"{workflow.WorkflowName} evaluation resulted in success - {eventName}");
                }).OnFail(() => {
                    Console.WriteLine($"{workflow.WorkflowName} evaluation resulted in failure");
                });

            }


        }
    }
}