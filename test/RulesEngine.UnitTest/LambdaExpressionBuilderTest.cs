// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    public class LambdaExpressionBuilderTest
    {
        [Fact]
        public void BuildExpressionForRuleTest()
        {
            var objBuilderFactory = new RuleExpressionBuilderFactory(new ReSettings());
            var builder = objBuilderFactory.RuleGetExpressionBuilder(RuleExpressionType.LambdaExpression);

            var parameterExpressions = new List<ParameterExpression>();
            parameterExpressions.Add(Expression.Parameter(typeof(string), "RequestType"));
            parameterExpressions.Add(Expression.Parameter(typeof(string), "RequestStatus"));
            parameterExpressions.Add(Expression.Parameter(typeof(string), "RegistrationStatus"));

            Rule mainRule = new Rule();
            mainRule.RuleName = "rule1";
            mainRule.Operator = "And";
            mainRule.Rules = new List<Rule>();

            Rule dummyRule = new Rule();
            dummyRule.RuleName = "testRule1";
            dummyRule.RuleExpressionType = RuleExpressionType.LambdaExpression;
            dummyRule.Expression = "RequestType == \"vod\"";

            mainRule.Rules.Add(dummyRule);

            ParameterExpression ruleInputExp = Expression.Parameter(typeof(RuleInput), nameof(RuleInput));

            var expression = builder.BuildExpressionForRule(dummyRule, parameterExpressions, ruleInputExp);

            Assert.NotNull(expression);
            Assert.Equal(typeof(RuleResultTree), expression.ReturnType);
        }
    }
}
