// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class WorkflowsToInjectMutationTest
    {
        [Fact]
        public async Task WorkflowsToInject_MultipleExecutionsAndInputSignatures_DoesNotMutateRulesOrThrowDuplicateKey()
        {
            var baseWorkflow = new Workflow
            {
                WorkflowName = "BaseWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "InjectedRule1",
                        Expression = "input1.Value > 0"
                    }
                }
            };

            var childWorkflow = new Workflow
            {
                WorkflowName = "ChildWorkflow",
                WorkflowsToInject = new List<string> { "BaseWorkflow" },
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "ChildRule1",
                        Expression = "input1.Value < 100"
                    }
                }
            };

            var engine = new RulesEngine(new[] { baseWorkflow, childWorkflow });

            var firstRun = await engine.ExecuteAllRulesAsync("ChildWorkflow", new RuleParameter("input1", new { Value = 10 }));
            Assert.Equal(2, firstRun.Count);
            Assert.All(firstRun, r => Assert.True(r.IsSuccess));

            // Original workflow object's Rules collection should not have been mutated in-place.
            Assert.Single(childWorkflow.Rules);

            // Second execution with a different input parameter signature triggers RegisterRule again.
            // Previously, GetWorkflow mutated workflow.Rules on every call, duplicating InjectedRule1
            // and causing dictFunc.Add to throw ArgumentException ("An item with the same key has already been added").
            var secondRun = await engine.ExecuteAllRulesAsync("ChildWorkflow",
                new RuleParameter("input1", new { Value = 10 }),
                new RuleParameter("input2", new { Extra = "ok" }));
            Assert.Equal(2, secondRun.Count);
            Assert.All(secondRun, r => Assert.True(r.IsSuccess));
        }

        [Fact]
        public async Task WorkflowsToInject_CircularReference_ThrowsInvalidOperationExceptionInsteadOfStackOverflow()
        {
            var wfA = new Workflow
            {
                WorkflowName = "WfA",
                WorkflowsToInject = new List<string> { "WfB" },
                Rules = new List<Rule> { new Rule { RuleName = "R1", Expression = "true" } }
            };

            var wfB = new Workflow
            {
                WorkflowName = "WfB",
                WorkflowsToInject = new List<string> { "WfA" },
                Rules = new List<Rule> { new Rule { RuleName = "R2", Expression = "true" } }
            };

            var engine = new RulesEngine(new[] { wfA, wfB });

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await engine.ExecuteAllRulesAsync("WfA", new RuleParameter("input1", new { Value = 1 })));
        }
    }
}
