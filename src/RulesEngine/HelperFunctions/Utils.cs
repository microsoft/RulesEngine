// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace RulesEngine.HelperFunctions
{
    public static class Utils
    {
        public static object GetTypedObject(dynamic input)
        {
            if (input is ExpandoObject)
            {
                Type type = CreateAbstractClassType(input);
                return CreateObject(type, input);
            }
            else
            {
                return input;
            }
        }
        public static Type CreateAbstractClassType(dynamic input)
        {
            List<DynamicProperty> props = [];

            if (input is System.Text.Json.JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                {
                    return typeof(object);
                }
            }
            else if (input == null)
            {
                return typeof(object);
            }

            if (input is not ExpandoObject expandoObject)
            {
                return input.GetType();
            }

            foreach (var expando in expandoObject)
            {
                Type value;
                if (expando.Value is IList list)
                {
                    if (list.Count == 0)
                    {
                        value = typeof(List<object>);
                    }
                    else
                    {
                        var internalType = CreateAbstractClassType(list[0]);
                        value = new List<object>().Cast(internalType).ToList(internalType).GetType();
                    }

                }
                else
                {
                    value = CreateAbstractClassType(expando.Value);
                }
                props.Add(new DynamicProperty(expando.Key, value));
            }

            var type = DynamicClassFactory.CreateType(props);
            return type;
        }

        public static object CreateObject(Type type, dynamic input)
        {
            if (input is not ExpandoObject expandoObject)
            {
                return Convert.ChangeType(input, type);
            }
            var obj = Activator.CreateInstance(type);

            var typeProps = type.GetProperties().ToDictionary(c => c.Name);

            foreach (var expando in expandoObject)
            {
                if (typeProps.ContainsKey(expando.Key) &&
                    expando.Value != null && (expando.Value.GetType().Name != "DBNull" || expando.Value != DBNull.Value))
                {
                    object val;
                    var propInfo = typeProps[expando.Key];
                    if (expando.Value is ExpandoObject)
                    {
                        var propType = propInfo.PropertyType;
                        val = CreateObject(propType, expando.Value);
                    }
                    else if (expando.Value is IList temp)
                    {
                        var internalType = propInfo.PropertyType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                        var newList = new List<object>().Cast(internalType).ToList(internalType);
                        foreach (var t in temp)
                        {
                            var child = CreateObject(internalType, t);
                            newList.Add(child);
                        };
                        val = newList;
                    }
                    else
                    {
                        val = expando.Value;
                    }
                    propInfo.SetValue(obj, val, null);
                }
            }

            return obj;
        }

        private static IEnumerable Cast(this IEnumerable self, Type innerType)
        {
            var methodInfo = typeof(Enumerable).GetMethod("Cast");
            var genericMethod = methodInfo.MakeGenericMethod(innerType);
            return genericMethod.Invoke(null, new[] { self }) as IEnumerable;
        }

        private static IList ToList(this IEnumerable self, Type innerType)
        {
            var methodInfo = typeof(Enumerable).GetMethod("ToList");
            var genericMethod = methodInfo.MakeGenericMethod(innerType);
            return genericMethod.Invoke(null, new[] { self }) as IList;
        }
    }


}
