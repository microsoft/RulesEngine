// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using FluentValidation;

namespace RulesEngine.UnitTest;

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

    [Fact]
    public void SystemLinqDynamicCore_WithLiterals_ParsingCorretly()
    {
        var board = new { NumberOfMembers = default(decimal?) };

        var parameter = RuleParameter.Create("Board", board);

        var parser = new RuleExpressionParser();

        try
        {
            const string expression1 = "Board.NumberOfMembers = 0.2d";
            var result1 = parser.Evaluate<bool>(expression1, [parameter]);
            Assert.False(result1);
        }
        catch (Exception)
        {
            // passing it over.
        }

        // This will throw an exception even if the expression is valid,
        // because the first one executed and cached in the Literals Dictionary.
        // This should not throw as it's valid.
        const string expression2 = "Board.NumberOfMembers = 0.2";
        var result2 = parser.Evaluate<bool>(expression2, [parameter]);
        Assert.False(result2);
    }
}
