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
    public class Issue711Test
    {
        [Fact]
        public async Task OutputExpression_AnonymousObjectWithCSharpStyleSyntax_FailsWithDynamicCoreError()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> {
                                    { "expression", "new { State = input1, CalculatedValue = 42 } as Result" }
                                }
                            }
                        }
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });

            var result = await engine.ExecuteActionWorkflowAsync("wf", "R",
                new[] { RuleParameter.Create("input1", "CA") });

            Assert.NotNull(result.Exception);
            Assert.Contains("Dynamic.Core", result.Exception.Message);
            Assert.Contains("as Alias", result.Exception.Message);
        }

        [Fact]
        public async Task OutputExpression_AnonymousObjectWithDynamicCoreSyntax_Works()
        {
            // Dynamic.Core syntax: parens, each field needs "as alias"
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> {
                                    { "expression", "new (input1 as State, 42 as CalculatedValue)" }
                                }
                            }
                        }
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });

            var result = await engine.ExecuteActionWorkflowAsync("wf", "R",
                new[] { RuleParameter.Create("input1", "CA") });

            Assert.Null(result.Exception);
            Assert.NotNull(result.Output);
        }
    }
}
