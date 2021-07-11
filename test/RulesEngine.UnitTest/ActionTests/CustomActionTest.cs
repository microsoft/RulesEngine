// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Enums;
using RulesEngine.Models;
using RulesEngine.UnitTest.ActionTests.MockClass;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest.ActionTests
{
    [ExcludeFromCodeCoverage]
    public class CustomActionTest
    {
        [Fact]
        public async Task CustomActionOnRuleMustHaveContextValues()
        {
            var workflows = GetWorkflowRules();
            var re = new RulesEngine(workflows, null, reSettings: new ReSettings {
                CustomActions = new Dictionary<string, System.Func<Actions.ActionBase>> {

                    { "ReturnContext", () => new ReturnContextAction() }
                }
            });

            var result = await re.ExecuteAllRulesAsync("successReturnContextAction", true);
        }

        private WorkflowRules[] GetWorkflowRules()
        {
            return new WorkflowRules[] {
                new WorkflowRules {
                    WorkflowName = "successReturnContextAction",
                    Rules = new Rule[] {
                        new Rule {
                            RuleName = "trueRule",
                            Expression = "input1 == true",
                            Actions = new Dictionary<ActionTriggerType, ActionInfo>() {
                                { ActionTriggerType.onSuccess, new ActionInfo {
                                    Name = "ReturnContext",
                                    Context =  new Dictionary<string, object> {
                                        {"stringContext", "hello"},
                                        {"intContext",1 },
                                        {"objectContext", new { a = "hello", b = 123 } }
                                    }
                                }

                            }

                        },


                    }
                }
            }
            };
        }


    }
}
