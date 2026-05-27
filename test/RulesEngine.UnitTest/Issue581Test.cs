// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue581Support
    {
        public class MyParam
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue581Test
    {
        [Fact]
        public async Task CustomParameterName_IsHonored()
        {
            var workflow = new Workflow
            {
                WorkflowName = "my_workflow",
                Rules = new[] {
                    new Rule {
                        RuleName = "MatchesFabrikam",
                        Enabled = true,
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "myValue.Value1 == \"Fabrikam\""
                    }
                }
            };

            var input = new Issue581Support.MyParam { Value1 = "Fabrikam", Value2 = "x" };
            var rp = new RuleParameter("myValue", input);
            var engine = new RulesEngine(new[] { workflow });

            var results = await engine.ExecuteAllRulesAsync("my_workflow", new[] { rp });

            Assert.True(results[0].IsSuccess,
                $"Expected success. Got: {results[0].ExceptionMessage}");
        }
    }
}
