// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class FormatErrorMessagesNestedAndCyclesTest
    {
        public class OrderNode
        {
            public string Code { get; set; }
            public int Amount { get; set; }
            public CustomerNode Customer { get; set; }
        }

        public class CustomerNode
        {
            public string Name { get; set; }
            public List<OrderNode> Orders { get; set; } = new List<OrderNode>();
        }

        [Fact]
        public async Task FormatErrorMessages_NestedChildRuleFailure_FormatsChildExceptionMessage()
        {
            var workflow = new Workflow
            {
                WorkflowName = "NestedErrorFormatWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "ParentRule",
                        Operator = "And",
                        ErrorMessage = "Parent failed for $(input1.Code)",
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                RuleName = "PassingChild",
                                Expression = "input1.Amount > 0",
                                ErrorMessage = "Amount $(input1.Amount) must be positive"
                            },
                            new Rule
                            {
                                RuleName = "FailingChild",
                                Expression = "input1.Amount <= 100",
                                ErrorMessage = "Amount $(input1.Amount) exceeds limit 100"
                            }
                        }
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("NestedErrorFormatWorkflow",
                new RuleParameter("input1", new OrderNode { Code = "ORD-1", Amount = 500 }));

            Assert.Single(results);
            Assert.False(results[0].IsSuccess);
            Assert.Equal("Parent failed for ORD-1", results[0].ExceptionMessage);

            var failingChild = results[0].ChildResults.Single(c => c.Rule.RuleName == "FailingChild");
            Assert.False(failingChild.IsSuccess);
            Assert.Equal("Amount 500 exceeds limit 100", failingChild.ExceptionMessage);
        }

        [Fact]
        public async Task FormatErrorMessages_InputWithCircularReferences_DoesNotThrowJsonException()
        {
            var customer = new CustomerNode { Name = "Acme" };
            var order = new OrderNode { Code = "ORD-99", Amount = 500, Customer = customer };
            customer.Orders.Add(order); // Circular reference: Order.Customer.Orders[0] == Order

            var workflow = new Workflow
            {
                WorkflowName = "CyclicErrorFormatWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "LimitRule",
                        Expression = "input1.Amount <= 100",
                        ErrorMessage = "Order $(input1.Code) for $(input1.Customer.Name) failed with amount $(input1.Amount)"
                    }
                }
            };

            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync("CyclicErrorFormatWorkflow",
                new RuleParameter("input1", order));

            Assert.Single(results);
            Assert.False(results[0].IsSuccess);
            Assert.Equal("Order ORD-99 for Acme failed with amount 500", results[0].ExceptionMessage);
        }
    }
}
