// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Actions;
using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class ReSettings
    {
        /// <summary>
        /// Get/Set the custom types to be used in Rule expressions
        /// </summary>
        public Type[] CustomTypes { get; set; }

        /// <summary>
        /// Get/Set the custom actions that can be used in the Rules
        /// </summary>
        public Dictionary<string, Func<ActionBase>> CustomActions { get; set; }

        /// <summary>
        /// When set to true, returns any exception occurred 
        /// while rule execution as ErrorMessage 
        /// otherwise throws an exception
        /// </summary>
        /// <remarks>This setting is only applicable if IgnoreException is set to false</remarks>
        public bool EnableExceptionAsErrorMessage { get; set; } = true;

        /// <summary>
        /// When set to true, it will ignore any exception thrown with rule compilation/execution
        /// </summary>
        public bool IgnoreException { get; set; } = false;

        /// <summary>
        /// Enables ErrorMessage Formatting 
        /// </summary>
        public bool EnableFormattedErrorMessage { get; set; } = true;

        /// <summary>
        /// Enables Global params and local params for rules
        /// </summary>
        public bool EnableScopedParams { get; set; } = true;

        /// <summary>
        /// Sets the mode for Nested rule execution, Default: All
        /// </summary>
        public NestedRuleExecutionMode NestedRuleExecutionMode { get; set; } = NestedRuleExecutionMode.All;

        /// <summary>
        /// Enables Local params for rules
        /// </summary>
        [Obsolete("Use 'EnableScopedParams' instead. This will be removed in next major version")]
        public bool EnableLocalParams {
            get { return EnableScopedParams; }
            set { EnableScopedParams = value; }
        }

        public MemCacheConfig CacheConfig { get; set; }
    }

    public enum NestedRuleExecutionMode
    {
        /// <summary>
        /// Executes all nested rules
        /// </summary>
        All,
        /// <summary>
        /// Skips nested rules whose execution does not impact parent rule's result
        /// </summary>
        Performance
    }
}
