// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class LambdaExpressionBuilderTest
    {
        [Fact]
        public void BuildExpressionForRuleTest()
        {
            var objBuilderFactory = new RuleExpressionBuilderFactory(new ReSettings());
            var builder = objBuilderFactory.RuleGetExpressionBuilder(RuleExpressionType.LambdaExpression);

            var ruleParameters = new RuleParameter[] {
                new RuleParameter("RequestType","Sales"),
                new RuleParameter("RequestStatus", "Active"),
                new RuleParameter("RegistrationStatus", "InProcess")
            };


            Rule mainRule = new Rule();
            mainRule.RuleName = "rule1";
            mainRule.Operator = "And";
            mainRule.Rules = new List<Rule>();

            Rule dummyRule = new Rule();
            dummyRule.RuleName = "testRule1";
            dummyRule.RuleExpressionType = RuleExpressionType.LambdaExpression;
            dummyRule.Expression = "RequestType == \"vod\"";

            mainRule.Rules.Add(dummyRule);
            var func = builder.BuildDelegateForRule(dummyRule, ruleParameters);

            Assert.NotNull(func);
            Assert.Equal(typeof(RuleResultTree), func.Method.ReturnType);
        }
    }
}
