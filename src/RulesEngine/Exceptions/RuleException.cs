// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class RuleException : Exception
    {
        public RuleException(string message) : base(message)
        {
        }

        public RuleException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
