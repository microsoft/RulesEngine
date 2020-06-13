// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    internal class CompiledRule
    {
        /// <summary>
        /// Gets or sets the compiled rules.
        /// </summary>
        /// <value>
        /// The compiled rules.
        /// </value>
        internal Delegate Rule { get; set; }


        /// <summary>
        /// Gets or sets the rule parameters.
        /// </summary>
        /// <value>
        /// The rule parameters.
        /// </value>
        internal CompiledRuleParam CompiledParameters { get; set; }
    }

}
