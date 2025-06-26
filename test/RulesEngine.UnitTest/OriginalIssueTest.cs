// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class OriginalIssueTest
    {
        [Fact]
        public async Task OriginalIssue_RuleChainingWithJsonConfig_ShouldWork()
        {
            // This test recreates the exact scenario from the GitHub issue
            var ruleJson = @"[
              {
                ""WorkflowName"": ""Tuning"",
                ""Rules"": [
                  {
                    ""RuleName"": ""EvaluationExpression"",
                    ""SuccessEvent"": ""EvaluationExpressionPassed"",
                    ""ErrorMessage"": ""EvaluationExpression not met."",
                    ""Expression"": ""metrics.current_value > 1000"",
                    ""RuleExpressionType"": ""LambdaExpression""
                  },
                  {
                    ""RuleName"": ""VerificationExpression"",
                    ""SuccessEvent"": ""VerificationExpressionPassed"",
                    ""ErrorMessage"": ""VerificationExpression failed."",
                    ""Expression"": ""@EvaluationExpression && metrics.cost_limit >= -1"",
                    ""RuleExpressionType"": ""LambdaExpression""
                  },
                  {
                    ""RuleName"": ""ActionExpression"",
                    ""SuccessEvent"": ""ActionExpressionApplied"",
                    ""ErrorMessage"": ""ActionExpression skipped."",
                    ""Expression"": ""VerificationExpressionPassed"",
                    ""RuleExpressionType"": ""LambdaExpression""
                  }
                ]
              }
            ]";

            var workflows = JsonConvert.DeserializeObject<List<Workflow>>(ruleJson);
            
            // Mock the MetricsProvider.GetSampleMetrics() call
            var metrics = new
            {
                current_value = 2000,  // > 1000, so EvaluationExpression should pass
                cost_limit = 100       // >= -1, so the cost_limit condition should pass
            };

            var inputs = new RuleParameter("metrics", metrics);
            var engine = new global::RulesEngine.RulesEngine(workflows.ToArray(), new ReSettings { EnableScopedParams = true });
            var results = await engine.ExecuteAllRulesAsync("Tuning", inputs);

            // Verify all rules pass as expected
            Assert.Equal(3, results.Count);
            
            // EvaluationExpression should pass (current_value 2000 > 1000)
            Assert.True(results[0].IsSuccess, 
                $"EvaluationExpression should pass but failed: {results[0].ExceptionMessage}");
            Assert.Equal("EvaluationExpression", results[0].Rule.RuleName);
            
            // VerificationExpression should pass (@EvaluationExpression is true AND cost_limit 100 >= -1)
            Assert.True(results[1].IsSuccess, 
                $"VerificationExpression should pass but failed: {results[1].ExceptionMessage}");
            Assert.Equal("VerificationExpression", results[1].Rule.RuleName);
            
            // ActionExpression should pass (VerificationExpressionPassed is available and true)
            Assert.True(results[2].IsSuccess, 
                $"ActionExpression should pass but failed: {results[2].ExceptionMessage}");
            Assert.Equal("ActionExpression", results[2].Rule.RuleName);

            // Verify success events are properly set
            Assert.Equal("EvaluationExpressionPassed", results[0].Rule.SuccessEvent);
            Assert.Equal("VerificationExpressionPassed", results[1].Rule.SuccessEvent);
            Assert.Equal("ActionExpressionApplied", results[2].Rule.SuccessEvent);
        }

        [Fact]
        public async Task OriginalIssue_FailureScenario_ShouldHandleCorrectly()
        {
            // Test the scenario where the first rule fails
            var ruleJson = @"[
              {
                ""WorkflowName"": ""Tuning"",
                ""Rules"": [
                  {
                    ""RuleName"": ""EvaluationExpression"",
                    ""SuccessEvent"": ""EvaluationExpressionPassed"",
                    ""ErrorMessage"": ""EvaluationExpression not met."",
                    ""Expression"": ""metrics.current_value > 1000"",
                    ""RuleExpressionType"": ""LambdaExpression""
                  },
                  {
                    ""RuleName"": ""VerificationExpression"",
                    ""SuccessEvent"": ""VerificationExpressionPassed"",
                    ""ErrorMessage"": ""VerificationExpression failed."",
                    ""Expression"": ""@EvaluationExpression && metrics.cost_limit >= -1"",
                    ""RuleExpressionType"": ""LambdaExpression""
                  }
                ]
              }
            ]";

            var workflows = JsonConvert.DeserializeObject<List<Workflow>>(ruleJson);
            
            // Create metrics where EvaluationExpression will fail
            var metrics = new
            {
                current_value = 500,   // < 1000, so EvaluationExpression should fail
                cost_limit = 100       // >= -1, but this won't matter because @EvaluationExpression is false
            };

            var inputs = new RuleParameter("metrics", metrics);
            var engine = new global::RulesEngine.RulesEngine(workflows.ToArray(), new ReSettings { EnableScopedParams = true });
            var results = await engine.ExecuteAllRulesAsync("Tuning", inputs);

            Assert.Equal(2, results.Count);
            
            // EvaluationExpression should fail (current_value 500 <= 1000)
            Assert.False(results[0].IsSuccess, "EvaluationExpression should fail");
            Assert.Equal("EvaluationExpression", results[0].Rule.RuleName);
            
            // VerificationExpression should fail (@EvaluationExpression is false, so entire expression is false)
            Assert.False(results[1].IsSuccess, "VerificationExpression should fail when EvaluationExpression fails");
            Assert.Equal("VerificationExpression", results[1].Rule.RuleName);
        }
    }
}