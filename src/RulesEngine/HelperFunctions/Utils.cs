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

            return input;
        }

        public static Type CreateAbstractClassType(dynamic input)
        {
            var props = new List<DynamicProperty>();

            if (input == null)
            {
                return typeof(object);
            }
            if (input is not ExpandoObject expandoObject)
            {
                return input.GetType();
            }

            foreach (var expando in expandoObject)
            {
                var value = expando.Value switch {
                    IList list => GetListType(list),
                    _ => CreateAbstractClassType(expando.Value)
                };
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
                    switch (expando.Value)
                    {
                        case ExpandoObject:
                        {
                            var propType = propInfo.PropertyType;
                            val = CreateObject(propType, expando.Value);
                            break;
                        }
                        case IList temp:
                        {
                            var internalType = propInfo.PropertyType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                            var newList = new List<object>().Cast(internalType).ToList(internalType);
                            foreach (var t in temp)
                            {
                                var child = CreateObject(internalType, t);
                                newList.Add(child);
                            };
                            val = newList;
                            break;
                        }
                        default:
                            val = expando.Value;
                            break;
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

        private static Type GetNullableType(Type type)
        {
            // Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
            type = Nullable.GetUnderlyingType(type) ?? type; // avoid type becoming null
            return type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
        }

        private static Type GetListType(IList items)
        {
            if (items.Count == 0)
            {
                return typeof(List<object>);
            }

            Type internalType;
            if (items[0] is not ExpandoObject)
            {
                internalType = CreateAbstractClassType(items[0]);
            }
            else
            {
                var expandoItems = items.Cast<IDictionary<string, object>>().ToList();

                var props = new List<DynamicProperty>();

                // loop for each item to get all unique property and then create type
                foreach (var property in expandoItems
                             .SelectMany(expandoItem => expandoItem)
                             .Where(pair => !props.Any(x => x.Name.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))))
                {
                    var type = property.Value switch {
                        IList list => GetListType(list),
                        _ => CreateAbstractClassType(property.Value)
                    };

                    // if property not exist in all items then convert type to nullable type
                    var isPropertyExistInAllItems = expandoItems.All(x => x.ContainsKey(property.Key));
                    type = isPropertyExistInAllItems ? type : GetNullableType(type);
                    props.Add(new DynamicProperty(property.Key, type));
                }

                internalType = DynamicClassFactory.CreateType(props);
            }

            return new List<object>().Cast(internalType)!.ToList(internalType)!.GetType();
        }
    }
}