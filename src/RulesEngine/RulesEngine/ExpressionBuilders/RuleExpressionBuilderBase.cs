// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RulesEngine.ExpressionBuilders
{
    /// <summary>
    /// Base class for expression builders
    /// </summary>
    internal abstract class RuleExpressionBuilderBase
    {
        /// <summary>
        /// Builds the expression for rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="typeParamExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns>Expression type</returns>
        internal abstract Expression<Func<RuleInput, RuleResultTree>> BuildExpressionForRule(Rule rule, IEnumerable<ParameterExpression> typeParamExpressions, ParameterExpression ruleInputExp);

        /// <summary>Builds the expression for rule parameter.</summary>
        /// <param name="rule">The rule.</param>
        /// <param name="typeParamExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns>Expression.</returns>
        internal abstract Expression BuildExpressionForRuleParam(LocalParam rule, IEnumerable<ParameterExpression> typeParamExpressions, ParameterExpression ruleInputExp);
    }
}
