// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace RulesEngine.Exceptions;

public class RuleException : Exception
{
    public RuleException(string message) : base(message)
    {
    }

    public RuleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}