// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue513Support
    {
        public class Record
        {
            public int Id { get; set; }
            public string Tag { get; set; }
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue513Test
    {
        // Documents the recommended pattern for "OR-semantics across many rules":
        // wrap them under a single parent rule with Operator="OrElse" and set
        // NestedRuleExecutionMode.Performance so the engine short-circuits on the
        // first true child. The reporter's 13-rules × 600-records scenario fits
        // this shape directly.
        [Fact]
        public async Task NestedOr_WithPerformanceMode_ShortCircuitsOnFirstTrue()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[]
                {
                    new Rule
                    {
                        RuleName = "AnyOfThese",
                        Operator = "OrElse",
                        Rules = Enumerable.Range(0, 13)
                            .Select(i => new Rule
                            {
                                RuleName = $"R{i}",
                                Expression = $"input1.Tag == \"t{i}\""
                            })
                            .ToList()
                    }
                }
            };
            var engine = new RulesEngine(
                new[] { workflow },
                new ReSettings { NestedRuleExecutionMode = NestedRuleExecutionMode.Performance });

            // Matches the first child rule (Tag == "t0").
            var firstMatch = new Issue513Support.Record { Tag = "t0" };
            var firstResults = await engine.ExecuteAllRulesAsync("wf", firstMatch);
            Assert.Single(firstResults);
            Assert.True(firstResults[0].IsSuccess);
            // Only the first child evaluated → child results count is 1, not 13.
            Assert.Single(firstResults[0].ChildResults);

            // Matches no rule.
            var noMatch = new Issue513Support.Record { Tag = "none" };
            var noResults = await engine.ExecuteAllRulesAsync("wf", noMatch);
            Assert.False(noResults[0].IsSuccess);
            // All 13 children evaluated because none matched.
            Assert.Equal(13, noResults[0].ChildResults.Count());
        }

        // Compared to top-level rules, where every rule runs every time and you get a
        // result tree per rule. This is the "no-short-circuit" baseline.
        [Fact]
        public async Task TopLevelRules_NoShortCircuit_AlwaysEvaluatesAll()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = Enumerable.Range(0, 13)
                    .Select(i => new Rule
                    {
                        RuleName = $"R{i}",
                        Expression = $"input1.Tag == \"t{i}\""
                    })
                    .ToArray()
            };
            var engine = new RulesEngine(new[] { workflow });

            var match = new Issue513Support.Record { Tag = "t0" };
            var results = await engine.ExecuteAllRulesAsync("wf", match);

            Assert.Equal(13, results.Count);
            Assert.Equal(1, results.Count(r => r.IsSuccess));
        }
    }
}
