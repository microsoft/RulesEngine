// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue717TestSupport
    {
        public class SimpleInput
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }

        public class ReturnTypeForIssue717
        {
            public int val { get; set; }
        }

        public static class FunctionsForIssue717
        {
            public static object CheckValueAsObject(SimpleInput input)
            {
                if (input != null && input.Value > 5)
                {
                    return new ReturnTypeForIssue717 { val = 1 };
                }
                return new ReturnTypeForIssue717 { val = 0 };
            }

            public static ReturnTypeForIssue717 CheckValueTyped(SimpleInput input)
            {
                if (input != null && input.Value > 5)
                {
                    return new ReturnTypeForIssue717 { val = 1 };
                }
                return new ReturnTypeForIssue717 { val = 0 };
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue717Test
    {
        [Fact]
        public async Task CustomMethodReturningTypedClass_Works()
        {
            // Documenting the working scenario as a regression guard for the workaround.
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R1",
                        Expression = "FunctionsForIssue717.CheckValueTyped(myObj).val == 1"
                    }
                }
            };
            var settings = new ReSettings
            {
                CustomTypes = new[]
                {
                    typeof(Issue717TestSupport.SimpleInput),
                    typeof(Issue717TestSupport.ReturnTypeForIssue717),
                    typeof(Issue717TestSupport.FunctionsForIssue717)
                }
            };
            var engine = new RulesEngine(new[] { workflow }, settings);
            var input = new Issue717TestSupport.SimpleInput { Value = 10, Name = "x" };
            var results = await engine.ExecuteAllRulesAsync("wf",
                new[] { RuleParameter.Create("myObj", input) });
            Assert.True(results[0].IsSuccess);
        }

        [Fact]
        public async Task CustomMethodReturningObject_ProducesHelpfulErrorMessage()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R1",
                        Expression = "FunctionsForIssue717.CheckValueAsObject(myObj).val == 1"
                    }
                }
            };
            var settings = new ReSettings
            {
                CustomTypes = new[]
                {
                    typeof(Issue717TestSupport.SimpleInput),
                    typeof(Issue717TestSupport.ReturnTypeForIssue717),
                    typeof(Issue717TestSupport.FunctionsForIssue717)
                }
            };
            var engine = new RulesEngine(new[] { workflow }, settings);
            var input = new Issue717TestSupport.SimpleInput { Value = 10, Name = "x" };
            var results = await engine.ExecuteAllRulesAsync("wf",
                new[] { RuleParameter.Create("myObj", input) });

            Assert.False(results[0].IsSuccess);
            // Error must explicitly mention the return type / typed-return guidance.
            Assert.True(
                results[0].ExceptionMessage.IndexOf("return type", StringComparison.OrdinalIgnoreCase) >= 0,
                $"Expected helpful hint about return type. Got: {results[0].ExceptionMessage}");
        }
    }
}
