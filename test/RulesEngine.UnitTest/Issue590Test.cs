// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue590Test
    {
        public class FlakyInput
        {
            private int _counter;
            private string _simpleProp;
            public string SimpleProp
            {
                get
                {
                    if (_counter++ == 0)
                    {
                        throw new ArgumentException("first-call-failure");
                    }
                    return _simpleProp;
                }
                set { _simpleProp = value; }
            }
        }

        [Fact]
        public async Task ExceptionMessage_FromPriorRunDoesNotLeakIntoNextSuccessfulRun()
        {
            var input = new FlakyInput { SimpleProp = "simpleProp" };
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "CheckSimpleProp",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "SimpleProp == \"simpleProp\"",
                        ErrorMessage = "should not leak"
                    }
                }
            };
            var settings = new ReSettings { UseFastExpressionCompiler = false };
            var engine = new RulesEngine(new[] { workflow }, settings);

            var firstRun = await engine.ExecuteAllRulesAsync("wf", input);
            Assert.False(firstRun[0].IsSuccess);
            Assert.False(string.IsNullOrEmpty(firstRun[0].ExceptionMessage));

            var secondRun = await engine.ExecuteAllRulesAsync("wf", input);
            Assert.True(secondRun[0].IsSuccess,
                $"Second run should succeed. Got ExceptionMessage = `{secondRun[0].ExceptionMessage}`");
            Assert.True(string.IsNullOrEmpty(secondRun[0].ExceptionMessage),
                $"Second run ExceptionMessage should be empty. Got `{secondRun[0].ExceptionMessage}`");
        }
    }
}
