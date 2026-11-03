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
    public class NestedOperatorCaseInsensitiveTest
    {
        [Theory]
        [InlineData("and", true)]
        [InlineData("andalso", true)]
        [InlineData("or", true)]
        [InlineData("orelse", true)]
        public async Task ExecuteAllRulesAsync_LowercaseNestedOperators_CompileAndEvaluateCorrectly(string op, bool expected)
        {
            var workflow = new Workflow
            {
                WorkflowName = "CaseInsensitiveOperatorWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "ParentRule",
                        Operator = op,
                        Rules = new List<Rule>
                        {
                            new Rule { RuleName = "Child1", Expression = "input1.Value > 5" },
                            new Rule { RuleName = "Child2", Expression = "input1.Value < 20" }
                        }
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("CaseInsensitiveOperatorWorkflow",
                new RuleParameter("input1", new { Value = 10 }));

            Assert.Single(results);
            Assert.Equal(expected, results[0].IsSuccess);
            Assert.NotNull(results[0].ChildResults);
            Assert.Equal(2, new List<RuleResultTree>(results[0].ChildResults).Count);
        }
    }
}
