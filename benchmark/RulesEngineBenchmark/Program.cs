// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Jobs;
using System.Text.Json;

namespace RulesEngineBenchmark
{

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class REBenchmark
    {
        private readonly RulesEngine.RulesEngine rulesEngine;
        private readonly object ruleInput;
        private readonly List<Workflow> workflow;

        private class ListItem
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }


        public REBenchmark()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "NestedInputDemo.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                throw new Exception("Rules not found.");
            }

            var fileData = File.ReadAllText(files[0]);
            workflow = JsonSerializer.Deserialize<List<Workflow>>(fileData);

            rulesEngine = new RulesEngine.RulesEngine(workflow.ToArray(), new ReSettings {
                EnableFormattedErrorMessage = false,
                EnableScopedParams = false
            });

            ruleInput = new {
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
        }

        [Params(1000, 10000)]
        public int N;

        [Benchmark]
        public void RuleExecutionDefault()
        {
            foreach (var workflow in workflow)
            {
                _ = rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, ruleInput).Result;
            }
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<REBenchmark>();
        }
    }
}
