// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Exceptions;
using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class DuplicateParamNameValidationTest
    {
        // Two GlobalParams with the same Name in the same workflow used to crash deep inside
        // RuleResultTree construction with "An item with the same key has already been added.
        // Key: dup". This change validates the workflow at AddWorkflow time and surfaces a clear
        // error pointing at the offending name.
        [Fact]
        public void AddWorkflow_DuplicateGlobalParamNames_ThrowsClearValidationError()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                GlobalParams = new[]
                {
                    new ScopedParam { Name = "dup", Expression = "1" },
                    new ScopedParam { Name = "dup", Expression = "2" }
                },
                Rules = new[] { new Rule { RuleName = "R", Expression = "dup == 1" } }
            };

            var engine = new RulesEngine();
            var ex = Assert.Throws<RuleValidationException>(() => engine.AddWorkflow(workflow));
            Assert.Contains("dup", ex.Message);
            Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddWorkflow_DuplicateLocalParamNames_ThrowsClearValidationError()
        {
            // Same protection for per-rule LocalParams.
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "dup == 1",
                        LocalParams = new[]
                        {
                            new ScopedParam { Name = "dup", Expression = "1" },
                            new ScopedParam { Name = "dup", Expression = "2" }
                        }
                    }
                }
            };

            var engine = new RulesEngine();
            var ex = Assert.Throws<RuleValidationException>(() => engine.AddWorkflow(workflow));
            Assert.Contains("dup", ex.Message);
            Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // A caller-supplied RuleParameter whose Name collides with a workflow GlobalParam's Name
        // used to surface as a cryptic "An item with the same key has already been added" deep
        // inside result-tree construction. After this change the per-rule ExceptionMessage
        // explicitly names the colliding identifier — same surfacing pattern as other
        // scoped-params errors that ExecuteAllRuleByWorkflow already catches and reports per rule.
        [Fact]
        public async Task ExecuteAllRulesAsync_CallerInputCollidesWithGlobalParam_SurfacesClearCollisionError()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                GlobalParams = new[]
                {
                    new ScopedParam { Name = "foo", Expression = "99" }
                },
                Rules = new[] { new Rule { RuleName = "R", Expression = "foo == 99" } }
            };
            var engine = new RulesEngine(new[] { workflow });

            var results = await engine.ExecuteAllRulesAsync("wf",
                new[] { RuleParameter.Create("foo", 42) });

            Assert.False(results[0].IsSuccess);
            Assert.Contains("foo", results[0].ExceptionMessage);
            Assert.Contains("collide", results[0].ExceptionMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("same key has already been added", results[0].ExceptionMessage);
        }

        // Sanity: no collision when names are distinct (regression guard for the validation logic).
        [Fact]
        public async Task ExecuteAllRulesAsync_DistinctNames_StillWorks()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                GlobalParams = new[]
                {
                    new ScopedParam { Name = "fromGlobal", Expression = "99" }
                },
                Rules = new[] { new Rule { RuleName = "R", Expression = "fromInput == 42 && fromGlobal == 99" } }
            };
            var engine = new RulesEngine(new[] { workflow });

            var results = await engine.ExecuteAllRulesAsync("wf",
                new[] { RuleParameter.Create("fromInput", 42) });

            Assert.True(results[0].IsSuccess);
        }
    }
}
