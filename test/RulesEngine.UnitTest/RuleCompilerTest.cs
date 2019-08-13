// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category","Unit")]
    public class RuleCompilerTest
    {
        [Fact]
        public void RuleCompiler_NullCheck()
        {
            Assert.Throws<ArgumentNullException>(() => new RuleCompiler(null, null));
            Assert.Throws<ArgumentNullException>(() => new RuleCompiler(new RuleExpressionBuilderFactory(new ReSettings()), null));
        }

        [Fact]
        public void RuleCompiler_CompileRule_ThrowsException()
        {
            var compiler = new RuleCompiler(new RuleExpressionBuilderFactory(new ReSettings()), new NullLogger());
            Assert.Throws<ArgumentException>(() => compiler.CompileRule(null, null));
            Assert.Throws<ArgumentException>(() => compiler.CompileRule(null, new RuleParameter[] { null}));
        }


    }
}
