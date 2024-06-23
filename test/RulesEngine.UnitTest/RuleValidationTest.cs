// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Exceptions;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest;

[ExcludeFromCodeCoverage]
public class RuleValidationTest
{
    [Fact]
    public async Task NullExpressionithLambdaExpression_ReturnsExepectedResults()
    {
        var workflow = GetNullExpressionithLambdaExpressionWorkflow();
        var reSettings = new ReSettings();

        var action = () => {
            _ = new RulesEngine(workflow, reSettings);
            return Task.CompletedTask;
        };

        Exception ex = await Assert.ThrowsAsync<RuleValidationException>(action);

        Assert.Contains(Constants.LAMBDA_EXPRESSION_EXPRESSION_NULL_ERRMSG, ex.Message);
    }

    [Fact]
    public async Task NestedRulesWithMissingOperator_ReturnsExepectedResults()
    {
        var workflow = GetEmptyOperatorWorkflow();
        var reSettings = new ReSettings();

        var action = () => {
            _ = new RulesEngine(workflow, reSettings);
            return Task.CompletedTask;
        };

        Exception ex = await Assert.ThrowsAsync<RuleValidationException>(action);

        Assert.Contains(Constants.OPERATOR_RULES_ERRMSG, ex.Message);
    }

    private Workflow[] GetNullExpressionithLambdaExpressionWorkflow()
    {
        return new[] {
            new Workflow {
                WorkflowName = "NestedRulesTest",
                Rules = new Rule[] {
                    new() { RuleName = "TestRule", RuleExpressionType = RuleExpressionType.LambdaExpression }
                }
            }
        };
    }

    private Workflow[] GetEmptyOperatorWorkflow()
    {
        return new[] {
            new Workflow {
                WorkflowName = "NestedRulesTest",
                Rules = new Rule[] {
                    new() {
                        RuleName = "AndRuleTrueFalse",
                        Expression = "true == true",
                        Rules = new Rule[] {
                            new() { RuleName = "trueRule1", Expression = "input1.TrueValue == true" },
                            new() { RuleName = "falseRule1", Expression = "input1.TrueValue == false" }
                        }
                    }
                }
            }
        };
    }
}
