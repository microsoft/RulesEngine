// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;
using FluentValidation.Results;
using System.Collections.Generic;

namespace RulesEngine.Exceptions
{
    public class RuleValidationException : ValidationException
    {
        public RuleValidationException(string message, IEnumerable<ValidationFailure> errors) : base(message, errors)
        {
        }
    }
}
