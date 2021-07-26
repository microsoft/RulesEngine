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
    public class RuleExpressionBuilderFactoryTest
    {
        [Theory]
        [InlineData(RuleExpressionType.LambdaExpression, typeof(LambdaExpressionBuilder))]
        public void RuleGetExpressionBuilderTest(RuleExpressionType expressionType, Type expectedExpressionBuilderType)
        {
            var reSettings = new ReSettings();
            var parser = new RuleExpressionParser(reSettings);
            var objBuilderFactory = new RuleExpressionBuilderFactory(reSettings, parser);
            var builder = objBuilderFactory.RuleGetExpressionBuilder(expressionType);

            var builderType = builder.GetType();
            Assert.Equal(expectedExpressionBuilderType.ToString(), builderType.ToString());
        }
    }
}
