// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest;

[ExcludeFromCodeCoverage]
public class ParameterNameChangeTest
{
    [Fact]
    public async Task RunTwiceTest_ReturnsExpectedResults()
    {
        var workflow = new Workflow {
            WorkflowName = "ParameterNameChangeWorkflow",
            Rules = new Rule[] {
                new() {
                    RuleName = "ParameterNameChangeRule",
                    RuleExpressionType = RuleExpressionType.LambdaExpression,
                    Expression = "test.blah == 1"
                }
            }
        };
        var engine = new RulesEngine();
        engine.AddOrUpdateWorkflow(workflow);

        dynamic dynamicBlah = new ExpandoObject();
        dynamicBlah.blah = (long)1;
        var inputPass = new RuleParameter("test", dynamicBlah);
        var inputFail = new RuleParameter("SOME_OTHER_NAME", dynamicBlah);
        // RuleParameter name matches expression, so should pass.
        var passResults = await engine.ExecuteAllRulesAsync("ParameterNameChangeWorkflow", inputPass);
        // RuleParameter name DOES NOT MATCH expression, so should fail.
        var failResults = await engine.ExecuteAllRulesAsync("ParameterNameChangeWorkflow", inputFail);
        Assert.True(passResults[0].IsSuccess);
        Assert.False(failResults[0].IsSuccess);
    }
}
