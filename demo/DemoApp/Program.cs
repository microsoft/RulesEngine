// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DemoApp
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using RulesEngine.Models;

    public class Program
    {
        public async static Task Main()
        {
            var items = new List<Guid> { Guid.NewGuid() };
            var workFlow = BuildExceptionWorkflow();

            // Execute rules engine
            var rulesEngine = new RulesEngine.RulesEngine();

            rulesEngine.AddOrUpdateWorkflow(workFlow);

            List<RuleResultTree> results = await rulesEngine.ExecuteAllRulesAsync(workFlow.WorkflowName, items);

            var exceptions = results
                .Where(r => r.ChildResults.Any(x => !string.IsNullOrWhiteSpace(x.ExceptionMessage)));

            Console.WriteLine(rulesEngine.GetType().Assembly.ToString());
            Console.WriteLine("Rule Exception Count: {0}", exceptions.Count());
            if (exceptions.Any())
            {
                Console.WriteLine("Rule Exception: {0}", exceptions.First().ChildResults.First().ExceptionMessage);
            }
            Console.WriteLine("Rule IsSuccess: {0}", results.First().IsSuccess);
        }

        private static Workflow BuildExceptionWorkflow()
        {
            var workFlow = new Workflow {
                WorkflowName = "workflow",
                Rules = new List<Rule>
                {
                new()
                {
                    Enabled = true,
                    RuleName = "InsertBankAccount",
                    Operator = "And",
                    Rules = new List<Rule> {new()
                        {
                            RuleName = "InsertBankAccountExpected",
                            Expression = "\"Sample\".Substring(-5)",
                            ErrorMessage = "Expected Completed Status."
                        }
                    },
                }
            }
            };
            return workFlow;
        }
    }
}
