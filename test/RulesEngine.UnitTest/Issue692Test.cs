// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue692Test
    {
        public class WithNullableDate
        {
            public DateTime? Dt { get; set; }
        }

        // The reporter says: when comparing a null DateTime against a set DateTime, BOTH
        // `<` and `>` return false (whereas in 5.0.3 and earlier null was treated as
        // "less than" any set datetime).
        //
        // Behavior of standard .NET nullable DateTime semantics:
        //   null < someDateTime  → false (Nullable<T> comparisons return false when either operand is null)
        //   null > someDateTime  → false
        // This is the SAME as standard C# semantics — Nullable<T> comparisons are tri-valued
        // and false-when-null is the documented behavior. There is no "null is less than" rule
        // in .NET; the reporter's previous behavior was either via Newtonsoft.Json string-typing
        // (null treated as empty / default DateTime) or via a Dynamic.Core quirk.
        //
        // These tests document the current behavior so we don't accidentally regress.
        [Fact]
        public async Task NullableDateTime_LessThan_NullReturnsFalse()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] { new Rule { RuleName = "R", Expression = "input1.Dt < DateTime.Now" } }
            };
            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { CustomTypes = new[] { typeof(DateTime) } });
            var results = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", new WithNullableDate { Dt = null }) });

            Assert.False(results[0].IsSuccess);
        }

        [Fact]
        public async Task NullableDateTime_GreaterThan_NullReturnsFalse()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] { new Rule { RuleName = "R", Expression = "input1.Dt > DateTime.Now" } }
            };
            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { CustomTypes = new[] { typeof(DateTime) } });
            var results = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", new WithNullableDate { Dt = null }) });

            Assert.False(results[0].IsSuccess);
        }

        // The canonical workaround for users who DO want null-aware semantics: check HasValue
        // explicitly. This is also the standard C# pattern.
        [Fact]
        public async Task NullableDateTime_ExplicitHasValueCheck_WorksAsExpected()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule { RuleName = "TreatNullAsLess",
                        Expression = "!input1.Dt.HasValue || input1.Dt < DateTime.Now" }
                }
            };
            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { CustomTypes = new[] { typeof(DateTime) } });

            var withNull = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", new WithNullableDate { Dt = null }) });
            Assert.True(withNull[0].IsSuccess);

            var withValue = await engine.ExecuteAllRulesAsync(
                "wf", new[] { RuleParameter.Create("input1", new WithNullableDate { Dt = DateTime.Now.AddDays(-1) }) });
            Assert.True(withValue[0].IsSuccess);
        }
    }
}
