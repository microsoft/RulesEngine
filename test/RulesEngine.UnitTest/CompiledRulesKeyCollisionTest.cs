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
    public class CompiledRulesKeyCollisionTest
    {
        private class ScopeA
        {
            public class Item
            {
                public int Value { get; set; }
            }
        }

        private class ScopeB
        {
            public class Item
            {
                public int Value { get; set; }
            }
        }

        [Fact]
        public async Task ExecuteAllRulesAsync_DifferentGenericListTypes_DoNotCollideInCompiledRulesCache()
        {
            var workflow = new Workflow
            {
                WorkflowName = "GenericListWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "HasTwoElements",
                        Expression = "input1.Count == 2"
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });

            var intListResult = await engine.ExecuteAllRulesAsync("GenericListWorkflow",
                new RuleParameter("input1", new List<int> { 1, 2 }));
            Assert.True(intListResult[0].IsSuccess);

            // List<string>.Name is also "List`1". Previously this collided with the cached List<int>
            // compiled rule and failed with InvalidCastException.
            var stringListResult = await engine.ExecuteAllRulesAsync("GenericListWorkflow",
                new RuleParameter("input1", new List<string> { "a", "b" }));
            Assert.True(stringListResult[0].IsSuccess);
            Assert.True(string.IsNullOrEmpty(stringListResult[0].ExceptionMessage));
        }

        [Fact]
        public async Task ExecuteAllRulesAsync_SameShortTypeNameInDifferentScopes_DoNotCollideInCompiledRulesCache()
        {
            var workflow = new Workflow
            {
                WorkflowName = "SameShortNameWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "CheckValue",
                        Expression = "input1.Value == 42"
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });

            var resA = await engine.ExecuteAllRulesAsync("SameShortNameWorkflow",
                new RuleParameter("input1", new ScopeA.Item { Value = 42 }));
            Assert.True(resA[0].IsSuccess);

            var resB = await engine.ExecuteAllRulesAsync("SameShortNameWorkflow",
                new RuleParameter("input1", new ScopeB.Item { Value = 42 }));
            Assert.True(resB[0].IsSuccess);
            Assert.True(string.IsNullOrEmpty(resB[0].ExceptionMessage));
        }
    }
}
