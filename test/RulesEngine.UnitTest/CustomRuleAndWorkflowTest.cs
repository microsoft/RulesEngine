using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest;

/// <summary>
///     Test with custom rules and workflows classes
/// </summary>
public class CustomRuleAndWorkflowTest
{
    [Fact]
    public async Task RulesEngine_WithCustomRulesAndWorkflows_RunsSuccessfully()
    {
        var customRule = new CustomRule {
            RuleName = "CustomRule",
            Operator = "And",
            Enabled = true,
            ThisIsAmazingRule = new[] {
                new CustomRule {
                    RuleName = "CustomRule1",
                    Enabled = true,
                    RuleExpressionType = RuleExpressionType.LambdaExpression,
                    Expression = "input1.x > 10"
                },
                new CustomRule {
                    RuleName = "CustomRule2",
                    Enabled = true,
                    RuleExpressionType = RuleExpressionType.LambdaExpression,
                    Expression = "input1.x > 10"
                }
            }
        };

        var customWorkflow = new CustomWorkflow {
            WorkflowName = "CustomWorkflow",
            RuleExpressionType = RuleExpressionType.LambdaExpression,
            Rules = new[] { customRule }
        };


        var re = new RulesEngine([customWorkflow]);
        var input1 = GetInput1();
        List<RuleResultTree> result = await re.ExecuteAllRulesAsync("CustomWorkflow", input1);
        Assert.NotNull(result);
        Assert.IsType<List<RuleResultTree>>(result);
        Assert.Contains(result, c => c.IsSuccess);
    }

    [Fact]
    public async Task RulesEngine_WithCustomRulesAndWorkflowsAsJsonArray_RunsSuccessfully()
    {
        var workflowJson = """
                           {
                               "WorkflowRulesToInject": null,
                               "Rules": [
                                   {
                                       "ThisIsAmazingRule": [
                                           {
                                               "RuleName": "CustomRule1",
                                               "RuleExpressionType": 0,
                                               "Expression": "input1.x > 10",
                                           },
                                           {
                                               "RuleName": "CustomRule2",
                                               "RuleExpressionType": 0,
                                               "Expression": "input1.x > 10",
                                           }
                                       ],
                                       "RuleName": "CustomRule",
                                       "Operator": "And",
                                       "RuleExpressionType": 0,
                           
                                   }
                               ],
                               "WorkflowName": "CustomWorkflow",
                               "RuleExpressionType": 0,
                               "GlobalParams": null
                           }
                           """;

        var re = new RulesEngine([workflowJson], typeof(CustomWorkflow));
        var input1 = GetInput1();
        List<RuleResultTree> result = await re.ExecuteAllRulesAsync("CustomWorkflow", input1);
        Assert.NotNull(result);
        Assert.IsType<List<RuleResultTree>>(result);
        Assert.Contains(result, c => c.IsSuccess);
    }


    private dynamic GetInput1()
    {
        var converter = new ExpandoObjectConverter();
        const string basicInfo =
            "{\"x\": 50}";
        return JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
    }

    public class CustomRule : IRule
    {
        public IEnumerable<CustomRule> ThisIsAmazingRule { get; set; }
        public string RuleName { get; set; }

        public Dictionary<string, object> Properties { get; set; }
        public string Operator { get; set; }
        public string ErrorMessage { get; set; }
        public bool Enabled { get; set; } = true;
        public RuleExpressionType RuleExpressionType { get; set; }
        public IEnumerable<string> WorkflowsToInject { get; set; }

        public IEnumerable<IRule> GetNestedRules()
        {
            return ThisIsAmazingRule;
        }

        public void SetRules(IEnumerable<IRule> rules)
        {
            ThisIsAmazingRule = rules.OfType<CustomRule>().ToArray();
        }

        public IEnumerable<ScopedParam> LocalParams { get; set; }
        public string Expression { get; set; }
        public RuleActions Actions { get; set; }
        public string SuccessEvent { get; set; }
    }

    public class CustomWorkflow : IWorkflow
    {
        public IEnumerable<string> WorkflowRulesToInject { get; set; }
        public IEnumerable<CustomRule> Rules { get; set; }
        public string WorkflowName { get; set; }
        public IEnumerable<string> WorkflowsToInject { get; set; }
        public RuleExpressionType RuleExpressionType { get; set; }
        public IEnumerable<ScopedParam> GlobalParams { get; set; }

        public IEnumerable<IRule> GetRules()
        {
            return Rules;
        }

        public void SetRules(IEnumerable<IRule> rules)
        {
            Rules = rules.OfType<CustomRule>().ToArray();
        }
    }
}
