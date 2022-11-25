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
        private static readonly List<Type> UnsignedNumericTypes = new List<Type> { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong)};
        private static readonly List<Type> SignedNumericTypes = new List<Type> { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double) };
        internal static Type CoerceNumericTypes(Type t1, Type t2) {
            // TODO: A bit more intelligence maybe? It could check the actual value
            // instead of "just" the type to see if the value is within the range of
            // the current type.
            if (t1 == t2)
                return t1;
            // If either type is "object", we're stuck.
            if (t1 == typeof(object) || t2 == typeof(object))
                return typeof(object);
            var signedIndex1 = SignedNumericTypes.IndexOf(t1);
            var unsignedIndex1 = UnsignedNumericTypes.IndexOf(t1);
            var signedIndex2 = SignedNumericTypes.IndexOf(t2);
            var unsignedIndex2 = UnsignedNumericTypes.IndexOf(t2);
            // If neither type is a number, we're stuck.
            if ((signedIndex1 == -1 && unsignedIndex1 == -1) || (signedIndex2 == -1 && unsignedIndex2 == -1))
                return typeof(object);
            // If they are both signed types, use the larger.
            if (signedIndex1 != -1 && signedIndex2 != -1)
                return SignedNumericTypes[Math.Max(signedIndex1, signedIndex2)];
            // If they are both unsigned types, use the larger.
            if (unsignedIndex1 != -1 && unsignedIndex2 != -1)
                return UnsignedNumericTypes[Math.Max(unsignedIndex1, unsignedIndex2)];
            // If there is a signed/unsigned mismatch, we will need to use a signed type.
            // If the signed type is larger than the unsigned type, use that.
            var signedIndex = Math.Max(signedIndex1, signedIndex2);
            var unsignedIndex = Math.Max(unsignedIndex1, unsignedIndex2);
            if (signedIndex > unsignedIndex)
                return SignedNumericTypes[signedIndex];
            // Otherwise we will need to step up the size.
            return SignedNumericTypes[Math.Min(unsignedIndex+1,SignedNumericTypes.Count-1)];
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
                        var expandoList = (IList)expando.Value;
                        var expandoListCount = expandoList.Count;
                        if (expandoListCount == 0)
                            value = typeof(List<object>);
                        else
                        {
                            var firstType = CreateAbstractClassType(expandoList[0]);
                            var otherTypes = new List<Type>(expandoListCount);
                            for(int f = 1; f < expandoListCount; ++f)
                                otherTypes.Add(CreateAbstractClassType(expandoList[f]));
                            var internalType = otherTypes.Aggregate(firstType, CoerceNumericTypes);
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
