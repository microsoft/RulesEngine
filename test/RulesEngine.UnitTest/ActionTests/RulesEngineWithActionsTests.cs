// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{

    [ExcludeFromCodeCoverage]
    public class RulesEngineWithActionsTests
    {

        [Fact]
        public async Task WhenExpressionIsSuccess_OutputExpressionAction_ReturnsExpressionEvaluation()
        {
            var engine = new RulesEngine(GetWorkflowWithActions());
            var result = await engine.ExecuteActionWorkflowAsync("ActionWorkflow", "ExpressionOutputRuleTest", new RuleParameter[0]);
            Assert.NotNull(result);
            Assert.Equal(2 * 2, result.Output);
        }

        [Fact]
        public async Task WhenExpressionIsSuccess_ComplexOutputExpressionAction_ReturnsExpressionEvaluation()
        {
            var engine = new RulesEngine(GetWorkflowWithActions());
            var result = await engine.ExecuteActionWorkflowAsync("ActionWorkflow", "ComplexOutputRuleTest", new RuleParameter[0]);
            Assert.NotNull(result);
            dynamic output = result.Output;
            Assert.Equal(2, output.test);
        }

        [Fact]
        public async Task WhenExpressionIsSuccess_EvaluateRuleAction_ReturnsExpressionEvaluation()
        {
            var engine = new RulesEngine(GetWorkflowWithActions());
            var result = await engine.ExecuteActionWorkflowAsync("ActionWorkflow", "EvaluateRuleTest", new RuleParameter[0]);
            Assert.NotNull(result);
            Assert.Equal(2 * 2, result.Output);
            Assert.Contains(result.Results, c => c.Rule.RuleName == "ExpressionOutputRuleTest");
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_CalledWithIncorrectWorkflowOrRuleName_ThrowsArgumentException()
        {
            var engine = new RulesEngine(GetWorkflowWithActions());
            await Assert.ThrowsAsync<ArgumentException>(async () => await engine.ExecuteActionWorkflowAsync("WrongWorkflow", "ExpressionOutputRuleTest", new RuleParameter[0]));
            await Assert.ThrowsAsync<ArgumentException>(async () => await engine.ExecuteActionWorkflowAsync("ActionWorkflow", "WrongRule", new RuleParameter[0]));
        }


        [Fact]
        public async Task ExecuteActionWorkflowAsync_CalledWithNoActionsInWorkflow_ExecutesSuccessfully()
        {

            var engine = new RulesEngine(GetWorkflowsWithoutActions());
            var result = await engine.ExecuteActionWorkflowAsync("NoActionWorkflow", "NoActionTest", new RuleParameter[0]);
            Assert.NotNull(result);
            Assert.Null(result.Output);
        }


        [Fact]
        public async Task ExecuteActionWorkflowAsync_SelfReferencingAction_NoFilter_ExecutesSuccessfully()
        {

            var engine = new RulesEngine(GetWorkflowWithActions());
            var result = await engine.ExecuteActionWorkflowAsync("WorkflowWithGlobalsAndSelfRefActions", "RuleReferencingSameWorkflow", new RuleParameter[0]);
            Assert.NotNull(result);
            Assert.Null(result.Output);
        }

        [Fact]
        public async Task ExecuteActionWorkflowAsync_SelfReferencingAction_WithFilter_ExecutesSuccessfully()
        {

            var engine = new RulesEngine(GetWorkflowWithActions());
            var result = await engine.ExecuteActionWorkflowAsync("WorkflowWithGlobalsAndSelfRefActions", "RuleReferencingSameWorkflowWithInputFilter", new RuleParameter[0]);
            Assert.NotNull(result);
            Assert.Equal(4,result.Output);
        }

        private Workflow[] GetWorkflowsWithoutActions()
        {
            var workflow1 = new Workflow {
                WorkflowName = "NoActionWorkflow",
                Rules = new List<Rule>{
                    new Rule{
                        RuleName = "NoActionTest",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "1 == 1",
                    }

                }
            };
            return new[] { workflow1 };
        }

        private Workflow[] GetWorkflowWithActions()
        {

            var workflow1 = new Workflow {
                WorkflowName = "ActionWorkflow",
                Rules = new List<Rule>{
                    new Rule{
                        RuleName = "ExpressionOutputRuleTest",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "1 == 1",
                        Actions = new RuleActions{
                            OnSuccess = new ActionInfo{
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object>{
                                    {"expression", "2*2"}
                                }
                            }
                        }
                    },
                    new Rule{
                        RuleName = "ComplexOutputRuleTest",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "1 == 1",
                        Actions = new RuleActions{
                            OnSuccess = new ActionInfo{
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object>{
                                    {"expression", "new (2 as test)"}
                                }
                            }
                        }
                    },
                    new Rule{
                        RuleName = "EvaluateRuleTest",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "1 == 1",
                        Actions = new RuleActions{
                            OnSuccess = new ActionInfo{
                                Name = "EvaluateRule",
                                Context = new Dictionary<string, object>{
                                    {"workflowName", "ActionWorkflow"},
                                    {"ruleName","ExpressionOutputRuleTest"}
                                }
                            }
                        }
                    }

                }

              
            };

            var workflow2 = new Workflow {
                WorkflowName = "WorkflowWithGlobalsAndSelfRefActions",
                GlobalParams = new[] {
                    new ScopedParam {
                        Name = "global1",
                        Expression = "\"Hello\""
                    }
                },
                Rules = new[] {

                    new Rule{
                        RuleName = "RuleReferencingSameWorkflow",
                        Expression = "1 == 1",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo{
                                Name = "EvaluateRule",
                                Context = new Dictionary<string, object>{
                                    {"workflowName", "WorkflowWithGlobalsAndSelfRefActions"},
                                    {"ruleName","OtherRule"}
                                }
                            }
                        }
                    },new Rule{
                        RuleName = "RuleReferencingSameWorkflowWithInputFilter",
                        Expression = "1 == 1",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo{
                                Name = "EvaluateRule",
                                Context = new Dictionary<string, object>{
                                    {"workflowName", "WorkflowWithGlobalsAndSelfRefActions"},
                                    {"ruleName","OtherRule"},
                                    {"inputFilter",new string[] { } },
                                    {"additionalInputs", new [] { 
                                        new ScopedParam(){
                                            Name = "additionalValue",
                                            Expression = "1"
                                        }

                                    } }
                                }

                            }
                        }
                    }


                    , new Rule{
                        RuleName = "OtherRule",
                        Expression = "additionalValue == 1",
                        Actions = new RuleActions {
                             OnSuccess = new ActionInfo{
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object>{
                                    {"expression", "2*2"}
                                }
                            }
                        }

                    }

                }

            };
            return new[] { workflow1, workflow2 };
        }
    }
}