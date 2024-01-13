﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    public class RuleParameter
    {
        public RuleParameter(string name, object value)
        {
            Value = Utils.GetTypedObject(value);
            Init(name, Value?.GetType());
        }

       
        internal RuleParameter(string name, Type type,object value = null)
        {
            Value = Utils.GetTypedObject(value);
            Init(name, type);
        }

        public Type Type { get; private set; }
        public string Name { get; private set; }
        public object Value { get; private set; }
        public ParameterExpression ParameterExpression { get; private set; }

        private void Init(string name, Type type)
        {
            Name = name;
            Type = type ?? typeof(object);
            ParameterExpression = Expression.Parameter(Type, Name);
        }

        public static RuleParameter Create<T>(string name, T value)
        {
            var typedValue = Utils.GetTypedObject(value);
            var type = typedValue?.GetType() ?? typeof(T);
            return new RuleParameter(name,type,value);
        }


    }
}
