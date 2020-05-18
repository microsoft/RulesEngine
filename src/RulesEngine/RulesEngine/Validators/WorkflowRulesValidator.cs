// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using FluentValidation;
using Microsoft.Rules.HelperFunctions;
using Microsoft.Rules.Models;

namespace Microsoft.Rules.Validators
{
    internal class WorkflowRulesValidator : AbstractValidator<WorkflowRules>
    {
        public WorkflowRulesValidator()
        {
            RuleFor(c => c.WorkflowName).NotEmpty().WithMessage(Constants.WORKFLOW_NAME_NULL_ERRMSG);
            When(c => c.Rules?.Any() != true, () =>
            {
                RuleFor(c => c.WorkflowRulesToInject).NotEmpty().WithMessage(Constants.INJECT_WORKFLOW_RULES_ERRMSG);
            }).Otherwise(() => {
                var ruleValidator = new RuleValidator();
                RuleForEach(c => c.Rules).SetValidator(ruleValidator);
            });
        }
    }
}
