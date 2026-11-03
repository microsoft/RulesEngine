// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class CacheRemoveAndStaleGlobalParamsTest
    {
        [Fact]
        public async Task AddOrUpdateWorkflow_RemovingGlobalParams_ClearsStaleGlobalParamsDelegate()
        {
            var workflowV1 = new Workflow
            {
                WorkflowName = "DynamicGlobalsWorkflow",
                GlobalParams = new List<ScopedParam>
                {
                    new ScopedParam { Name = "bonus", Expression = "input1.Value * 2" }
                },
                Rules = new List<Rule>
                {
                    new Rule { RuleName = "Rule1", Expression = "bonus > 10" }
                }
            };

            var engine = new RulesEngine(new[] { workflowV1 });

            var resV1 = await engine.ExecuteAllRulesAsync("DynamicGlobalsWorkflow",
                new RuleParameter("input1", new { Value = 10 }));
            Assert.True(resV1[0].IsSuccess);
            Assert.True(resV1[0].Inputs.ContainsKey("bonus"));

            // Update workflow to remove GlobalParams completely.
            var workflowV2 = new Workflow
            {
                WorkflowName = "DynamicGlobalsWorkflow",
                GlobalParams = null,
                Rules = new List<Rule>
                {
                    new Rule { RuleName = "Rule1", Expression = "input1.Value > 5" }
                }
            };

            engine.AddOrUpdateWorkflow(workflowV2);

            var resV2 = await engine.ExecuteAllRulesAsync("DynamicGlobalsWorkflow",
                new RuleParameter("input1", new { Value = 10 }));
            Assert.True(resV2[0].IsSuccess);
            Assert.False(resV2[0].Inputs.ContainsKey("bonus"));
        }

        [Fact]
        public void RulesCache_RemoveWorkflow_DoesNotEvictOtherWorkflowWithSameNamePrefix()
        {
            var cache = new RulesCache(new ReSettings());
            var wfOrder = new Workflow { WorkflowName = "Order" };
            var wfOrderApproval = new Workflow { WorkflowName = "OrderApproval" };

            cache.AddOrUpdateWorkflows("Order", wfOrder);
            cache.AddOrUpdateWorkflows("OrderApproval", wfOrderApproval);

            var dummyRules = new Dictionary<string, RuleFunc<RuleResultTree>>();
            cache.AddOrUpdateCompiledRule("Order-input1_Object", "Order", dummyRules);
            cache.AddOrUpdateCompiledRule("OrderApproval-input1_Object", "OrderApproval", dummyRules);

            Assert.True(cache.AreCompiledRulesUpToDate("Order-input1_Object", "Order"));
            Assert.True(cache.AreCompiledRulesUpToDate("OrderApproval-input1_Object", "OrderApproval"));

            cache.Remove("Order");

            Assert.False(cache.AreCompiledRulesUpToDate("Order-input1_Object", "Order"));
            // "OrderApproval" must still have its compiled rules cached and up-to-date.
            Assert.True(cache.AreCompiledRulesUpToDate("OrderApproval-input1_Object", "OrderApproval"));
        }

        [Fact]
        public void RulesCache_RemoveWorkflow_DoesNotEvictWorkflowNamedWithRemovedNameAndHyphen()
        {
            // Compiled keys are "{workflow}-{params}", so "A-B-input1_Object" also starts with "A-".
            var cache = new RulesCache(new ReSettings());
            cache.AddOrUpdateWorkflows("A", new Workflow { WorkflowName = "A" });
            cache.AddOrUpdateWorkflows("A-B", new Workflow { WorkflowName = "A-B" });

            var dummyRules = new Dictionary<string, RuleFunc<RuleResultTree>>();
            cache.AddOrUpdateCompiledRule("A-input1_Object", "A", dummyRules);
            cache.AddOrUpdateCompiledRule("A-input1_Object-input2_Object", "A", dummyRules);
            cache.AddOrUpdateCompiledRule("A-B-input1_Object", "A-B", dummyRules);
            cache.AddOrUpdateGlobalParamsDelegate("A-input1_Object", _ => new Dictionary<string, object>());
            cache.AddOrUpdateGlobalParamsDelegate("A-B-input1_Object", _ => new Dictionary<string, object>());

            cache.Remove("A");

            Assert.Null(cache.GetCompiledRules("A-input1_Object"));
            Assert.Null(cache.GetCompiledRules("A-input1_Object-input2_Object"));
            Assert.Null(cache.GetGlobalParamsDelegate("A-input1_Object"));
            Assert.NotNull(cache.GetCompiledRules("A-B-input1_Object"));
            Assert.NotNull(cache.GetGlobalParamsDelegate("A-B-input1_Object"));
            Assert.True(cache.AreCompiledRulesUpToDate("A-B-input1_Object", "A-B"));

            cache.Remove("A-B");

            Assert.Null(cache.GetCompiledRules("A-B-input1_Object"));
            Assert.Null(cache.GetGlobalParamsDelegate("A-B-input1_Object"));
        }

        [Fact]
        public async Task AddOrUpdateWorkflow_GlobalParamsFailingToCompile_ClearsStaleGlobalParamsDelegate()
        {
            var workflowV1 = new Workflow
            {
                WorkflowName = "BrokenGlobalsWorkflow",
                GlobalParams = new List<ScopedParam>
                {
                    new ScopedParam { Name = "bonus", Expression = "input1.Value * 2" }
                },
                Rules = new List<Rule>
                {
                    new Rule { RuleName = "Rule1", Expression = "bonus > 10" }
                }
            };

            var engine = new RulesEngine(new[] { workflowV1 });
            var resV1 = await engine.ExecuteAllRulesAsync("BrokenGlobalsWorkflow",
                new RuleParameter("input1", new { Value = 10 }));
            Assert.True(resV1[0].Inputs.ContainsKey("bonus"));

            // V2's global param no longer compiles; V1's compiled delegate must not keep running.
            var workflowV2 = new Workflow
            {
                WorkflowName = "BrokenGlobalsWorkflow",
                GlobalParams = new List<ScopedParam>
                {
                    new ScopedParam { Name = "other", Expression = "input1.DoesNotExist * 2" }
                },
                Rules = new List<Rule>
                {
                    new Rule { RuleName = "Rule1", Expression = "other > 10" }
                }
            };
            engine.AddOrUpdateWorkflow(workflowV2);

            var resV2 = await engine.ExecuteAllRulesAsync("BrokenGlobalsWorkflow",
                new RuleParameter("input1", new { Value = 10 }));
            Assert.False(resV2[0].IsSuccess);
            Assert.False(resV2[0].Inputs.ContainsKey("bonus"));
        }
    }
}
