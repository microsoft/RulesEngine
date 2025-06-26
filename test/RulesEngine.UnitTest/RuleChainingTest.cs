// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class RuleChainingTest
    {
        [Fact]
        public async Task RuleChaining_WithRuleReference_ShouldPassResult()
        {
            var workflow = new Workflow
            {
                WorkflowName = "TestChaining",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "EvaluationExpression",
                        SuccessEvent = "EvaluationExpressionPassed",
                        ErrorMessage = "EvaluationExpression not met.",
                        Expression = "input1.current_value > 1000",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    },
                    new Rule
                    {
                        RuleName = "VerificationExpression",
                        SuccessEvent = "VerificationExpressionPassed",
                        ErrorMessage = "VerificationExpression failed.",
                        Expression = "@EvaluationExpression && input1.cost_limit >= -1",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow }, new ReSettings { EnableScopedParams = true });

            var input = new
            {
                current_value = 2000,
                cost_limit = 100
            };

            var results = await engine.ExecuteAllRulesAsync("TestChaining", new RuleParameter("input1", input));

            // Both rules should pass - the first because current_value > 1000
            // and the second because the first rule passed (@EvaluationExpression = true) and cost_limit >= -1
            Assert.True(results[0].IsSuccess, "EvaluationExpression should pass");
            Assert.True(results[1].IsSuccess, "VerificationExpression should pass with rule reference");
        }

        [Fact]
        public async Task RuleChaining_WithSuccessEventReference_ShouldWork()
        {
            var workflow = new Workflow
            {
                WorkflowName = "TestChainingWithEvent",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "EvaluationExpression",
                        SuccessEvent = "EvaluationExpressionPassed",
                        ErrorMessage = "EvaluationExpression not met.",
                        Expression = "input1.current_value > 1000",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    },
                    new Rule
                    {
                        RuleName = "ActionExpression",
                        SuccessEvent = "ActionExpressionApplied",
                        ErrorMessage = "ActionExpression skipped.",
                        Expression = "EvaluationExpressionPassed",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow }, new ReSettings { EnableScopedParams = true });

            var input = new
            {
                current_value = 2000,
                cost_limit = 100
            };

            var results = await engine.ExecuteAllRulesAsync("TestChainingWithEvent", new RuleParameter("input1", input));

            // First rule should pass
            Assert.True(results[0].IsSuccess, "EvaluationExpression should pass");
            // Second rule should pass because it references the success event of the first rule
            Assert.True(results[1].IsSuccess, "ActionExpression should pass with success event reference");
        }

        [Fact]
        public async Task RuleChaining_OriginalIssueScenario_ShouldWork()
        {
            var workflow = new Workflow
            {
                WorkflowName = "Tuning",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "EvaluationExpression",
                        SuccessEvent = "EvaluationExpressionPassed",
                        ErrorMessage = "EvaluationExpression not met.",
                        Expression = "metrics.current_value > 1000",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    },
                    new Rule
                    {
                        RuleName = "VerificationExpression",
                        SuccessEvent = "VerificationExpressionPassed",
                        ErrorMessage = "VerificationExpression failed.",
                        Expression = "@EvaluationExpression && metrics.cost_limit >= -1",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    },
                    new Rule
                    {
                        RuleName = "ActionExpression",
                        SuccessEvent = "ActionExpressionApplied",
                        ErrorMessage = "ActionExpression skipped.",
                        Expression = "VerificationExpressionPassed",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow }, new ReSettings { EnableScopedParams = true });

            var metrics = new
            {
                current_value = 2000,
                cost_limit = 100
            };

            var results = await engine.ExecuteAllRulesAsync("Tuning", new RuleParameter("metrics", metrics));

            // All three rules should pass:
            // 1. EvaluationExpression: current_value (2000) > 1000 = true
            // 2. VerificationExpression: @EvaluationExpression (true) && cost_limit (100) >= -1 = true 
            // 3. ActionExpression: VerificationExpressionPassed (true) = true
            Assert.True(results[0].IsSuccess, "EvaluationExpression should pass");
            Assert.True(results[1].IsSuccess, "VerificationExpression should pass with rule reference");
            Assert.True(results[2].IsSuccess, "ActionExpression should pass with success event reference");
        }

        [Fact]
        public async Task RuleChaining_WithoutScopedParams_ShouldFail()
        {
            var workflow = new Workflow
            {
                WorkflowName = "TestChaining",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "EvaluationExpression",
                        SuccessEvent = "EvaluationExpressionPassed",
                        ErrorMessage = "EvaluationExpression not met.",
                        Expression = "input1.current_value > 1000",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    },
                    new Rule
                    {
                        RuleName = "VerificationExpression",
                        SuccessEvent = "VerificationExpressionPassed",
                        ErrorMessage = "VerificationExpression failed.",
                        Expression = "@EvaluationExpression && input1.cost_limit >= -1",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            };

            // Create engine WITHOUT EnableScopedParams
            var engine = new RulesEngine(new[] { workflow }, new ReSettings { EnableScopedParams = false });

            var input = new
            {
                current_value = 2000,
                cost_limit = 100
            };

            var results = await engine.ExecuteAllRulesAsync("TestChaining", new RuleParameter("input1", input));

            // First rule should pass, second should fail because rule chaining requires scoped params
            Assert.True(results[0].IsSuccess, "EvaluationExpression should pass");
            Assert.False(results[1].IsSuccess, "VerificationExpression should fail without scoped params enabled");
            Assert.Contains("@EvaluationExpression", results[1].ExceptionMessage);
        }
    }
}