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
    public class Issue624Test
    {
        private class ThrowingAction : ActionBase
        {
            public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
            {
                throw new InvalidOperationException("boom from custom action");
            }
        }

        [Fact]
        public async Task Repro_InvalidExpression_WithEnableExceptionAsErrorMessageFalse_ShouldThrow()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "BadRule",
                        Expression = "input1.NotARealProperty.Foo == 1"
                    }
                }
            };

            var engine = new RulesEngine(
                new[] { workflow },
                new ReSettings { EnableExceptionAsErrorMessage = false });

            await Assert.ThrowsAnyAsync<System.Exception>(async () =>
                await engine.ExecuteAllRulesAsync("wf", "hello"));
        }

        [Fact]
        public async Task Repro_RuntimeException_WithEnableExceptionAsErrorMessageFalse_ShouldThrow()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "DivideByZero",
                        Expression = "(1 / input1) == 1"
                    }
                }
            };

            var engine = new RulesEngine(
                new[] { workflow },
                new ReSettings { EnableExceptionAsErrorMessage = false });

            await Assert.ThrowsAnyAsync<System.Exception>(async () =>
                await engine.ExecuteAllRulesAsync("wf", 0));
        }

        [Fact]
        public async Task Repro_InvalidExpression_ExecuteActionWorkflowAsync_WithFlagFalse_ShouldThrow()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "BadRule",
                        Expression = "input1.NotARealProperty.Foo == 1"
                    }
                }
            };

            var engine = new RulesEngine(
                new[] { workflow },
                new ReSettings { EnableExceptionAsErrorMessage = false });

            await Assert.ThrowsAnyAsync<System.Exception>(async () =>
                await engine.ExecuteActionWorkflowAsync("wf", "BadRule",
                    new[] { RuleParameter.Create("input1", "hello") }));
        }

        [Fact]
        public async Task CustomActionThrows_WithEnableExceptionAsErrorMessageFalse_ShouldPropagate()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "RuleWithThrowingAction",
                        Expression = "true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "throwing",
                                Context = new Dictionary<string, object>()
                            }
                        }
                    }
                }
            };

            var settings = new ReSettings
            {
                EnableExceptionAsErrorMessage = false,
                CustomActions = new Dictionary<string, Func<ActionBase>>
                {
                    { "throwing", () => new ThrowingAction() }
                }
            };

            var engine = new RulesEngine(new[] { workflow }, settings);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await engine.ExecuteAllRulesAsync("wf", "x"));
        }

        [Fact]
        public async Task CustomActionThrows_WithEnableExceptionAsErrorMessageTrue_ShouldCaptureInResult()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "RuleWithThrowingAction",
                        Expression = "true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "throwing",
                                Context = new Dictionary<string, object>()
                            }
                        }
                    }
                }
            };

            var settings = new ReSettings
            {
                EnableExceptionAsErrorMessage = true,
                CustomActions = new Dictionary<string, Func<ActionBase>>
                {
                    { "throwing", () => new ThrowingAction() }
                }
            };

            var engine = new RulesEngine(new[] { workflow }, settings);

            var results = await engine.ExecuteAllRulesAsync("wf", "x");
            Assert.Single(results);
            Assert.NotNull(results[0].ActionResult);
            Assert.NotNull(results[0].ActionResult.Exception);
        }

        [Fact]
        public async Task Default_EnableExceptionAsErrorMessageTrue_ShouldNotThrow_ReportsAsErrorMessage()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "BadRule",
                        Expression = "input1.NotARealProperty.Foo == 1"
                    }
                }
            };

            // Default settings: EnableExceptionAsErrorMessage = true
            var engine = new RulesEngine(new[] { workflow });

            var results = await engine.ExecuteAllRulesAsync("wf", "hello");
            Assert.Single(results);
            Assert.False(results[0].IsSuccess);
            Assert.False(string.IsNullOrEmpty(results[0].ExceptionMessage));
        }
    }
}
