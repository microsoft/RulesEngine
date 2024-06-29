// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            var reSettings = new ReSettings();
            var objBuilderFactory = new RuleExpressionBuilderFactory(reSettings, new RuleExpressionParser(reSettings));
            var builder = objBuilderFactory.RuleGetExpressionBuilder(RuleExpressionType.LambdaExpression);

            var ruleParameters = new RuleParameter[] {
                new RuleParameter("RequestType","Sales"),
                new RuleParameter("RequestStatus", "Active"),
                new RuleParameter("RegistrationStatus", "InProcess")
            };


            var mainRule = new Rule {
                RuleName = "rule1",
                Operator = "And",
                Rules = new List<Rule>()
            };

            var dummyRule = new Rule {
                RuleName = "testRule1",
                RuleExpressionType = RuleExpressionType.LambdaExpression,
                Expression = "RequestType == \"vod\""
            };

            mainRule.Rules = mainRule.Rules.Append(dummyRule);
            var func = builder.BuildDelegateForRule(dummyRule, ruleParameters);

            Assert.NotNull(func);
            Assert.Equal(typeof(RuleResultTree), func.Method.ReturnType);
        }
    }
}
