// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Actions;
using RulesEngine.Models;
using RulesEngine.UnitTest.ActionTests.MockClass;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RulesEngine.UnitTest.ActionTests;

[ExcludeFromCodeCoverage]
public class CustomActionTest
{
    [Fact]
    public async Task CustomActionOnRuleMustHaveContextValues()
    {
        var workflow = GetWorkflow();
        var re = new RulesEngine(workflow,
            new ReSettings {
                CustomActions = new Dictionary<string, Func<ActionBase>> {
                    { "ReturnContext", () => new ReturnContextAction() }
                }
            });

        var result = await re.ExecuteAllRulesAsync("successReturnContextAction", true);
    }


    [Fact]
    public async Task CustomAction_WithSystemTextJsobOnRuleMustHaveContextValues()
    {
        var workflow = GetWorkflow();
        var workflowStr = JsonConvert.SerializeObject(workflow);
        var serializationOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var workflowViaTextJson = JsonSerializer.Deserialize<Workflow[]>(workflowStr, serializationOptions);


        var re = new RulesEngine(workflow,
            new ReSettings {
                CustomActions = new Dictionary<string, Func<ActionBase>> {
                    { "ReturnContext", () => new ReturnContextAction() }
                }
            });


        var result = await re.ExecuteAllRulesAsync("successReturnContextAction", true);
    }

    private Workflow[] GetWorkflow()
    {
        return new Workflow[] {
            new() {
                WorkflowName = "successReturnContextAction",
                Rules = new Rule[] {
                    new() {
                        RuleName = "trueRule",
                        Expression = "input1 == true",
                        Actions = new RuleActions {
                            OnSuccess = new ActionInfo {
                                Name = "ReturnContext",
                                Context = new Dictionary<string, object> {
                                    { "stringContext", "hello" },
                                    { "intContext", 1 },
                                    { "objectContext", new { a = "hello", b = 123 } }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}