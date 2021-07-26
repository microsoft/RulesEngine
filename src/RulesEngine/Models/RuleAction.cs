// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class RuleAction
    {
        public ActionInfo OnSuccess { get; set; }
        public ActionInfo OnFailure { get; set; }
    }
}
