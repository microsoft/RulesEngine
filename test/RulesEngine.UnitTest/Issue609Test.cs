// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue609Test
    {
        private static Workflow ManyRulesWorkflow(int count) => new Workflow
        {
            WorkflowName = "wf",
            Rules = Enumerable.Range(0, count)
                .Select(i => new Rule { RuleName = $"R{i}", Expression = "input1 >= 0" })
                .ToArray()
        };

        [Fact]
        public async Task ExecuteAllRulesAsync_WithUncancelledToken_RunsNormally()
        {
            var engine = new RulesEngine(new[] { ManyRulesWorkflow(5) });
            var results = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", 1) }, CancellationToken.None);
            Assert.Equal(5, results.Count);
            Assert.All(results, r => Assert.True(r.IsSuccess));
        }

        [Fact]
        public async Task ExecuteAllRulesAsync_WithAlreadyCancelledToken_Throws()
        {
            var engine = new RulesEngine(new[] { ManyRulesWorkflow(5) });
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await engine.ExecuteAllRulesAsync(
                    "wf", new[] { RuleParameter.Create("input1", 1) }, cts.Token));
        }

        [Fact]
        public async Task ExecuteAllRulesAsync_DefaultOverloads_StillWork_NoBehavioralBreakage()
        {
            // The pre-existing signatures still resolve correctly. The new 3-arg overload is
            // strictly more specific than `params object[]`, so call sites that pass
            // (string, array) continue to bind to the params overloads as before.
            var engine = new RulesEngine(new[] { ManyRulesWorkflow(3) });
            var byParams = await engine.ExecuteAllRulesAsync("wf", 1);
            var byRuleParams = await engine.ExecuteAllRulesAsync("wf", new[] { RuleParameter.Create("input1", 1) });
            Assert.Equal(3, byParams.Count);
            Assert.Equal(3, byRuleParams.Count);
        }
    }
}
