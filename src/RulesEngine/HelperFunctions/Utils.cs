// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace RulesEngine.HelperFunctions
{
    public static class Utils
    {
        public static object GetTypedObject(dynamic input, int sampleSize = 1)
        {
            if (input is ExpandoObject)
            {
                Type type = CreateAbstractClassType(input, sampleSize);
                return CreateObject(type, input);
            }
            else
            {
                return input;
            }
        }
        private static readonly List<Type> UnsignedNumericTypes = new List<Type> { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong)};
        private static readonly List<Type> SignedNumericTypes = new List<Type> { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double) };
        private static Type CoerceNumericTypes(int signedIndex1, int unsignedIndex1, int signedIndex2, int unsignedIndex2) {
            // If they are both signed value types, use the larger.
            if (signedIndex1 != -1 && signedIndex2 != -1)
                return SignedNumericTypes[Math.Max(signedIndex1, signedIndex2)];
            // If they are both unsigned value types, use the larger.
            if (unsignedIndex1 != -1 && unsignedIndex2 != -1)
                return UnsignedNumericTypes[Math.Max(unsignedIndex1, unsignedIndex2)];
            // If there is a signed/unsigned mismatch, we will need to use a signed value type.
            // If the signed value type is larger than the unsigned value type, use that.
            var signedIndex = Math.Max(signedIndex1, signedIndex2);
            var unsignedIndex = Math.Max(unsignedIndex1, unsignedIndex2);
            if (signedIndex > unsignedIndex)
                return SignedNumericTypes[signedIndex];
            // Otherwise we will need to increase the size.
            // There are fewer unsigned types than signed types, so we can't exceed the
            // signed-type array length by adding 1.
            return SignedNumericTypes[unsignedIndex + 1];
        }
        private static Type CoerceValueTypes(Type t1, Type t2) {
            // We might have a value type and a Nullable variant of that value type.
            // In that case, we stick with the Nullable variant.
            var underlyingType1 = Nullable.GetUnderlyingType(t1);
            var underlyingType2 = Nullable.GetUnderlyingType(t2);
            if (underlyingType1 == t2)
                return t1;
            if (underlyingType2 == t1)
                return t2;
            // It's possible that one type is a Nullable variant of a signed/unsigned type,
            // and the other is a vice-verse unsigned/signed type. We need to handle that.
            // So remember the Null-ness for later, and revert back to the underlying types.
            var makeNullable = underlyingType1 != null || underlyingType2 != null;
            t1 = underlyingType1 ?? t1;
            t2 = underlyingType2 ?? t2;
            // If we have a mix of value types, we might be able to come up with a compromise ...
            var signedIndex1 = SignedNumericTypes.IndexOf(t1);
            var unsignedIndex1 = UnsignedNumericTypes.IndexOf(t1);
            var signedIndex2 = SignedNumericTypes.IndexOf(t2);
            var unsignedIndex2 = UnsignedNumericTypes.IndexOf(t2);
            // Either or both types could still be non-numeric value types (bool, char, tuples, enums).
            // If this is the case, then once again, we have to use "object".
            if (((signedIndex1 == -1) && (unsignedIndex1 == -1)) || ((signedIndex2 == -1) && (unsignedIndex2 == -1)))
                return typeof(object);
            var numericType = CoerceNumericTypes(signedIndex1, unsignedIndex1, signedIndex2, unsignedIndex2);
            // If we found a Nullable type earlier, then use Nullable for this (potentially
            // different) type.
            return makeNullable ? typeof(Nullable<>).MakeGenericType(numericType) : numericType;
        }
        internal static Type CoerceTypes(Type t1, Type t2) {
            // If the types are the same, use that type.
            if (t1 == t2)
                return t1;
            // If one of the types is null, we can try to create a Nullable type from the other.
            // If the other is not a value type, then null is a valid value for that type.
            if ((t1 == null && t2 != null) || (t1 != null && t2 == null)) {
                var nonNullType = t1 ?? t2;
                // If the non-null type is already a Nullable type, it's good enough.
                return nonNullType.IsValueType && Nullable.GetUnderlyingType(nonNullType) == null ? typeof(Nullable<>).MakeGenericType(nonNullType) : nonNullType;
            }
            // Unless both types are value types, we'll have to use "object".
            if (!t1.IsValueType || !t2.IsValueType)
                return typeof(object);
            return CoerceValueTypes(t1, t2);
        }
        public static Type CreateAbstractClassType(dynamic input, int sampleSize = 1)
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
                            var firstType = expandoList[0] == null ? null : CreateAbstractClassType(expandoList[0], sampleSize);
                            var otherTypes = new List<Type>(expandoListCount);
                            var sampleCount = (sampleSize <= 0 ? expandoListCount : sampleSize);
                            for (int f = 1; f < sampleCount; ++f)
                                otherTypes.Add(expandoList[f] == null ? null : CreateAbstractClassType(expandoList[f], sampleSize));
                            // If there are any nulls, we want to deal with them LAST.
                            // Otherwise we might find an int, then a null (changing our type to Nullable<int>)
                            // then a double, then a null ... in that example, we would want Nullable<double> to
                            // be the result. Without re-ordering (or more complex logic in CoerceTypes), we
                            // would end up with "object" (or worse, an exception of some kind).
                            if (otherTypes.Any(t => t == null)) {
                                otherTypes = otherTypes.Where(t => t != null).ToList();
                                otherTypes.Add(null);
                            }
                            var internalType = otherTypes.Aggregate(firstType, CoerceTypes) ?? typeof(object);
                            value = new List<object>().Cast(internalType).ToList(internalType).GetType();
                        }
                    }
                    else
                    {
                        value = CreateAbstractClassType(expando.Value, sampleSize);
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
                try {
                    return Convert.ChangeType(input, Nullable.GetUnderlyingType(type) ?? type);
                } catch (InvalidCastException) {
                    return TypeDescriptor.GetConverter(type).ConvertFrom(input);
                }
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
