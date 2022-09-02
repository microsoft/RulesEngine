// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class ParameterNameChangeTest
    {
        [Fact]
        public async Task RunTwiceTest_ReturnsExpectedResults()
        {
            var workflow = new Workflow {
                WorkflowName = "ParameterNameChangeWorkflow",
                Rules = new Rule[] {
                    new Rule {
                        RuleName = "ParameterNameChangeRule",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "test.blah == 1"
                    }
                }
            };
            var engine = new RulesEngine();
            engine.AddOrUpdateWorkflow(workflow);

            dynamic dynamicBlah = new ExpandoObject();
            dynamicBlah.blah = (Int64)1;
            var input_pass = new RuleParameter("test", dynamicBlah);
            var input_fail = new RuleParameter("SOME_OTHER_NAME", dynamicBlah);
            // RuleParameter name matches expression, so should pass.
            var pass_results = await engine.ExecuteAllRulesAsync("ParameterNameChangeWorkflow", input_pass);
            // RuleParameter name DOES NOT MATCH expression, so should fail.
            var fail_results = await engine.ExecuteAllRulesAsync("ParameterNameChangeWorkflow", input_fail);
            Assert.True(pass_results.First().IsSuccess);
            Assert.False(fail_results.First().IsSuccess);
        }
    }
}
