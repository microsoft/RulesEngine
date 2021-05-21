// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Exceptions;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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
        public async Task RulesEngine_InjectedRules_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            var result = await re.ExecuteAllRulesAsync("inputWorkflowReference", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
        }

        [Theory]
        [InlineData("rules2.json")]
        public async Task ExecuteRule_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result, c => c.IsSuccess);
        }


        [Theory]
        [InlineData("rules2.json")]
        public void GetAllRegisteredWorkflows_ReturnsListOfAllWorkflows(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            var workflows = re.GetAllRegisteredWorkflowNames();

            Assert.NotNull(workflows);
            Assert.Equal(2, workflows.Count);
            Assert.Contains("inputWorkflow", workflows);
        }

        [Fact]
        public void GetAllRegisteredWorkflows_NoWorkflow_ReturnsEmptyList()
        {
            var re = new RulesEngine();
            var workflows = re.GetAllRegisteredWorkflowNames();

            Assert.NotNull(workflows);
            Assert.Empty(workflows);
        }

        [Theory]
        [InlineData("rules2.json")]
        public async Task ExecuteRule_ManyInputs_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            dynamic input4 = GetInput1();
            dynamic input5 = GetInput2();
            dynamic input6 = GetInput3();

            dynamic input7 = GetInput1();
            dynamic input8 = GetInput2();
            dynamic input9 = GetInput3();

            dynamic input10 = GetInput1();
            dynamic input11 = GetInput2();
            dynamic input12 = GetInput3();

            dynamic input13 = GetInput1();
            dynamic input14 = GetInput2();
            dynamic input15 = GetInput3();


            dynamic input16 = GetInput1();
            dynamic input17 = GetInput2();
            dynamic input18 = GetInput3();

            List<RuleResultTree> result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3, input4, input5, input6, input7, input8, input9, input10, input11, input12, input13, input14, input15, input16, input17, input18);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result, c => c.IsSuccess);
        }


        [Theory]
        [InlineData("rules2.json")]
        public async Task ExecuteRule_CalledMultipleTimes_ReturnsSameResult(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result1 = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result1);
            Assert.IsType<List<RuleResultTree>>(result1);
            Assert.Contains(result1, c => c.IsSuccess);

            List<RuleResultTree> result2 = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result2);
            Assert.IsType<List<RuleResultTree>>(result2);
            Assert.Contains(result2, c => c.IsSuccess);

            var expected = result1.Select(c => new { c.Rule.RuleName, c.IsSuccess });
            var actual = result2.Select(c => new { c.Rule.RuleName, c.IsSuccess });
            Assert.Equal(expected, actual);


        }

        [Theory]
        [InlineData("rules2.json")]
        public async Task ExecuteRule_SingleObject_ReturnsListOfRuleResultTree(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = await re.ExecuteAllRulesAsync("inputWorkflow", input1);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.DoesNotContain(result, c => c.IsSuccess);
        }

        [Theory]
        [InlineData("rules3.json")]
        public async Task ExecuteRule_ExceptionScenario_RulesInvalid(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result[0].ExceptionMessage) || string.IsNullOrWhiteSpace(result[0].ExceptionMessage));
        }

        [Theory]
        [InlineData("rules2.json")]
        [Obsolete]
        public async Task ExecuteRule_ReturnsListOfRuleResultTree_ResultMessage(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.NotNull(result.First().GetMessages());
            Assert.NotNull(result.First().GetMessages().WarningMessages);
        }

        [Fact]
        public void RulesEngine_New_IncorrectJSON_ThrowsException()
        {
            Assert.Throws<RuleValidationException>(() => {
                var workflow = new WorkflowRules();
                var re = CreateRulesEngine(workflow);
            });

            Assert.Throws<RuleValidationException>(() => {
                var workflow = new WorkflowRules() { WorkflowName = "test" };
                var re = CreateRulesEngine(workflow);
            });
        }


        [Theory]
        [InlineData("rules1.json")]
        public async Task ExecuteRule_InvalidWorkFlow_ThrowsException(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input = GetInput1();

            await Assert.ThrowsAsync<ArgumentException>(async () => { await re.ExecuteAllRulesAsync("inputWorkflow1", input); });
        }

        [Theory]
        [InlineData("rules1.json")]
        public async Task RemoveWorkflow_RemovesWorkflow(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            var result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            re.RemoveWorkflow("inputWorkflow");

            await Assert.ThrowsAsync<ArgumentException>(async () => await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3));
        }


        [Theory]
        [InlineData("rules1.json")]
        public async Task ClearWorkflow_RemovesAllWorkflow(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);
            re.ClearWorkflows();

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            await Assert.ThrowsAsync<ArgumentException>(async () => await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3));
            await Assert.ThrowsAsync<ArgumentException>(async () => await re.ExecuteAllRulesAsync("inputWorkflowReference", input1, input2, input3));
        }

        [Theory]
        [InlineData("rules1.json")]
        [InlineData("rules2.json")]
        public async Task ExecuteRule_InputWithVariableProps_ReturnsResult(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = GetInput1();
            dynamic input2 = GetInput2();
            dynamic input3 = GetInput3();

            List<RuleResultTree> result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result, c => c.IsSuccess);

            input3.hello = "world";

            result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result, c => c.IsSuccess);
        }

        [Theory]
        [InlineData("rules4.json")]
        public async Task RulesEngine_Execute_Rule_For_Nested_Rule_Params_Returns_Success(string ruleFileName)
        {
            var inputs = GetInputs4();

            var ruleParams = new List<RuleParameter>();

            for (var i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                var obj = Utils.GetTypedObject(input);
                ruleParams.Add(new RuleParameter($"input{i + 1}", obj));
            }

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), ruleFileName, SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                throw new Exception("Rules not found.");
            }

            var fileData = File.ReadAllText(files[0]);
            var bre = new RulesEngine(JsonConvert.DeserializeObject<WorkflowRules[]>(fileData), null);
            var result = await bre.ExecuteAllRulesAsync("inputWorkflow", ruleParams?.ToArray());
            var ruleResult = result?.FirstOrDefault(r => string.Equals(r.Rule.RuleName, "GiveDiscount10", StringComparison.OrdinalIgnoreCase));
            Assert.True(ruleResult.IsSuccess);
        }

        [Theory]
        [InlineData("rules2.json")]
        public async Task ExecuteRule_ReturnsProperErrorOnMissingRuleParameter(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            var input1 = new RuleParameter("customName", GetInput1());
            var input2 = new RuleParameter("input2", GetInput2());
            var input3 = new RuleParameter("input3", GetInput3());

            var result = await re.ExecuteAllRulesAsync("inputWorkflow", input1, input2, input3);
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.Contains(result.First().ChildResults, c => c.ExceptionMessage.Contains("Unknown identifier 'input1'"));
        }

        [Theory]
        [InlineData("rules5.json", "hello", true)]
        [InlineData("rules5.json", null, false)]
        public async Task ExecuteRule_WithInjectedUtils_ReturnsListOfRuleResultTree(string ruleFileName, string propValue, bool expectedResult)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = new ExpandoObject();
            if (propValue != null)
            {
                input1.Property1 = propValue;
            }

            if (propValue == null)
            {
                input1.Property1 = null;
            }

            var utils = new TestInstanceUtils();

            var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1), new RuleParameter("utils", utils));
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.All(result, c => Assert.Equal(expectedResult, c.IsSuccess));
        }

        [Theory]
        [InlineData("rules6.json")]
        public async Task ExecuteRule_RuleWithMethodExpression_ReturnsSucess(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            Func<bool> func = () => true;

            dynamic input1 = new ExpandoObject();
            input1.Property1 = "hello";
            input1.Boolean = false;
            input1.Method = func;

            var utils = new TestInstanceUtils();

            var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1));
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.All(result, c => Assert.True(c.IsSuccess));
        }

        [Theory]
        [InlineData("rules7.json")]
        public async Task ExecuteRule_RuleWithUnaryExpression_ReturnsSucess(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = new ExpandoObject();
            input1.Boolean = false;

            var utils = new TestInstanceUtils();

            var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1));
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.All(result, c => Assert.True(c.IsSuccess));
        }

        [Theory]
        [InlineData("rules8.json")]
        public async Task ExecuteRule_RuleWithMemberAccessExpression_ReturnsSucess(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName);

            dynamic input1 = new ExpandoObject();
            input1.Boolean = false;

            var utils = new TestInstanceUtils();

            var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1));
            Assert.NotNull(result);
            Assert.IsType<List<RuleResultTree>>(result);
            Assert.All(result, c => Assert.False(c.IsSuccess));
        }

        [Theory]
        [InlineData("rules9.json")]
        public async Task ExecuteRule_MissingMethodInExpression_ReturnsException(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName, new ReSettings() { EnableExceptionAsErrorMessage = false });

            dynamic input1 = new ExpandoObject();
            input1.Data = new { TestProperty = "" };
            input1.Boolean = false;

            var utils = new TestInstanceUtils();

            await Assert.ThrowsAsync<RuleException>(async () => {
                var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1));
            });
        }

        [Theory]
        [InlineData("rules9.json")]
        public async Task ExecuteRule_CompilationException_ReturnsAsErrorMessage(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName, new ReSettings() { EnableExceptionAsErrorMessage = true });

            dynamic input1 = new ExpandoObject();
            input1.Data = new { TestProperty = "" };
            input1.Boolean = false;

            var utils = new TestInstanceUtils();
            var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1));

            Assert.NotNull(result);
            Assert.StartsWith("Exception while parsing expression", result[1].ExceptionMessage);
        }

        [Theory]
        [InlineData("rules9.json")]
        public async Task ExecuteRuleWithIgnoreException_CompilationException_DoesNotReturnsAsErrorMessage(string ruleFileName)
        {
            var re = GetRulesEngine(ruleFileName, new ReSettings() { EnableExceptionAsErrorMessage = true , IgnoreException = true});

            dynamic input1 = new ExpandoObject();
            input1.Data = new { TestProperty = "" };
            input1.Boolean = false;

            var utils = new TestInstanceUtils();
            var result = await re.ExecuteAllRulesAsync("inputWorkflow", new RuleParameter("input1", input1));

            Assert.NotNull(result);
            Assert.False(result[1].ExceptionMessage.StartsWith("Exception while parsing expression"));
        }


        [Fact]
        public async Task ExecuteRule_RuntimeError_ShouldReturnAsErrorMessage()
        {

            var workflow = new WorkflowRules {
                WorkflowName = "TestWorkflow",
                Rules = new[] {
                    new Rule {
                        RuleName = "ruleWithRuntimeError",
                        Expression = "input1.Country.ToLower() == \"india\""
                    }
                }
            };

            var re = new RulesEngine(new[] { workflow }, null, null);
            var input = new RuleTestClass {
                Country = null
            };

            var result = await re.ExecuteAllRulesAsync("TestWorkflow", input);

            Assert.NotNull(result);
            Assert.All(result, rule => Assert.False(rule.IsSuccess));
            Assert.All(result, rule => Assert.StartsWith("Error while executing rule :", rule.ExceptionMessage));
        }


        [Fact]
        public async Task ExecuteRule_RuntimeError_ThrowsException()
        {

            var workflow = new WorkflowRules {
                WorkflowName = "TestWorkflow",
                Rules = new[] {
                    new Rule {
                        RuleName = "ruleWithRuntimeError",
                        Expression = "input1.Country.ToLower() == \"india\""
                    }
                }
            };

            var re = new RulesEngine(new[] { workflow }, null, new ReSettings {
                EnableExceptionAsErrorMessage = false
            });
            var input = new RuleTestClass {
                Country = null
            };

            _ = await Assert.ThrowsAsync<RuleException>(async () => await re.ExecuteAllRulesAsync("TestWorkflow", input));
            
        }

        [Fact]
        public async Task ExecuteRule_RuntimeError_IgnoreException_DoesNotReturnException()
        {

            var workflow = new WorkflowRules {
                WorkflowName = "TestWorkflow",
                Rules = new[] {
                    new Rule {
                        RuleName = "ruleWithRuntimeError",
                        Expression = "input1.Country.ToLower() == \"india\""
                    }
                }
            };

            var re = new RulesEngine(new[] { workflow }, null, new ReSettings {
                IgnoreException = true
            });
            var input = new RuleTestClass {
                Country = null
            };

            var result = await re.ExecuteAllRulesAsync("TestWorkflow", input);

            Assert.NotNull(result);
            Assert.All(result, rule => Assert.False(rule.IsSuccess));
            Assert.All(result, rule => Assert.Empty(rule.ExceptionMessage));
        }


        [Fact]
        public async Task RemoveWorkFlow_ShouldRemoveAllCompiledCache()
        {
            var workflow = new WorkflowRules {
                WorkflowName = "Test",
                Rules = new Rule[]{
                    new Rule {
                        RuleName = "RuleWithLocalParam",
                        LocalParams = new List<LocalParam> {
                            new LocalParam {
                                Name = "lp1",
                                Expression = "true"
                            }
                        },
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "lp1 ==  true"
                    }
                }
            };

            var re = new RulesEngine();
            re.AddWorkflow(workflow);

            var result1 = await re.ExecuteAllRulesAsync("Test","hello");
            Assert.True(result1.All(c => c.IsSuccess));

            re.RemoveWorkflow("Test");
            workflow.Rules.First().LocalParams.First().Expression = "false";

            re.AddWorkflow(workflow);
            var result2 = await re.ExecuteAllRulesAsync("Test", "hello");
            Assert.True(result2.All(c => c.IsSuccess == false));
        }

        [Fact]
        public async Task ClearWorkFlow_ShouldRemoveAllCompiledCache()
        {
            var workflow = new WorkflowRules {
                WorkflowName = "Test",
                Rules = new Rule[]{
                    new Rule {
                        RuleName = "RuleWithLocalParam",
                        LocalParams = new LocalParam[] {
                            new LocalParam {
                                Name = "lp1",
                                Expression = "true"
                            }
                        },
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "lp1 ==  true"
                    }
                }
            };

            var re = new RulesEngine();
            re.AddWorkflow(workflow);

            var result1 = await re.ExecuteAllRulesAsync("Test", "hello");
            Assert.True(result1.All(c => c.IsSuccess));

            re.ClearWorkflows();
            workflow.Rules.First().LocalParams.First().Expression = "false";

            re.AddWorkflow(workflow);
            var result2 = await re.ExecuteAllRulesAsync("Test", "hello");
            Assert.True(result2.All(c => c.IsSuccess == false));
        }

        [Fact]
        public async Task ExecuteRule_WithNullInput_ShouldNotThrowException()
        {
            var workflow = new WorkflowRules {
                WorkflowName = "Test",
                Rules = new Rule[]{
                    new Rule {
                        RuleName = "RuleWithLocalParam",

                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "input1 == null || input1.hello.world = \"wow\""
                    }
                }
            };

            var re = new RulesEngine();
            re.AddWorkflow(workflow);

            var result1 = await re.ExecuteAllRulesAsync("Test", new RuleParameter("input1", value:null));
            Assert.True(result1.All(c => c.IsSuccess));


            var result2 = await re.ExecuteAllRulesAsync("Test",new object[] { null });
            Assert.True(result2.All(c => c.IsSuccess));

            dynamic input1 = new ExpandoObject();
            input1.hello = new ExpandoObject();
            input1.hello.world = "wow";

            List<RuleResultTree> result3 = await re.ExecuteAllRulesAsync("Test", input1);
            Assert.True(result3.All(c => c.IsSuccess));

        }

        [Fact]
        public async Task ExecuteRule_SpecialCharInWorkflowName_RunsSuccessfully()
        {
            var workflow = new WorkflowRules {
                WorkflowName = "Exámple",
                Rules = new Rule[]{
                    new Rule {
                        RuleName = "RuleWithLocalParam",

                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "input1 == null || input1.hello.world = \"wow\""
                    }
                }
            };

            var workflowStr = "{\"WorkflowName\":\"Exámple\",\"WorkflowRulesToInject\":null,\"GlobalParams\":null,\"Rules\":[{\"RuleName\":\"RuleWithLocalParam\",\"Properties\":null,\"Operator\":null,\"ErrorMessage\":null,\"Enabled\":true,\"ErrorType\":\"Warning\",\"RuleExpressionType\":\"LambdaExpression\",\"WorkflowRulesToInject\":null,\"Rules\":null,\"LocalParams\":null,\"Expression\":\"input1 == null || input1.hello.world = \\\"wow\\\"\",\"Actions\":null,\"SuccessEvent\":null}]}";

            var re = new RulesEngine(new string[] { workflowStr },null,null);
           // re.AddWorkflow(workflowStr);

            dynamic input1 = new ExpandoObject();
            input1.hello = new ExpandoObject();
            input1.hello.world = "wow";

            List<RuleResultTree> result3 = await re.ExecuteAllRulesAsync("Exámple", input1);
            Assert.True(result3.All(c => c.IsSuccess));

        }



        private RulesEngine CreateRulesEngine(WorkflowRules workflow)
        {
            var json = JsonConvert.SerializeObject(workflow);
            return new RulesEngine(new string[] { json }, null);
        }

        private RulesEngine GetRulesEngine(string filename, ReSettings reSettings = null)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory() as string, "TestData", filename);
            var data = File.ReadAllText(filePath);

            var injectWorkflow = new WorkflowRules {
                WorkflowName = "inputWorkflowReference",
                WorkflowRulesToInject = new List<string> { "inputWorkflow" }
            };

            var injectWorkflowStr = JsonConvert.SerializeObject(injectWorkflow);
            var mockLogger = new Mock<ILogger>();
            return new RulesEngine(new string[] { data, injectWorkflowStr }, mockLogger.Object, reSettings);
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

            dynamic input1 = JsonConvert.DeserializeObject<List<RuleTestClass>>(laborCategoriesInput);
            dynamic input2 = JsonConvert.DeserializeObject<ExpandoObject>(currentLaborCategoryInput);
            dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo);
            dynamic input4 = JsonConvert.DeserializeObject<ExpandoObject>(basicInfo);
            dynamic input5 = JsonConvert.DeserializeObject<ExpandoObject>(orderInfo);

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

        [ExcludeFromCodeCoverage]
        private class TestInstanceUtils
        {
            public bool CheckExists(string str)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    return true;
                }

                return false;
            }

        }

    }
}