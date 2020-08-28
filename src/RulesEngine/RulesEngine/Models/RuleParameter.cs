// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using RulesEngine.HelperFunctions;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class RuleParameter
    {
        public RuleParameter(string name,object value)
        {
            Value = Utils.GetTypedObject(value);
            Type = Value.GetType();
            Name = name;
        }

        public Type Type { get; }
        public string Name { get; }
        public object Value { get; }

    }
}
