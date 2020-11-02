// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class ReSettings
    {
        public Type[] CustomTypes { get; set; }

        public Dictionary<string, Func<ActionBase>> CustomActions { get; set; }

        public bool EnableExceptionAsErrorMessage { get; set; } = true;
    }
}
