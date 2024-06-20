// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class RuleActions
    {
        public ActionInfo OnSuccess { get; set; }
        public ActionInfo OnFailure { get; set; }
    }
}
