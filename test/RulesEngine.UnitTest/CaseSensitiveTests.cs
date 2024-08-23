// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class CaseSensitiveTests
    {
        [Theory]
        [InlineData(true,true,false)]
        [InlineData(false,true,true)]
        public async Task CaseSensitiveTest(bool caseSensitive, bool expected1, bool expected2)
        {
            var reSettings = new ReSettings {
                IsExpressionCaseSensitive = caseSensitive
            };


            var worflow = new Workflow {
                WorkflowName = "CaseSensitivityTest",
                Rules = new[] {
                    new Rule {
                        RuleName = "check same case1",
                        Expression = "input1 == \"hello\""
                    },
                    new Rule {
                        RuleName = "check same case2",
                        Expression = "INPUT1 == \"hello\""
                    }
                }
            };

            var re = new RulesEngine(new[] { worflow }, reSettings);
            var result = await re.ExecuteAllRulesAsync("CaseSensitivityTest", "hello");

            Assert.Equal(expected1, result[0].IsSuccess);
            Assert.Equal(expected2, result[1].IsSuccess);
        }




    }
}
