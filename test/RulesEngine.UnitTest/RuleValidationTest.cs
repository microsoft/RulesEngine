// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class RuleValidationTest
    {
        [Fact]
        public async Task NullExpressionithLambdaExpression_ReturnsExepectedResults()
        {
            var workflow = GetNullExpressionithLambdaExpressionWorkflow();
            var reSettings = new ReSettings { };
            RulesEngine rulesEngine = new RulesEngine();

            Func<Task> action = () => {
                new RulesEngine(workflow, reSettings: reSettings);
                return Task.CompletedTask;
            };

            Exception ex = await Assert.ThrowsAsync<Exceptions.RuleValidationException>(action);

            Assert.Contains(Constants.LAMBDA_EXPRESSION_EXPRESSION_NULL_ERRMSG, ex.Message);

        }

        [Fact]
        public async Task NestedRulesWithMissingOperator_ReturnsExepectedResults()
        {
            var workflow = GetEmptyOperatorWorkflow();
            var reSettings = new ReSettings { };
            RulesEngine rulesEngine = new RulesEngine();

            Func<Task> action = () => {
                new RulesEngine(workflow, reSettings: reSettings);
                return Task.CompletedTask;
            };

            Exception ex = await Assert.ThrowsAsync<Exceptions.RuleValidationException>(action);

            Assert.Contains(Constants.OPERATOR_RULES_ERRMSG, ex.Message);

        }

        private Workflow[] GetNullExpressionithLambdaExpressionWorkflow()
        {
            return new[] {
                new Workflow {
                    Name = "NestedRulesTest",
                    Rules = new Rule[] {
                        new Rule {
                            Name = "TestRule",
                            RuleExpressionType = RuleExpressionType.LambdaExpression,
                        }
                    }
                }
            };
        }

        private Workflow[] GetEmptyOperatorWorkflow()
        {
            return new[] {
                new Workflow {
                    Name = "NestedRulesTest",
                    Rules = new Rule[] {
                        new Rule {
                            Name = "AndRuleTrueFalse",
                            Expression = "true == true",
                            Rules = new Rule[] {
                                new Rule{
                                    Name = "trueRule1",
                                    Expression = "input1.TrueValue == true",
                                },
                                new Rule {
                                    Name = "falseRule1",
                                    Expression = "input1.TrueValue == false"
                                }

                            }
                        }
                    }
                }
            };
        }
    }
}
