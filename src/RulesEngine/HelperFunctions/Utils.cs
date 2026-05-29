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
            else if (input is IDictionary<string, object> dict)
            {
                Type type = CreateAbstractClassTypeFromDictionary(dict);
                return CreateObjectFromDictionary(type, dict);
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
                    value = BuildListType(list);
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

        // Returns the CLR List<T> type that should represent a heterogeneous IList of ExpandoObject /
        // IDictionary<string, object> elements. Walks every element so properties that only appear in
        // later elements are still included in the generated type. See #704.
        private static Type BuildListType(IList list)
        {
            if (list.Count == 0)
            {
                return typeof(List<object>);
            }

            var firstElement = list[0];
            if (firstElement is ExpandoObject || firstElement is IDictionary<string, object>)
            {
                var merged = MergeDictionaries(list.OfType<IDictionary<string, object>>());
                var internalType = CreateAbstractClassTypeFromDictionary(merged);
                return new List<object>().Cast(internalType).ToList(internalType).GetType();
            }

            // Non-schema-like element: fall back to first-element type as before.
            var legacyType = CreateAbstractClassType(firstElement);
            return new List<object>().Cast(legacyType).ToList(legacyType).GetType();
        }

        // Unions schemas from any number of dict-like inputs. Used both to merge sibling
        // elements of a heterogeneous list (#704) and to merge nested dicts recursively.
        private static IDictionary<string, object> MergeDictionaries(IEnumerable<IDictionary<string, object>> dictionaries)
        {
            var merged = new Dictionary<string, object>();
            foreach (var dict in dictionaries)
            {
                foreach (var kvp in dict)
                {
                    merged[kvp.Key] = merged.TryGetValue(kvp.Key, out var existing)
                        ? MergeValues(existing, kvp.Value)
                        : kvp.Value;
                }
            }
            return merged;
        }

        private static object MergeValues(object existing, object incoming)
        {
            if (existing is IDictionary<string, object> a && incoming is IDictionary<string, object> b)
            {
                return MergeDictionaries(new[] { a, b });
            }
            if (existing is IList la && incoming is IList lb)
            {
                var combined = new List<object>();
                foreach (var e in la) combined.Add(e);
                foreach (var e in lb) combined.Add(e);
                return combined;
            }
            // First non-null wins on type conflict.
            return existing ?? incoming;
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

        private static Type CreateAbstractClassTypeFromDictionary(IDictionary<string, object> dictionary)
        {
            List<DynamicProperty> props = [];

            foreach (var kvp in dictionary)
            {
                Type valueType;
                if (kvp.Value is ExpandoObject)
                {
                    valueType = CreateAbstractClassType(kvp.Value);
                }
                else if (kvp.Value is IDictionary<string, object> nestedDict)
                {
                    valueType = CreateAbstractClassTypeFromDictionary(nestedDict);
                }
                else if (kvp.Value is IList list)
                {
                    valueType = BuildListType(list);
                }
                else
                {
                    valueType = kvp.Value?.GetType() ?? typeof(object);
                }
                props.Add(new DynamicProperty(kvp.Key, valueType));
            }

            return DynamicClassFactory.CreateType(props);
        }

        private static object CreateObjectFromDictionary(Type type, IDictionary<string, object> dictionary)
        {
            var obj = Activator.CreateInstance(type);
            var typeProps = type.GetProperties().ToDictionary(c => c.Name);

            foreach (var kvp in dictionary)
            {
                if (typeProps.ContainsKey(kvp.Key) &&
                    kvp.Value != null && (kvp.Value.GetType().Name != "DBNull" || kvp.Value != DBNull.Value))
                {
                    object val;
                    var propInfo = typeProps[kvp.Key];
                    if (kvp.Value is ExpandoObject)
                    {
                        val = CreateObject(propInfo.PropertyType, kvp.Value);
                    }
                    else if (kvp.Value is IDictionary<string, object> nestedDict)
                    {
                        val = CreateObjectFromDictionary(propInfo.PropertyType, nestedDict);
                    }
                    else if (kvp.Value is IList temp)
                    {
                        var internalType = propInfo.PropertyType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                        var newList = new List<object>().Cast(internalType).ToList(internalType);
                        foreach (var t in temp)
                        {
                            var child = t is IDictionary<string, object> d
                                ? CreateObjectFromDictionary(internalType, d)
                                : (t is ExpandoObject ? CreateObject(internalType, t) : t);
                            newList.Add(child);
                        }
                        val = newList;
                    }
                    else
                    {
                        val = kvp.Value;
                    }
                    propInfo.SetValue(obj, val, null);
                }
            }

            return obj;
        }
    }


}
