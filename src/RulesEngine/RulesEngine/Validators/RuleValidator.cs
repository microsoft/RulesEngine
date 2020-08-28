﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation;
using RulesEngine.HelperFunctions;
using RulesEngine.Models;

namespace RulesEngine.Validators
{
    internal class RuleValidator : AbstractValidator<Rule>
    {
        private readonly List<ExpressionType> _nestedOperators = new List<ExpressionType> { ExpressionType.And, ExpressionType.AndAlso, ExpressionType.Or, ExpressionType.OrElse };
        public RuleValidator()
        {
            RuleFor(c => c.RuleName).NotEmpty().WithMessage(Constants.RULE_NAME_NULL_ERRMSG);

            //Nested expression check
            When(c => c.RuleExpressionType == null,() =>
            {
                RuleFor(c => c.Operator)
                   .NotNull().WithMessage(Constants.OPERATOR_NULL_ERRMSG)
                   .Must(op => _nestedOperators.Any(x => x.ToString().Equals(op, StringComparison.OrdinalIgnoreCase)))
                   .WithMessage(Constants.OPERATOR_INCORRECT_ERRMSG);

                When(c => c.Rules?.Any() != true, () =>
                {
                    RuleFor(c => c.WorkflowRulesToInject).NotEmpty().WithMessage(Constants.INJECT_WORKFLOW_RULES_ERRMSG);
                })
                .Otherwise(() => {
                    RuleFor(c => c.Rules).Must(BeValidRulesList);
                });
            });
            RegisterExpressionTypeRules();
        }

        private void RegisterExpressionTypeRules()
        {
            When(c => c.RuleExpressionType == RuleExpressionType.LambdaExpression, () =>
            {
                RuleFor(c => c.Expression).NotEmpty().WithMessage(Constants.LAMBDA_EXPRESSION_EXPRESSION_NULL_ERRMSG);
                RuleFor(c => c.Operator).Null().WithMessage(Constants.LAMBDA_EXPRESSION_OPERATOR_ERRMSG);
                RuleFor(c => c.Rules).Null().WithMessage(Constants.LAMBDA_EXPRESSION_RULES_ERRMSG);
            });
        }

        private bool BeValidRulesList(List<Rule> rules)
        {
            if (rules?.Any() != true) return false;
            var validator = new RuleValidator();
            var isValid = true;
            foreach(var rule in rules){
                isValid &= validator.Validate(rule).IsValid;
                if (!isValid) break;
            }
            return isValid;
        }
    }
}
