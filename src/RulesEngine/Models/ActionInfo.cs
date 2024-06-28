// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class ActionInfo
    {
        public string Name { get; set; }
        public Dictionary<string, object> Context { get; set; }
    }
}
