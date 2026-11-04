// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue519Test
    {
        [Fact]
        public async Task ExecuteActionWorkflowAsync_FailingRule_PopulatesExceptionMessageFromErrorMessage()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "input1 == \"expected\"",
                        ErrorMessage = "Input was $(input1), expected `expected`"
                    }
                }
            };
            var engine = new RulesEngine(new[] { workflow });

            var actionResult = await engine.ExecuteActionWorkflowAsync("wf", "R",
                new[] { RuleParameter.Create("input1", "actual-value") });

            var ruleResult = actionResult.Results.Single();
            Assert.False(ruleResult.IsSuccess);
            // Before the fix, ExceptionMessage was empty; after, it should contain the interpolated ErrorMessage.
            Assert.False(string.IsNullOrEmpty(ruleResult.ExceptionMessage));
            Assert.Contains("actual-value", ruleResult.ExceptionMessage);
        }

        [Fact]
        public async Task ExecuteAllRulesAsync_BehavesTheSameForReference()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "input1 == \"expected\"",
                        ErrorMessage = "Input was $(input1), expected `expected`"
                    }
                }
            };
            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("wf", "actual-value");

            Assert.False(results[0].IsSuccess);
            Assert.Contains("actual-value", results[0].ExceptionMessage);
        }
    }
}
