// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace RulesEngine.Extensions;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> Safe<T>(this IEnumerable<T> enumerable)
    {
        return enumerable ?? Enumerable.Empty<T>();
    }
}