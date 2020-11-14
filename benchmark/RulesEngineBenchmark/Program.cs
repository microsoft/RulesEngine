﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RulesEngineBenchmark
{
    [MemoryDiagnoser]
    public class REBenchmark
    {
        private readonly RulesEngine.RulesEngine rulesEngine;
        private readonly object ruleInput;
        private readonly List<WorkflowRules> workflows;
        class ListItem
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }


        public REBenchmark()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "NestedInputDemo.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");

            var fileData = File.ReadAllText(files[0]);
            workflows = JsonConvert.DeserializeObject<List<WorkflowRules>>(fileData);
            
            rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), null,new ReSettings { 
                EnableFormattedErrorMessage = false,
                EnableLocalParams = false
            });

            ruleInput =  new
            {
                SimpleProp = "simpleProp",
                NestedProp = new
                {
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
            foreach (var workflow in workflows)
            {
                List<RuleResultTree> resultList = rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, ruleInput).Result;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<REBenchmark>();
        }
    }
}
