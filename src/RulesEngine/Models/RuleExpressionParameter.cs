// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RulesEngine.Models
{
    /// <summary>
    /// CompiledParam class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RuleExpressionParameter
    {
        public ParameterExpression ParameterExpression { get; set; }
        
        public Expression ValueExpression { get; set; }

    }
}
