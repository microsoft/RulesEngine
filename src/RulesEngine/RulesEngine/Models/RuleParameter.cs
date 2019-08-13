// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class RuleParameter
    {
        public RuleParameter(Type type)
        {
            Type = type;
            Name = type.Name;
        }
        public RuleParameter(Type type,string name)
        {
            Type = type;
            Name = name;
        }

        public RuleParameter(string name,object value)
        {
            Type = value.GetType();
            Name = name;
            Value = value;
        }


        public Type Type { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

    }
}
