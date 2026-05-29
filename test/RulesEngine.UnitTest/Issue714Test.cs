// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public static class Issue714Counter
    {
        private static int _count;
        public static int CallCount => _count;
        public static void Reset() => Interlocked.Exchange(ref _count, 0);
        public static string FromDb(string s)
        {
            Interlocked.Increment(ref _count);
            return s;
        }
    }

    [ExcludeFromCodeCoverage]
    public class Issue714Test
    {
        [Fact]
        public async Task GlobalParam_WithMultipleRules_IsEvaluatedOnce()
        {
            Issue714Counter.Reset();

            var workflow = new Workflow
            {
                WorkflowName = "wf",
                GlobalParams = new[] {
                    new ScopedParam { Name = "myglobal1", Expression = "Issue714Counter.FromDb(input1)" }
                },
                Rules = new[] {
                    new Rule { RuleName = "r1", Expression = "myglobal1 == \"hello\"" },
                    new Rule { RuleName = "r2", Expression = "input1.ToLower() == myglobal1" },
                    new Rule { RuleName = "r3", Expression = "myglobal1.Length == 5" }
                }
            };

            var engine = new RulesEngine(new[] { workflow },
                new ReSettings { CustomTypes = new[] { typeof(Issue714Counter) } });

            await engine.ExecuteAllRulesAsync("wf", "hello");

            Assert.Equal(1, Issue714Counter.CallCount);
        }
    }
}
