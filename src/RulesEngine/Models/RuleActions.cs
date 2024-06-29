// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [Obsolete("RuleAction class is deprecated. Use RuleActions class instead.")]
    [ExcludeFromCodeCoverage]
    public class RuleAction : RuleActions
    {
    }
  
    [ExcludeFromCodeCoverage]
    public class RuleActions
    {
        public ActionInfo OnSuccess { get; set; }
        public ActionInfo OnFailure { get; set; }
    }
}
