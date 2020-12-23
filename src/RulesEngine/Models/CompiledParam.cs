// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>
    /// CompiledParam class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class CompiledParam
    {
        internal string Name { get; set; }
        internal Type ReturnType { get; set; }
        internal Func<object[], object> Value { get; set; }
        internal RuleParameter AsRuleParameter()
        {
            return new RuleParameter(Name, ReturnType);
        }
    }
}
