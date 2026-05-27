// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
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
            var settings = new ReSettings {
                CustomTypes = new[]
                {
                    typeof(JObject),
                    typeof(JToken),
                    typeof(JArray)
                }
            };
            var parser = new RuleExpressionParser(settings);

            var json = @"{
               ""list"": [
                    { ""item1"": ""hello"", ""item3"": 1 },
                    { ""item2"": ""world"" }
               ]
            }";
            var input = JObject.Parse(json);

            var result1 = parser.Evaluate<object>(
                "Convert.ToInt32(input[\"list\"][0][\"item3\"]) == 1",
                new[] { new RuleParameter("input", input) }
            );
            Assert.True((bool)result1);

            var result2 = parser.Evaluate<object>(
                "Convert.ToString(input[\"list\"][1][\"item2\"]) == \"world\"",
                new[] { new RuleParameter("input", input) }
            );
            Assert.True((bool)result2);

            var result3 = parser.Evaluate<object>(
                "string.Concat(" +
                  "Convert.ToString(input[\"list\"][0][\"item1\"]), " +
                  "Convert.ToString(input[\"list\"][1][\"item2\"]))",
                new[] { new RuleParameter("input", input) }
            );
            Assert.Equal("helloworld", result3);
        }

        [Theory]
        [InlineData(false)]
        public void TestExpressionWithDifferentCompilerSettings(bool fastExpressionEnabled){
            var ruleParser = new RuleExpressionParser(new Models.ReSettings() { UseFastExpressionCompiler = fastExpressionEnabled });

            decimal? d1 = null;
            var result = ruleParser.Evaluate<bool>("d1 < 20", new[] { Models.RuleParameter.Create("d1", d1) });
            Assert.False(result);
        }

        [Fact]
        public void TestExpressionWithDictionaryParameter()
        {
            var parser = new RuleExpressionParser(new ReSettings());

            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "Formule", "Essentielle" }
            };

            var ruleParameters = new[] { RuleParameter.Create("_", payload) };

            var resultNotEqual = parser.Evaluate<bool>("Formule != \"Essentielle\"", ruleParameters);
            Assert.False(resultNotEqual);

            var resultEqual = parser.Evaluate<bool>("Formule == \"Essentielle\"", ruleParameters);
            Assert.True(resultEqual);
        }

        [Fact]
        public void TestExpressionWithDictionaryParameter_MultipleKeys()
        {
            var parser = new RuleExpressionParser(new ReSettings());

            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", 30 }
            };

            var ruleParameters = new[] { RuleParameter.Create("input", payload) };

            var result = parser.Evaluate<bool>("Name == \"John\" && Age == 30", ruleParameters);
            Assert.True(result);
        }
    }

    
}
