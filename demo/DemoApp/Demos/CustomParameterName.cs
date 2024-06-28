using RulesEngine.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class CustomParameterName
    {
        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(Basic)}....");

            var workflows = new Workflow[] {
                new Workflow {
                    WorkflowName = "my_workflow",
                    Rules = [
                        new Rule {
                            RuleName = "MatchesFabrikam",
                            SuccessMessage = "does match",
                            ErrorMessage = "does not match",
                            Expression = "myValue.Value1 == \"Fabrikam\""
                        }
                    ]
                }
            };

            var bre = new RulesEngine.RulesEngine(workflows);

            var rp = new RuleParameter[] {
                new RuleParameter("myValue", new { Value1 = "Fabrikam" })
            };

            var ret = await bre.ExecuteAllRulesAsync("my_workflow", rp);

            var outcome = false;

            //Different ways to show test results:
            outcome = ret.TrueForAll(r => r.IsSuccess);

            ret.OnSuccess((eventName) =>
            {
                Console.WriteLine($"Result '{eventName}' is as expected.");
                outcome = true;
            });

            ret.OnFail(() =>
            {
                outcome = false;
            });

            Console.WriteLine($"Test outcome: {outcome}.");
        }
    }
}
