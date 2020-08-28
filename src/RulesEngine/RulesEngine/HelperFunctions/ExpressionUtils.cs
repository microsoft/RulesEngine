// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace RulesEngine.HelperFunctions
{
    public static class ExpressionUtils
    {
        public static bool CheckContains(string check, string valList)
        {
            if (String.IsNullOrEmpty(check) || String.IsNullOrEmpty(valList))
                return false;

            var list = valList.Split(',').ToList();
            return list.Contains(check);
        }
    }
}
