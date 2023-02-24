// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using RulesEngine.ExpressionBuilders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;

namespace RulesEngine.UnitTest.RuleExpressionParserTests
{
    [ExcludeFromCodeCoverage]
    public class RuleExpressionParserTests
    {
        public RuleExpressionParserTests() { 
           
        
        }


        [Fact]
        public void TestExpressionWithJObject()
        {
            var ruleParser = new RuleExpressionParser(new Models.ReSettings());

            var inputStr = @"{
               ""list"": [
                    { ""item1"": ""hello"",
                        ""item3"": 1
                        },
                    {
                        ""item2"": ""world""
                        }
                ]
            }";


            var input = JObject.Parse(inputStr);


            var value = ruleParser.Evaluate<object>("input.list[0].item3 == 1", new[] { new Models.RuleParameter("input", input) });

            Assert.Equal(true,
                         value);


            var value2 = ruleParser.Evaluate<object>("input.list[1].item2 == \"world\"", new[] { new Models.RuleParameter("input", input) });

            Assert.Equal(true,
                         value2);


            var value3= ruleParser.Evaluate<object>("string.Concat(input.list[0].item1,input.list[1].item2)", new[] { new Models.RuleParameter("input", input) });

            Assert.Equal("helloworld", value3);
        }
    }
}
