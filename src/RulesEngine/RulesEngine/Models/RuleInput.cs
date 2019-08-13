// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RulesEngine.Models
{
    /// <summary>
    /// Rule input
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class RuleInput
    {
        /// <summary>
        /// Gets the today UTC.
        /// </summary>
        /// <value>
        /// The today UTC.
        /// </value>
        public DateTime TodayUtc { get; set; }
    }
}
