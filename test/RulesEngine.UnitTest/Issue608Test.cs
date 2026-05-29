// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue608Support
    {
        public class AppData
        {
            public List<Detail> Details { get; set; } = new List<Detail>();
        }
        public class Detail
        {
            public decimal? Amount { get; set; }
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue608Test
    {
        private static Workflow BuildWorkflow() => new Workflow
        {
            WorkflowName = "wf",
            Rules = new[] {
                new Rule {
                    RuleName = "ChainedSum",
                    Expression = "Total > 0",
                    LocalParams = new List<ScopedParam>
                    {
                        new ScopedParam { Name = "Field1", Expression = "AppData.Details.Sum(l => l.Amount)" },
                        new ScopedParam { Name = "Field2", Expression = "AppData.Details.Sum(l => l.Amount)" },
                        new ScopedParam { Name = "Field3", Expression = "AppData.Details.Sum(l => l.Amount)" },
                        new ScopedParam { Name = "Total",  Expression = "Field1 + Field2 + Field3" }
                    }
                }
            }
        };

        private static Issue608Support.AppData BuildInput() => new Issue608Support.AppData
        {
            Details = new List<Issue608Support.Detail>
            {
                new Issue608Support.Detail { Amount = 1 },
                new Issue608Support.Detail { Amount = 2 },
                new Issue608Support.Detail { Amount = 3 },
            }
        };

        [Fact]
        public async Task ChainedScopedParamSum_WithFastCompiler_Works()
        {
            var engine = new RulesEngine(new[] { BuildWorkflow() },
                new ReSettings { UseFastExpressionCompiler = true });

            var rp = new RuleParameter("AppData", BuildInput());
            var results = await engine.ExecuteAllRulesAsync("wf", new[] { rp });

            Assert.True(results[0].IsSuccess,
                $"Expected success. Got: {results[0].ExceptionMessage}");
        }

        [Fact]
        public async Task ChainedScopedParamSum_WithoutFastCompiler_StillWorks()
        {
            var engine = new RulesEngine(new[] { BuildWorkflow() },
                new ReSettings { UseFastExpressionCompiler = false });

            var rp = new RuleParameter("AppData", BuildInput());
            var results = await engine.ExecuteAllRulesAsync("wf", new[] { rp });

            Assert.True(results[0].IsSuccess);
        }
    }
}
