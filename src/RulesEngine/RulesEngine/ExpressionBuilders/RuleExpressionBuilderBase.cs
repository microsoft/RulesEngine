﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Rules.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Rules.ExpressionBuilders
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
    }
}
