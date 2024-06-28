// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace RulesEngine.HelperFunctions
{
    public static class ExpressionUtils
    {
        //TODO: add more helper functions

        /// <summary>
        /// check that comma delimited string list contains a string
        /// </summary>
        /// <param name="check">string to find</param>
        /// <param name="valList">comma delimited string</param>
        /// <returns></returns>
        public static bool CheckContains(string check, string valList)
        {
            if (string.IsNullOrEmpty(check) || string.IsNullOrEmpty(valList))
                return false;

            var list = valList.Split(',').ToList();
            return list.Contains(check);
        }
    }
}
