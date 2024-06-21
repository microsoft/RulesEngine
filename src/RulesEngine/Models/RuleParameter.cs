// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class RuleParameter
    {
        public RuleParameter(string name, object value) : this(name, value?.GetType(), value) { }       
        protected RuleParameter(string name, Type type, object value = null)
        {
            Name = name;
            Value = Utils.GetTypedObject(value);
            Type = type ?? typeof(object);
            ParameterExpression = Expression.Parameter(Type, Name);
        }

        public Type Type { get; private set; }
        public string Name { get; private set; }
        public object Value { get; private set; }
        public ParameterExpression ParameterExpression { get; private set; }

        public static RuleParameter Create<T>(string name, T value)
        {
            var typedValue = Utils.GetTypedObject(value);
            var type = typedValue?.GetType() ?? typeof(T);

            return new RuleParameter(name,type,value);
        }
    }
}
