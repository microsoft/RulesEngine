using RulesEngine.Extensions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Demo;

public class CustomClasses
{
    /// <summary>
    ///     Some customizable rule class
    /// </summary>
    public class CustomRule : IRule
    {
        private IEnumerable<CustomRule> _thisIsAmazingRule;

        public IEnumerable<CustomRule> ThisIsAmazingRule {
            get => _thisIsAmazingRule;
            set {
                if (value != null) {
                    _thisIsAmazingRule = value.ToArray();
                }

                //  Whatever you want to do with the rules
                throw new ArithmeticException();
            }
        }

        public string RuleName { get; set; }

        public Dictionary<string, object> Properties { get; set; }
        public string Operator { get; set; }
        public string ErrorMessage { get; set; }

        // Note: null is false by default and thus could be disabling the rule by accident
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

    /// <summary>
    ///     Customizable Workflow class
    /// </summary>
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

    public async Task Run()
    {
        Console.WriteLine($"Running {nameof(Basic)}....");
        var workflows = new List<IWorkflow>();
        var workflow = new CustomWorkflow { WorkflowName = "Test Workflow Rule 1" };

        var rules = new List<CustomRule>();

        var rule = new CustomRule {
            RuleName = "Test Rule",
            SuccessEvent = "Count is within tolerance.",
            ErrorMessage = "Over expected.",
            Expression = "count < 3",
            RuleExpressionType = RuleExpressionType.LambdaExpression
        };

        rules.Add(rule);

        workflow.Rules = rules;

        workflows.Add(workflow);

        var bre = new RulesEngine.RulesEngine(workflows.ToArray());

        dynamic datas = new ExpandoObject();
        datas.count = 1;
        var inputs = new[] { datas };

        var resultList = await bre.ExecuteAllRulesAsync("Test Workflow Rule 1", inputs);

        bool outcome;

        //Different ways to show test results:
        outcome = resultList.TrueForAll(r => r.IsSuccess);

        resultList.OnSuccess(eventName => {
            Console.WriteLine($"Result '{eventName}' is as expected.");
            outcome = true;
        });

        resultList.OnFail(() => {
            outcome = false;
        });

        Console.WriteLine($"Test outcome: {outcome}.");
    }
}
