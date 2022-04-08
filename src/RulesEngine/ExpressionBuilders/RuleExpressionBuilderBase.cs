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
        internal abstract RuleFunc<RuleResultTree> BuildDelegateForRule(Rule rule, RuleParameter[] ruleParams);

        internal abstract Expression Parse(string expression, ParameterExpression[] parameters, Type returnType);

        internal abstract Func<object[], Dictionary<string, object>> CompileScopedParams(RuleParameter[] ruleParameters, RuleExpressionParameter[] scopedParameters);
    }
}
