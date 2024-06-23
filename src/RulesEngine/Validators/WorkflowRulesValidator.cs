using FluentValidation;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System.Linq;

namespace RulesEngine.Validators;

internal class WorkflowsValidator : AbstractValidator<Workflow>
{
    /// <summary>
    ///     Validates the workflow object.
    ///     The workflow name should not be null or empty.
    ///     The workflow should have at least one rule or a list of workflows to inject.
    ///     If the workflow has rules, then the rules should be validated.
    /// </summary>
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