// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue606Support
    {
        public class Line
        {
            public IReadOnlyList<string> Modifiers { get; set; } = new List<string>();
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue606Test
    {
        [Fact]
        public async Task LambdaParameter_InAnyWithArrayLiteralContains_IsRecognized()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "ContainsModifier",
                        Expression = "line.Modifiers.Any(l => new [] {\"25\"}.Contains(l))"
                    }
                }
            };
            var input = new Issue606Support.Line { Modifiers = new List<string> { "25", "30" } };
            var rp = new RuleParameter("line", input);

            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("wf", new[] { rp });

            Assert.True(results[0].IsSuccess,
                $"Expected success. Got: {results[0].ExceptionMessage}");
        }
    }
}
