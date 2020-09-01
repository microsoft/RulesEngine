﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using RulesEngine.Exceptions;
using RulesEngine.Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Xunit;
using Newtonsoft.Json.Converters;
using RulesEngine.HelperFunctions;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
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

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            var result = re.ExecuteRule("inputWorkflowReference",input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result,c => c.IsSuccess);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_SingleObject_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow", input1);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.DoesNotContain(result,c => c.IsSuccess);
        }

        [Theory]
        [InlineData("rules3.json")]
        public void ExecuteRule_ExceptionScenario_RulesInvalid(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result[0].ExceptionMessage) || string.IsNullOrWhiteSpace(result[0].ExceptionMessage));
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_ReturnsListOfRuleResultTree_ResultMessage(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow",input1, input2, input3);
            Assert.NotNull(result);
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
            dynamic input = GetInput1();

            Assert.Throws<ArgumentException>(() => { re.ExecuteRule("inputWorkflow1",  input); });
        }

        [Theory]
        [InlineData("rules1.json")]
        public void RemoveWorkflow_RemovesWorkflow(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            re.RemoveWorkflow("inputWorkflow");

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            Assert.Throws<ArgumentException>(() => re.ExecuteRule("inputWorkflow",input1, input2, input3 ));
        }


        [Theory]
        [InlineData("rules1.json")]
        public void ClearWorkflow_RemovesAllWorkflow(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            re.ClearWorkflows();

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            Assert.Throws<ArgumentException>(() => re.ExecuteRule("inputWorkflow", input1, input2, input3));
            Assert.Throws<ArgumentException>(() => re.ExecuteRule("inputWorkflowReference", input1, input2, input3));
        }


        [Theory]
        [InlineData("rules1.json")]
        [InlineData("rules2.json")]
        public void ExecuteRule_InputWithVariableProps_ReturnsResult(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result,c => c.IsSuccess);

            input3.hello = "world";

            result = re.ExecuteRule("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result,c => c.IsSuccess);
        }


        [Theory]
        [InlineData("rules4.json")]
        public void RulesEngine_Execute_Rule_For_Nested_Rule_Params_Returns_Success(string ruleFileName)
        {
            dynamic[] inputs = GetInputs4();

            var ruleParams = new List<RuleParameter>();

            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                var obj = Utils.GetTypedObject(input);
                ruleParams.Add(new RuleParameter($"input{i + 1}", obj));
            }

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), ruleFileName, SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");

            var fileData = File.ReadAllText(files[0]);
            var bre = new RulesEngine(JsonConvert.DeserializeObject<WorkflowRules[]>(fileData), null);
            var result = bre.ExecuteRule("inputWorkflow", ruleParams?.ToArray()); ;
            var ruleResult = result?.FirstOrDefault(r => string.Equals(r.Rule.RuleName, "GiveDiscount10", StringComparison.OrdinalIgnoreCase));
            Assert.True(ruleResult.IsSuccess);
        }

        [Theory]
        [InlineData("rules2.json")]
        public void ExecuteRule_ReturnsProperErrorOnMissingRuleParameter(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            var input1 = new RuleParameter("customName",GetInput1());
            var input2 = new RuleParameter("input2",GetInput2());
            var input3 = new RuleParameter("input3",GetInput3());

            List<RuleResultTree> result = re.ExecuteRule("inputWorkflow", input1,input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result.First().ChildResults, c => c.ExceptionMessage.Contains("Unknown identifier 'input1'"));
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
            return new RulesEngine(new string[] { data, injectWorkflowStr }, mockLogger.Object);
        }


        private dynamic GetInput1()
        {
            var converter = new ExpandoObjectConverter();
            var basicInfo = "{\"name\": \"Dishant\",\"email\": \"abc@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyalityFactor\": 3,\"totalPurchasesToDate\": 10000}";
            return JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
        }

        private dynamic GetInput2()
        {
            var converter = new ExpandoObjectConverter();
            var orderInfo = "{\"totalOrders\": 5,\"recurringItems\": 2}";
            return JsonConvert.DeserializeObject<ExpandoObject>(orderInfo, converter);
        }

        private dynamic GetInput3()
        {
            var converter = new ExpandoObjectConverter();
            var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";
            return JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo, converter);
        }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        /// <returns>
        /// The inputs.
        /// </returns>
        private static dynamic[] GetInputs4()
        {
            var basicInfo = "{\"name\": \"Dishant\",\"email\": \"abc@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyalityFactor\": 3,\"totalPurchasesToDate\": 70000}";
            var orderInfo = "{\"totalOrders\": 50,\"recurringItems\": 2}";
            var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";
            var laborCategoriesInput = "[{\"country\": \"india\", \"loyalityFactor\": 2, \"totalPurchasesToDate\": 20000}]";
            var currentLaborCategoryInput = "{\"CurrentLaborCategoryProp\":\"TestVal2\"}";

            var converter = new ExpandoObjectConverter();

            dynamic input1 = JsonConvert.DeserializeObject<List<RuleTestClass>>(laborCategoriesInput);
            dynamic input2 = JsonConvert.DeserializeObject<ExpandoObject>(currentLaborCategoryInput, converter);
            dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo, converter);
            dynamic input4 = JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
            dynamic input5 = JsonConvert.DeserializeObject<ExpandoObject>(orderInfo, converter);

            var inputs = new dynamic[]
                {
                    input1,
                    input2,
                    input3,
                    input4,
                    input5
                };

            return inputs;
        }

    }
}