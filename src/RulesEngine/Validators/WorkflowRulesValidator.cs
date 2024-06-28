// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System.Linq;

namespace RulesEngine.Validators
{
    internal class WorkflowsValidator : AbstractValidator<Workflow>
    {
        public WorkflowsValidator()
        {
            RuleFor(c => c.WorkflowName).NotEmpty().WithMessage(Constants.WORKFLOW_NAME_NULL_ERRMSG);
            When(c => c.Rules?.Any() != true, () => {
                RuleFor(c => c.WorkflowsToInject).NotEmpty().WithMessage(Constants.INJECT_WORKFLOW_RULES_ERRMSG);
            }).Otherwise(() => {
                var ruleValidator = new RuleValidator();
                RuleForEach(c => c.Rules).SetValidator(ruleValidator);
            });
        }
    }
}
