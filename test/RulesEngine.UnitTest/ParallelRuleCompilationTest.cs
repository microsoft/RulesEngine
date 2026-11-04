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
    public class ParallelRuleCompilationTest
    {
        // Larger than the internal MinRulesForParallelCompilation threshold so parallel mode actually engages.
        private const int RuleCount = 64;

        private static Workflow BuildLargeWorkflow() => new Workflow
        {
            WorkflowName = "wf",
            Rules = Enumerable.Range(0, RuleCount)
                .Select(i => new Rule { RuleName = $"R{i}", Expression = $"input1 >= {i}" })
                .ToArray()
        };

        [Fact]
        public async Task ParallelCompilation_ProducesIdenticalResultsAsSerial()
        {
            var workflow = BuildLargeWorkflow();

            var serialEngine = new RulesEngine(new[] { workflow },
                new ReSettings { EnableParallelRuleCompilation = false });
            var parallelEngine = new RulesEngine(new[] { workflow },
                new ReSettings { EnableParallelRuleCompilation = true });

            var serial = await serialEngine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", 32) });
            var parallel = await parallelEngine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", 32) });

            Assert.Equal(serial.Count, parallel.Count);
            for (var i = 0; i < serial.Count; i++)
            {
                Assert.Equal(serial[i].Rule.RuleName, parallel[i].Rule.RuleName);
                Assert.Equal(serial[i].IsSuccess, parallel[i].IsSuccess);
            }
        }

        [Fact]
        public async Task ParallelCompilation_PreservesExceptionMessage_WhenRuleExpressionThrows()
        {
            // Inject a deliberately broken rule into a large-enough workflow that the parallel
            // path engages, then assert the per-rule ExceptionMessage explains the underlying
            // failure rather than leaking an AggregateException.
            var rules = Enumerable.Range(0, RuleCount)
                .Select(i => new Rule { RuleName = $"R{i}", Expression = "input1 >= 0" })
                .ToList();
            rules[5].Expression = "input1.NoSuchMember.Foo()";

            var workflow = new Workflow { WorkflowName = "wf", Rules = rules };
            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { EnableParallelRuleCompilation = true });

            var results = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", 1) });

            var broken = results.Single(r => r.Rule.RuleName == "R5");
            Assert.False(broken.IsSuccess);
            Assert.False(string.IsNullOrEmpty(broken.ExceptionMessage));
            Assert.DoesNotContain("AggregateException", broken.ExceptionMessage);
        }

        [Fact]
        public async Task ParallelCompilation_FallsBackToSerial_WhenFastExpressionCompilerEnabled()
        {
            // The flag combination is permitted at construction time (back-compat), but the
            // engine declines to parallelize and silently uses the serial path. We can only
            // assert correctness here (results match serial). The fallback itself is
            // observable in benchmarks, not in functional tests.
            var engine = new RulesEngine(new[] { BuildLargeWorkflow() },
                new ReSettings
                {
                    EnableParallelRuleCompilation = true,
                    UseFastExpressionCompiler = true
                });

            var results = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", 100) });

            Assert.Equal(RuleCount, results.Count);
            Assert.All(results, r => Assert.True(r.IsSuccess));
        }

        [Fact]
        public async Task ParallelCompilation_FallsBackToSerial_ForSmallWorkflowsBelowThreshold()
        {
            // Below the minimum threshold, the engine declines to parallelize. Verify a
            // 5-rule workflow still works correctly with the flag enabled.
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = Enumerable.Range(0, 5)
                    .Select(i => new Rule { RuleName = $"R{i}", Expression = $"input1 >= {i}" })
                    .ToArray()
            };
            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { EnableParallelRuleCompilation = true });

            var results = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", 3) });

            Assert.Equal(5, results.Count);
            // input1=3 means R0..R3 succeed, R4 fails. Same outcome whether serial or parallel.
            Assert.Equal(4, results.Count(r => r.IsSuccess));
        }
    }
}
