// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class TypedClassTests
    {
        public class Transazione
        {
            public static string StaticProperty { get; set; } = "Hello";
            public List<Attore> Attori { get; set; } = new();
        }
        public class Attore
        {
            public Guid Id { get; internal set; }
            public string Nome { get; internal set; }
            public RuoloAttore RuoloAttore { get; internal set; }
        }

        public enum RuoloAttore
        {
            A,
            B,
            C
        }

        [Fact]
        public async Task TypedClassTest()
        {
            Workflow workflow = new() {
                WorkflowName = "Conferimento",
                Rules = new Rule[] {
                    new() {
                        RuleName = "Attore Da",
                        Enabled = true,
                        ErrorMessage = "Attore Da Id must be defined",
                        SuccessEvent = "10",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "transazione.Attori.Any(a => a.RuoloAttore == 1)",
                    },
                    new() {
                        RuleName = "Attore A",
                        Enabled = true,
                        ErrorMessage = "Attore A must be defined",
                        SuccessEvent = "10",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "transazione.Attori != null",
                    },
                }
            };
            var reSettings = new ReSettings() {
                CustomTypes = new Type[] {
                },
                AutoRegisterInputType = false
            };
            var re = new RulesEngine(reSettings);
            re.AddWorkflow(workflow);

            var param = new Transazione {
                    Attori = new List<Attore>{
                    new Attore{
                        RuoloAttore = RuoloAttore.B,
                   
                    },
                    new Attore {
                         RuoloAttore = RuoloAttore.C
                    }
                }

            };

            var result = await  re.ExecuteAllRulesAsync("Conferimento", new RuleParameter("transazione", param));

            Assert.All(result, (res) => Assert.True(res.IsSuccess));

        }


        [Fact]
        public async Task TypedClassInputSameNameAsTypeTest()
        {
            Workflow workflow = new() {
                WorkflowName = "Conferimento",
                Rules = new Rule[] {
                    new() {
                        RuleName = "Attore Da",
                        Enabled = true,
                        ErrorMessage = "Attore Da Id must be defined",
                        SuccessEvent = "10",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "transazione.Attori.Any(a => a.RuoloAttore == 1)",
                    },
                    new() {
                        RuleName = "Attore A",
                        Enabled = true,
                        ErrorMessage = "Attore A must be defined",
                        SuccessEvent = "10",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "transazione.Attori != null",
                    }
                   
                }
            };
            var reSettings = new ReSettings() {
                CustomTypes = new Type[] {
                    typeof(Transazione)
                }
            };
            var re = new RulesEngine(reSettings);
            re.AddWorkflow(workflow);

            var param = new Transazione {
                Attori = new List<Attore>{
                    new Attore{
                        RuoloAttore = RuoloAttore.B,

                    },
                    new Attore {
                         RuoloAttore = RuoloAttore.C
                    }
                }

            };

            var result = await re.ExecuteAllRulesAsync("Conferimento", new RuleParameter("Transazione", param));

            Assert.All(result, (res) => Assert.True(res.IsSuccess));

        }


        [Fact]
        public async Task TypedClassBothAccessibleTestWhenCaseInsensitive()
        {
            Workflow workflow = new() {
                WorkflowName = "Conferimento",
                Rules = new Rule[] {
                    new() {
                        RuleName = "Attore Da",
                        Enabled = true,
                        ErrorMessage = "Attore Da Id must be defined",
                        SuccessEvent = "10",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "transazione.Attori.Any(a => a.RuoloAttore == 1)",
                    },
                    new() {
                        RuleName = "Attore A",
                        Enabled = true,
                        ErrorMessage = "Attore A must be defined",
                        SuccessEvent = "10",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "transazione.Attori != null",
                    },
                     new() {
                        RuleName = "Static FieldTest",
                        Expression = "Transazione.StaticProperty == \"Hello\""
                    }
                }
            };
            var reSettings = new ReSettings() {
                CustomTypes = new Type[] {
                    typeof(Transazione)
                },
                IsExpressionCaseSensitive = true
            };
            var re = new RulesEngine(reSettings);
            re.AddWorkflow(workflow);

            var param = new Transazione {
                Attori = new List<Attore>{
                    new Attore{
                        RuoloAttore = RuoloAttore.B,

                    },
                    new Attore {
                         RuoloAttore = RuoloAttore.C
                    }
                }

            };

            var result = await re.ExecuteAllRulesAsync("Conferimento", new RuleParameter("transazione", param));

            Assert.All(result, (res) => Assert.True(res.IsSuccess));

        }
    }
}
