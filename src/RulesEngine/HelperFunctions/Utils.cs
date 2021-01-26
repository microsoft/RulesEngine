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
            List<DynamicProperty> props = new List<DynamicProperty>();

            if (input == null)
            {
                return typeof(object);
            }
            if (!(input is ExpandoObject))
            {
                return input.GetType();
            }

            else
            {
                foreach (var expando in (IDictionary<string, object>)input)
                {
                    Type value;
                    if (expando.Value is IList)
                    {
                        if (((IList)expando.Value).Count == 0)
                            value = typeof(List<object>);
                        else
                        {
                            var internalType = CreateAbstractClassType(((IList)expando.Value)[0]);
                            value = new List<object>().Cast(internalType).ToList(internalType).GetType();
                        }

                    }
                    else
                    {
                        value = CreateAbstractClassType(expando.Value);
                    }
                    props.Add(new DynamicProperty(expando.Key, value));
                }
            }

            var type = DynamicClassFactory.CreateType(props);
            return type;
        }

        public static object CreateObject(Type type, dynamic input)
        {
            if (!(input is ExpandoObject))
            {
                return Convert.ChangeType(input, type);
            }
            object obj = Activator.CreateInstance(type);

            var typeProps = type.GetProperties().ToDictionary(c => c.Name);

            foreach (var expando in (IDictionary<string, object>)input)
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
                    else if (expando.Value is IList)
                    {
                        var internalType = propInfo.PropertyType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                        var temp = (IList)expando.Value;
                        var newList = new List<object>().Cast(internalType).ToList(internalType);
                        for (int i = 0; i < temp.Count; i++)
                        {
                            var child = CreateObject(internalType, temp[i]);
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
