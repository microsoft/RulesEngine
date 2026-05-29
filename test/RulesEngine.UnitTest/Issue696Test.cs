// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue696Support
    {
        public class Nested
        {
            public Inner Inner { get; set; }
        }
        public class Inner
        {
            public string Name { get; set; }
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue696Test
    {
        [Fact]
        public async Task ErrorMessage_WithNestedDottedInterpolation_ResolvesRecursively()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "false",
                        ErrorMessage = "got $(input1.Inner.Name)"
                    }
                }
            };
            var input = new Issue696Support.Nested
            {
                Inner = new Issue696Support.Inner { Name = "deep-value" }
            };
            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("wf",
                new[] { RuleParameter.Create("input1", input) });

            Assert.Equal("got deep-value", results[0].ExceptionMessage);
        }

        [Fact]
        public async Task ErrorMessage_WithSingleLevelInterpolation_StillWorks()
        {
            // Regression guard for the existing one-level case.
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "false",
                        ErrorMessage = "got $(input1.Name)"
                    }
                }
            };
            var input = new Issue696Support.Inner { Name = "simple-value" };
            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("wf",
                new[] { RuleParameter.Create("input1", input) });

            Assert.Contains("simple-value", results[0].ExceptionMessage);
        }
    }
}
