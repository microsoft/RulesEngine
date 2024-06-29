// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using RulesEngine.ExpressionBuilders;
using System.Diagnostics.CodeAnalysis;
using Xunit;

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

        [Theory]
        [InlineData(false)]
        public void TestExpressionWithDifferentCompilerSettings(bool fastExpressionEnabled){
            var ruleParser = new RuleExpressionParser(new Models.ReSettings() { UseFastExpressionCompiler = fastExpressionEnabled });

            decimal? d1 = null;
            var result = ruleParser.Evaluate<bool>("d1 < 20", new[] { Models.RuleParameter.Create("d1", d1) });
            Assert.False(result);
        }
    }

    
}
