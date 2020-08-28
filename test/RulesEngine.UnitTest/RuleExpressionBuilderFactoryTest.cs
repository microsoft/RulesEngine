// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    public class RuleExpressionBuilderFactoryTest
    {
        [Theory]
        [InlineData(RuleExpressionType.LambdaExpression, typeof(LambdaExpressionBuilder))]
        public void RuleGetExpressionBuilderTest(RuleExpressionType expressionType, Type expectedExpressionBuilderType)
        {
            var objBuilderFactory = new RuleExpressionBuilderFactory(new ReSettings());
            var builder = objBuilderFactory.RuleGetExpressionBuilder(expressionType);

            var builderType = builder.GetType();
            Assert.Equal(expectedExpressionBuilderType.ToString(), builderType.ToString());
        }
    }
}
