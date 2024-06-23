// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentValidation;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RulesEngine.Validators;

internal class RuleValidator : AbstractValidator<Rule>
{
    /// <summary>
    ///     List of supported operators.
    /// </summary>
    private readonly List<ExpressionType> _nestedOperators =
        [ExpressionType.And, ExpressionType.AndAlso, ExpressionType.Or, ExpressionType.OrElse];

    /// <summary>
    ///     Validates the rule object.
    ///     If the rule object has nested rules, it validates the nested rules as well.
    ///     The rule name should not be null or empty.
    ///     The operator should be one of the supported operators.
    /// </summary>
    public RuleValidator()
    {
        RuleFor(c => c.RuleName).NotEmpty().WithMessage(Constants.RULE_NAME_NULL_ERRMSG);

        //Nested expression check
        When(c => c.Operator != null, () => {
            RuleFor(c => c.Operator)
                .NotNull().WithMessage(Constants.OPERATOR_NULL_ERRMSG)
                .Must(op => _nestedOperators.Exists(x => x.ToString().Equals(op, StringComparison.OrdinalIgnoreCase)))
                .WithMessage(Constants.OPERATOR_INCORRECT_ERRMSG)
                .OverridePropertyName($"Method: {nameof(Rule.Rules)}");

            When(c => c.Rules?.Any() != true, () => {
                    RuleFor(c => c.WorkflowsToInject).NotEmpty().WithMessage(Constants.INJECT_WORKFLOW_RULES_ERRMSG);
                })
                .Otherwise(() => {
                    RuleFor(c => c.Rules).Must(BeValidRulesList);
                });
        });
        RegisterExpressionTypeRules();
    }

    private void RegisterExpressionTypeRules()
    {
        When(c => c.Operator == null && c.RuleExpressionType == RuleExpressionType.LambdaExpression, () => {
            RuleFor(c => c.Expression).NotEmpty().WithMessage(Constants.LAMBDA_EXPRESSION_EXPRESSION_NULL_ERRMSG);
            RuleFor(c => c.Rules).Empty().WithMessage(Constants.OPERATOR_RULES_ERRMSG);
        });
    }

    private bool BeValidRulesList(IEnumerable<Rule> rules)
    {
        var enumerable = rules as Rule[] ?? rules.ToArray();
        if (enumerable.Length <= 0)
        {
            return false;
        }

        var validator = new RuleValidator();
        var isValid = true;
        foreach (var rule in enumerable)
        {
            isValid &= validator.Validate(rule).IsValid;
            if (!isValid)
            {
                break;
            }
        }

        return isValid;
    }
}