// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ScopedParamException: Exception
    {
        public ScopedParamException(string message, Exception innerException, string scopedParamName): base(message,innerException)
        {
            Data.Add("ScopedParamName", scopedParamName);
        }
    }
}
