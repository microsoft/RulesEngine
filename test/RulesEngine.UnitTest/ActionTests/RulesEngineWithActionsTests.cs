using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RulesEngine.Enums;
using RulesEngine.Models;
using Xunit;

namespace RulesEngine.UnitTest{

    [ExcludeFromCodeCoverage]
    public class RulesEngineWithActionsTests{

        [Fact]
        public async Task WhenExpressionIsSuccess_OutputExpressionAction_ReturnsExpressionEvaluation(){
            var engine = new RulesEngine(GetWorkflowWithActions());
            var result = await engine.ExecuteActionWorkflowAsync("ActionWorkflow", "ExpressionOutputRuleTest", new RuleParameter[0]);
            Assert.NotNull(result);
            Assert.Equal(2*2,result.Output);
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

        private WorkflowRules[] GetWorkflowWithActions(){
            
            var workflow1 = new WorkflowRules{
                WorkflowName = "ActionWorkflow",
                Rules = new List<Rule>{
                    new Rule{
                        RuleName = "ExpressionOutputRuleTest",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "1 == 1",
                        Actions = new Dictionary<ActionTriggerType, ActionInfo>{
                            { ActionTriggerType.onSuccess, new ActionInfo{
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object>{
                                    {"expression", "2*2"}
                                }
                            }}
                        }
                    },
                    new Rule{
                        RuleName = "EvaluateRuleTest",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "1 == 1",
                        Actions = new Dictionary<ActionTriggerType, ActionInfo>{
                            { ActionTriggerType.onSuccess, new ActionInfo{
                                Name = "EvaluateRule",
                                Context = new Dictionary<string, object>{
                                    {"workflowName", "ActionWorkflow"},
                                    {"ruleName","ExpressionOutputRuleTest"}
                                }
                            }}
                        }
                    }

                }
            };
            return new []{workflow1};
        }
    }
}