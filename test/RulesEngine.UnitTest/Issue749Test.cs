// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [DynamicLinqType]
    [ExcludeFromCodeCoverage]
    public static class Issue749ScannedType
    {
        public static bool IsSpecial(int value) => value == 42;
    }

    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class Issue749Test
    {
        [Fact]
        public void CustomTypeProvider_ScanningEnabled_IncludesDynamicLinqTypeMarkedType()
        {
            var provider = new CustomTypeProvider(Array.Empty<Type>(), enableAssemblyScanning: true);

            var allTypes = provider.GetCustomTypes();

            Assert.Contains(typeof(Issue749ScannedType), allTypes);
        }

        [Fact]
        public void CustomTypeProvider_ScanningDisabled_ExcludesDynamicLinqTypeMarkedType()
        {
            var provider = new CustomTypeProvider(Array.Empty<Type>(), enableAssemblyScanning: false);

            var allTypes = provider.GetCustomTypes();

            Assert.DoesNotContain(typeof(Issue749ScannedType), allTypes);
        }

        [Fact]
        public void CustomTypeProvider_ScanningDisabled_StillIncludesExplicitAndInputTypes()
        {
            var provider = new CustomTypeProvider(new[] { typeof(List<string>) }, enableAssemblyScanning: false);

            var allTypes = provider.GetCustomTypes();

            Assert.Contains(typeof(List<string>), allTypes);
            Assert.Contains(typeof(System.Linq.Enumerable), allTypes);
            Assert.Contains(typeof(System.Collections.IEnumerable), allTypes);
        }

        [Fact]
        public async Task ScanningDisabled_BasicRuleAndInputMethods_StillWork()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "input1.Value.ToString() == \"42\""
                    }
                }
            };

            var reSettings = new ReSettings { EnableAssemblyScanning = false };
            var engine = new RulesEngine(new[] { workflow }, reSettings);

            var result = await engine.ExecuteAllRulesAsync("wf",
                new RuleParameter("input1", new { Value = 42 }));

            Assert.True(result[0].IsSuccess);
        }

        [Fact]
        public async Task ScanningEnabled_DynamicLinqTypeStaticMethod_Resolves()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "Issue749ScannedType.IsSpecial(input1.Value)"
                    }
                }
            };

            var reSettings = new ReSettings { EnableAssemblyScanning = true };
            var engine = new RulesEngine(new[] { workflow }, reSettings);

            var result = await engine.ExecuteAllRulesAsync("wf",
                new RuleParameter("input1", new { Value = 42 }));

            Assert.True(result[0].IsSuccess);
        }

        [Fact]
        public async Task ScanningDisabled_TypeRegisteredExplicitly_StaticMethodResolves()
        {
            var workflow = new Workflow
            {
                WorkflowName = "wf",
                Rules = new[] {
                    new Rule {
                        RuleName = "R",
                        Expression = "Issue749ScannedType.IsSpecial(input1.Value)"
                    }
                }
            };

            var reSettings = new ReSettings {
                EnableAssemblyScanning = false,
                CustomTypes = new[] { typeof(Issue749ScannedType) }
            };
            var engine = new RulesEngine(new[] { workflow }, reSettings);

            var result = await engine.ExecuteAllRulesAsync("wf",
                new RuleParameter("input1", new { Value = 42 }));

            Assert.True(result[0].IsSuccess);
        }
    }
}
