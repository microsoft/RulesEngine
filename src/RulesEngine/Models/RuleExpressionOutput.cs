// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.Models
{
    public class RuleExpressionOutput<T>
    {
        public Dictionary<string,object> Inputs { get; set; }
        public T Output { get; set; }
    }
}
