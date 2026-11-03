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
    // An EvaluateRule action forwards the source rule's inputs, which already contain the
    // evaluated GlobalParams. Chaining into a workflow that declares the same GlobalParams
    // used to fail with "The identifier 'x' was defined more than once".
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class Issue626Test
    {
        private static Workflow GetWorkflow(string globalName = "g", string chainedExpression = "g == 1",
            Dictionary<string, object> extraContext = null)
        {
            var context = new Dictionary<string, object> {
                { "workflowName", "wf" },
                { "ruleName", "r1" }
            };
            foreach (var kv in extraContext ?? new Dictionary<string, object>())
            {
                context[kv.Key] = kv.Value;
            }

            return new Workflow {
                WorkflowName = "wf",
                GlobalParams = new[] { new ScopedParam { Name = globalName, Expression = "input1.X" } },
                Rules = new[] {
                    new Rule {
                        RuleName = "r1",
                        Expression = chainedExpression,
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> { { "expression", globalName + " * 10" } }
                            }
                        }
                    },
                    new Rule {
                        RuleName = "r2",
                        Expression = "true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo { Name = "EvaluateRule", Context = context }
                        }
                    }
                }
            };
        }

        private static RuleParameter Input(int x) => new RuleParameter("input1", new { X = x });

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteActionWorkflowAsync_ChainIntoWorkflowWithGlobalParams_Succeeds(bool exceptionAsErrorMessage)
        {
            var engine = new RulesEngine(new[] { GetWorkflow() },
                new ReSettings { EnableExceptionAsErrorMessage = exceptionAsErrorMessage });

            var result = await engine.ExecuteActionWorkflowAsync("wf", "r2", new[] { Input(1) });

            Assert.Null(result.Exception);
            Assert.Equal(10, result.Output);
            Assert.All(result.Results, r => Assert.True(r.IsSuccess, r.ExceptionMessage));
        }

        [Fact]
        public async Task ExecuteAllRulesAsync_ChainIntoWorkflowWithGlobalParams_Succeeds()
        {
            var engine = new RulesEngine(new[] { GetWorkflow() });

            var results = await engine.ExecuteAllRulesAsync("wf", Input(1));

            var chained = results.Single(r => r.Rule.RuleName == "r2");
            Assert.True(chained.IsSuccess, chained.ExceptionMessage);
            Assert.Null(chained.ActionResult.Exception);
            Assert.Equal(10, chained.ActionResult.Output);
        }

        // A separate workflow with global "G" (= input1.X + 100) whose rule chains into targetWorkflow/targetRule.
        private static Workflow GetSourceWorkflow(string targetWorkflow, string targetRule)
        {
            return new Workflow {
                WorkflowName = "source",
                GlobalParams = new[] { new ScopedParam { Name = "G", Expression = "input1.X + 100" } },
                Rules = new[] {
                    new Rule {
                        RuleName = "start",
                        Expression = "G == 101",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "EvaluateRule",
                                Context = new Dictionary<string, object> { { "workflowName", targetWorkflow }, { "ruleName", targetRule } }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_CaseInsensitiveGlobalName_TargetGlobalIsRecomputed()
        {
            // Default IsExpressionCaseSensitive = false: the forwarded "G" (101) collides with the
            // target workflow's global "g" in the expression parser.
            var engine = new RulesEngine(new[] { GetSourceWorkflow("wf", "r1"), GetWorkflow() },
                new ReSettings { EnableExceptionAsErrorMessage = false });

            var result = await engine.ExecuteActionWorkflowAsync("source", "start", new[] { Input(1) });

            // "G" is dropped in favour of the target workflow's own "g", recomputed from input1 (1, not 101).
            Assert.Null(result.Exception);
            Assert.Equal(10, result.Output);
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_CaseSensitiveGlobalName_SourceGlobalIsKept()
        {
            // With IsExpressionCaseSensitive = true, "G" and "g" are different identifiers: keep "G".
            var engine = new RulesEngine(new[] { GetSourceWorkflow("wf", "r1"), GetWorkflow(chainedExpression: "g == 1 && G == 101") },
                new ReSettings { IsExpressionCaseSensitive = true, EnableExceptionAsErrorMessage = false });

            var result = await engine.ExecuteActionWorkflowAsync("source", "start", new[] { Input(1) });

            Assert.Null(result.Exception);
            Assert.Equal(10, result.Output);
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_TargetWithoutThatGlobal_SourceGlobalIsForwarded()
        {
            var target = new Workflow {
                WorkflowName = "plain",
                Rules = new[] { new Rule { RuleName = "check", Expression = "G == 101" } }
            };
            var engine = new RulesEngine(new[] { GetSourceWorkflow("plain", "check"), target },
                new ReSettings { EnableExceptionAsErrorMessage = false });

            var result = await engine.ExecuteActionWorkflowAsync("source", "start", new[] { Input(1) });

            Assert.Null(result.Exception);
            var check = Assert.Single(result.Results, r => r.Rule.RuleName == "check");
            Assert.True(check.IsSuccess, check.ExceptionMessage);
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_ScopedParamsDisabled_InputNamedLikeGlobalIsForwarded()
        {
            // With scoped params off the workflow's GlobalParams are never evaluated, so an input that
            // happens to share a global's name is a real input and must reach the chained rule.
            var workflow = GetWorkflow(chainedExpression: "g == 1");
            workflow.Rules.First().Actions = null;
            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { EnableScopedParams = false, EnableExceptionAsErrorMessage = false });

            var result = await engine.ExecuteActionWorkflowAsync("wf", "r2", new[] { Input(1), new RuleParameter("g", 1) });

            Assert.Null(result.Exception);
            Assert.All(result.Results, r => Assert.True(r.IsSuccess, r.ExceptionMessage));
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_AdditionalInputsWithGlobalParams_StillWork()
        {
            var workflow = GetWorkflow(
                chainedExpression: "g == 1 && extra == 2",
                extraContext: new Dictionary<string, object> {
                    { "additionalInputs", new List<ScopedParam> { new ScopedParam { Name = "extra", Expression = "g + 1" } } }
                });
            // Default settings: r1 cannot compile for the top-level call (no "extra" there), only for the chained one.
            var engine = new RulesEngine(new[] { workflow });

            var result = await engine.ExecuteActionWorkflowAsync("wf", "r2", new[] { Input(1) });

            Assert.Null(result.Exception);
            Assert.Equal(10, result.Output);
        }
    }
}
