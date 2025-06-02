// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class CustomTypeFromRuleParameterTest
    {
        public class SomeType
        {
            public string SomeMethod() => "test";
        }

        [Fact]
        public async Task CustomTypeFromRuleParameter_ShouldBeAccessible()
        {
            // Arrange - test scenario from issue #667
            var workflow = new Workflow
            {
                WorkflowName = "Default",
                Rules = new Rule[]
                {
                    new()
                    {
                        RuleName = "unittestrule2",
                        Enabled = true,
                        Expression = "utils.SomeMethod() == \"test\"",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            };

            var reSettings = new ReSettings
            {
                AutoRegisterInputType = true,
                // CustomTypes is intentionally not set to test that RuleParameter types are registered
            };

            var bre = new RulesEngine(reSettings);
            bre.AddWorkflow(workflow);

            // Act - this should now work after the fix
            var rp = new RuleParameter("utils", new SomeType());
            var resultList = await bre.ExecuteAllRulesAsync("Default", rp);

            // Assert
            Assert.Single(resultList);
            var result = resultList[0];
            Assert.True(result.IsSuccess, $"Rule should succeed when custom type from RuleParameter is accessible. Error: {result.ExceptionMessage}");
        }

        [Fact]
        public async Task CustomTypeFromRuleParameter_WithExistingCustomTypes_ShouldMerge()
        {
            // Arrange
            var workflow = new Workflow
            {
                WorkflowName = "Test",
                Rules = new Rule[]
                {
                    new()
                    {
                        RuleName = "rule1",
                        Enabled = true,
                        Expression = "utils.SomeMethod() == \"test\" && ExpressionUtils.CheckContains(\"test\", \"test,other\")",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            };

            var reSettings = new ReSettings
            {
                AutoRegisterInputType = true,
                CustomTypes = new[] { typeof(HelperFunctions.ExpressionUtils) }
            };

            var bre = new RulesEngine(reSettings);
            bre.AddWorkflow(workflow);

            // Act - this should merge existing CustomTypes with RuleParameter types
            var rp = new RuleParameter("utils", new SomeType());
            var resultList = await bre.ExecuteAllRulesAsync("Test", rp);

            // Assert
            Assert.Single(resultList);
            Assert.True(resultList[0].IsSuccess, "Rule should succeed when both existing CustomTypes and RuleParameter types are accessible");
        }
    }
}