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

            // Surface duplicate GlobalParam names with a clear message instead of a cryptic
            // "An item with the same key has already been added" failure later at execution.
            RuleFor(c => c)
                .Must(workflow => FindDuplicateName(workflow.GlobalParams?.Select(p => p?.Name)) == null)
                .WithMessage(workflow => string.Format(
                    Constants.DUPLICATE_GLOBAL_PARAM_NAME_ERRMSG,
                    FindDuplicateName(workflow.GlobalParams?.Select(p => p?.Name))));
        }

        internal static string FindDuplicateName(System.Collections.Generic.IEnumerable<string> names)
        {
            if (names == null) return null;
            var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name)) continue;
                if (!seen.Add(name)) return name;
            }
            return null;
        }
    }
}
