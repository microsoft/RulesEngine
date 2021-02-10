// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class ActionRuleResult : ActionResult
    {
        public List<RuleResultTree> Results { get; set; }
    }
}
