// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class ActionResult
    {
        public object Output { get; set; }
        public Exception Exception { get; set; }
    }
}
