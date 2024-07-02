// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace RulesEngine.HelperFunctions
{
    public static class JsonElementExtensions
    {
        public static dynamic ToExpandoObject(this JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var expandoObject = new ExpandoObject() as IDictionary<string, object>;
                    foreach (var property in element.EnumerateObject())
                    {
                        expandoObject[property.Name] = property.Value.ToExpandoObject();
                    }
                    return expandoObject;

                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(item.ToExpandoObject());
                    }
                    return list;

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt32(out var intValue))
                    {
                        return intValue;
                    }
                    if (element.TryGetInt64(out var longValue))
                    {
                        return longValue;
                    }

                    return element.GetDecimal();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                default:
                    return null;
            }
        }
    }
}