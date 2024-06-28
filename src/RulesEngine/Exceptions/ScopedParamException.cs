// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.Exceptions
{
    public class ScopedParamException: Exception
    {
        public ScopedParamException(string message, Exception innerException, string scopedParamName): base(message,innerException)
        {
            Data.Add("ScopedParamName", scopedParamName);
        }
    }
}
