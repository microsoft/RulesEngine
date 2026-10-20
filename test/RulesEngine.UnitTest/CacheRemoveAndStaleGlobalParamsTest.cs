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
            cache.AddOrUpdateCompiledRule("Order-input1_Object", dummyRules);
            cache.AddOrUpdateCompiledRule("OrderApproval-input1_Object", dummyRules);

            Assert.True(cache.AreCompiledRulesUpToDate("Order-input1_Object", "Order"));
            Assert.True(cache.AreCompiledRulesUpToDate("OrderApproval-input1_Object", "OrderApproval"));

            cache.Remove("Order");

            Assert.False(cache.AreCompiledRulesUpToDate("Order-input1_Object", "Order"));
            // "OrderApproval" must still have its compiled rules cached and up-to-date.
            Assert.True(cache.AreCompiledRulesUpToDate("OrderApproval-input1_Object", "OrderApproval"));
        }
    }
}
