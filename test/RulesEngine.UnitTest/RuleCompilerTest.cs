// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class RuleCompilerTest
    {
        [Fact]
        public void RuleCompiler_NullCheck()
        {
            Assert.Throws<ArgumentNullException>(() => new RuleCompiler(null, null));
            var reSettings = new ReSettings();
            var parser = new RuleExpressionParser(reSettings);
            Assert.Throws<ArgumentNullException>(() => new RuleCompiler(null, null));
        }

        [Fact]
        public void RuleCompiler_CompileRule_ThrowsException()
        {
            var reSettings = new ReSettings();
            var parser = new RuleExpressionParser(reSettings);
            var compiler = new RuleCompiler(new RuleExpressionBuilderFactory(reSettings, parser),null);
            Assert.Throws<ArgumentNullException>(() => compiler.CompileRule(null, null,null));
            Assert.Throws<ArgumentNullException>(() => compiler.CompileRule(null, new RuleParameter[] { null },null));
        }
    }
}
