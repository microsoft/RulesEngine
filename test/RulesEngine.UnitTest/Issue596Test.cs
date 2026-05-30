// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Actions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue596Test
    {
        private class CountingAction : ActionBase
        {
            public static int RunCount;
            public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
            {
                Interlocked.Increment(ref RunCount);
                return new ValueTask<object>("done");
            }
        }

        private static Workflow WorkflowWithAction() => new Workflow
        {
            WorkflowName = "wf",
            Rules = new[] {
                new Rule {
                    RuleName = "R",
                    Expression = "true",
                    Actions = new RuleActions {
                        OnSuccess = new ActionInfo { Name = "counting", Context = new Dictionary<string, object>() }
                    }
                }
            }
        };

        [Fact]
        public async Task AutoExecuteActions_True_RunsActions_DefaultBehavior()
        {
            CountingAction.RunCount = 0;
            var settings = new ReSettings
            {
                CustomActions = new Dictionary<string, Func<ActionBase>> { { "counting", () => new CountingAction() } }
            };
            var engine = new RulesEngine(new[] { WorkflowWithAction() }, settings);

            var results = await engine.ExecuteAllRulesAsync("wf", "x");

            Assert.True(results[0].IsSuccess);
            Assert.Equal(1, CountingAction.RunCount);
        }

        [Fact]
        public async Task AutoExecuteActions_False_EvaluatesRulesButSkipsActions()
        {
            CountingAction.RunCount = 0;
            var settings = new ReSettings
            {
                AutoExecuteActions = false,
                CustomActions = new Dictionary<string, Func<ActionBase>> { { "counting", () => new CountingAction() } }
            };
            var engine = new RulesEngine(new[] { WorkflowWithAction() }, settings);

            var results = await engine.ExecuteAllRulesAsync("wf", "x");

            // Rule still evaluated...
            Assert.True(results[0].IsSuccess);
            // ...but the action did NOT run automatically.
            Assert.Equal(0, CountingAction.RunCount);

            // Caller can still run the action selectively afterwards.
            var actionResult = await engine.ExecuteActionWorkflowAsync("wf", "R", new RuleParameter[0]);
            Assert.Equal("done", actionResult.Output);
            Assert.Equal(1, CountingAction.RunCount);
        }
    }
}
