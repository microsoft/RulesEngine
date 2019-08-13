// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using RulesEngine.Exceptions;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    public class RulesEngineTest
    {
        [Theory]
        [InlineData("rules1.json")]
        public void RulesEngine_New_ReturnsNotNull(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            Assert.NotNull(re);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void RulesEngine_InjectedRules_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input = GetInput();

            dynamic input2 = GetInput();
            input2.value1 = "val1";

            var result = re.ExecuteRule("inputWorkflowReference", new List<dynamic>() { input, input2 }.AsEnumerable(), new object[] { });
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input = GetInput();

            dynamic input2 = GetInput();
            input2.value1 = "val1";

            var result = re.ExecuteRule("inputWorkflow", new List<dynamic>() { input, input2 }.AsEnumerable(), new object[] { });
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_SingleObject_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input = GetInput();

            dynamic input2 = GetInput();
            input2.value1 = "val1";

            var result = re.ExecuteRule("inputWorkflow",input);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_ReturnsListOfRuleResultTree_ResultMessage(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input = GetInput();

            dynamic input2 = GetInput();
            input2.value1 = "val1";

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow", input);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.NotNull(result.First().GetMessages());
            Assert.NotNull(result.First().GetMessages().WarningMessages);
        }

        [Fact]
        public void RulesEngine_New_IncorrectJSON_ThrowsException()
        {

            Assert.Throws<RuleValidationException>(() =>
            {
                var workflow = new WorkflowRules();
                var re = CreateRulesEngine(workflow);
            });


            Assert.Throws<RuleValidationException>(() =>
            {
                var workflow = new WorkflowRules() { WorkflowName = "test" };
                var re = CreateRulesEngine(workflow);
            });



        }


        [Theory]
        [InlineData("rules1.json")]
        public void ExecuteRule_InvalidWorkFlow_ThrowsException(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input = GetInput();

            Assert.Throws<ArgumentException>(() => { re.ExecuteRule("inputWorkflow1", new List<dynamic>() { input }.AsEnumerable(), new object[] { }); });
        }

        [Theory]
        [InlineData("rules1.json")]
        [InlineData("rules2.json")]
        public void ExecuteRule_InputWithVariableProps_ReturnsResult(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input = GetInput();
            dynamic input2 = GetInput();
            input2.valueX = "hello";

            var result = re.ExecuteRule("inputWorkflow", new List<dynamic>() { input, input2 }.AsEnumerable(), new object[] { });
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
        }

        private RulesEngine CreateRulesEngine(WorkflowRules workflow)
        {
            var json = JsonConvert.SerializeObject(workflow);
            return new RulesEngine(new string[] { json }, null);
        }

        private RulesEngine GetRulesEngine(string filename)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory() as string, "TestData", filename);
            var data = File.ReadAllText(filePath);

            var injectWorkflow = new WorkflowRules
            {
                WorkflowName = "inputWorkflowReference",
                WorkflowRulesToInject = new List<string> { "inputWorkflow" }
            };

            var injectWorkflowStr = JsonConvert.SerializeObject(injectWorkflow);
            var mockLogger = new Mock<ILogger>();
            return new RulesEngine(new string[] { data, injectWorkflowStr}, mockLogger.Object);
        }

        private dynamic GetInput()
        {
            dynamic input = new ExpandoObject();
            input.value1 = "value1";
            input.value2 = "value2";
            input.value3 = 1;
            input.value4 = new { subValue = "subValue", nullValue = default(string) };
            return input;
        }
    }
}