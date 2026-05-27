// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Actions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue576Test
    {
        private class NoContextAction : ActionBase
        {
            public static bool WasRun;
            public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
            {
                WasRun = true;
                return new ValueTask<object>("done");
            }
        }

        [Fact]
        public async Task CustomAction_WithNullContext_DoesNotThrow()
        {
            NoContextAction.WasRun = false;

            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "noctx",
                                Context = null
                            }
                        }
                    }
                }
            };
            var settings = new ReSettings
            {
                CustomActions = new Dictionary<string, Func<ActionBase>>
                {
                    { "noctx", () => new NoContextAction() }
                }
            };
            var engine = new RulesEngine(new[] { workflow }, settings);
            var results = await engine.ExecuteAllRulesAsync("wf", "x");

            Assert.True(NoContextAction.WasRun, "Custom action should have run even with null Context");
            Assert.Null(results[0].ActionResult?.Exception);
        }

        [Fact]
        public void ActionContext_NullDictionary_DoesNotThrow()
        {
            var ex = Record.Exception(() => new ActionContext(null, null));
            Assert.Null(ex);
        }
    }
}
