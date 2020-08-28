// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RulesEngine.HelperFunctions
{
    /// <summary>
    /// Helpers
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// To the result tree expression.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="childRuleResults">The child rule results.</param>
        /// <param name="isSuccessExp">The is success exp.</param>
        /// <param name="typeParamExpressions">The type parameter expressions.</param>
        /// <param name="ruleInputExp">The rule input exp.</param>
        /// <returns>Expression of func</returns>
        internal static Expression<Func<RuleInput, RuleResultTree>> ToResultTreeExpression(Rule rule, IEnumerable<MemberInitExpression> childRuleResults, BinaryExpression isSuccessExp, IEnumerable<ParameterExpression> typeParamExpressions, ParameterExpression ruleInputExp, string exceptionMessage = "")
        {
            var memberInit = ToResultTree(rule, childRuleResults, isSuccessExp, typeParamExpressions, null, exceptionMessage);
            var lambda = Expression.Lambda<Func<RuleInput, RuleResultTree>>(memberInit, new[] { ruleInputExp });
            return lambda;
        }

        /// <summary>
        /// To the result tree member expression 
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="childRuleResults">The child rule results.</param>
        /// <param name="isSuccessExp">The is success exp.</param>
        /// <param name="childRuleResultsblockexpr">The child rule results block expression.</param>
        /// <returns></returns>
        internal static MemberInitExpression ToResultTree(Rule rule, IEnumerable<MemberInitExpression> childRuleResults, BinaryExpression isSuccessExp, IEnumerable<ParameterExpression> typeParamExpressions, BlockExpression childRuleResultsblockexpr, string exceptionMessage = "")
        {
            var createdType = typeof(RuleResultTree);
            var ctor = Expression.New(createdType);

            var ruleProp = createdType.GetProperty(nameof(RuleResultTree.Rule));
            var isSuccessProp = createdType.GetProperty(nameof(RuleResultTree.IsSuccess));
            var childResultProp = createdType.GetProperty(nameof(RuleResultTree.ChildResults));
            var inputProp = createdType.GetProperty(nameof(RuleResultTree.Input));
            var exceptionProp = createdType.GetProperty(nameof(RuleResultTree.ExceptionMessage));

            var rulePropBinding = Expression.Bind(ruleProp, Expression.Constant(rule));
            var isSuccessPropBinding = Expression.Bind(isSuccessProp, isSuccessExp);
            var inputBinding = Expression.Bind(inputProp, typeParamExpressions.FirstOrDefault());
            var exceptionBinding = Expression.Bind(exceptionProp, Expression.Constant(exceptionMessage));

            MemberInitExpression memberInit;

            if (childRuleResults != null)
            {
                var ruleResultTreeArr = Expression.NewArrayInit(typeof(RuleResultTree), childRuleResults);

                var childResultPropBinding = Expression.Bind(childResultProp, ruleResultTreeArr);
                memberInit = Expression.MemberInit(ctor, new[] { rulePropBinding, isSuccessPropBinding, childResultPropBinding, inputBinding, exceptionBinding });
            }
            else if (childRuleResultsblockexpr != null)
            {
                var childResultPropBinding = Expression.Bind(childResultProp, childRuleResultsblockexpr);
                memberInit = Expression.MemberInit(ctor, new[] { rulePropBinding, isSuccessPropBinding, childResultPropBinding, inputBinding, exceptionBinding });
            }
            else
            {
                memberInit = Expression.MemberInit(ctor, new[] { rulePropBinding, isSuccessPropBinding, inputBinding, exceptionBinding });
            }

            return memberInit;
        }

        /// <summary>
        /// To the result tree error messages
        /// </summary>
        /// <param name="ruleResultTree">ruleResultTree</param>
        /// <param name="ruleResultMessage">ruleResultMessage</param>
        internal static void ToResultTreeMessages(RuleResultTree ruleResultTree, ref RuleResultMessage ruleResultMessage)
        {
            if (ruleResultTree.ChildResults != null)
            {
                GetChildRuleMessages(ruleResultTree.ChildResults, ref ruleResultMessage);
            }
            else
            {
                if (ruleResultTree.IsSuccess)
                {
                    string errMsg = ruleResultTree.Rule.ErrorMessage;
                    errMsg = string.IsNullOrEmpty(errMsg) ? $"Error message does not configured for {ruleResultTree.Rule.RuleName}" : errMsg;

                    if (ruleResultTree.Rule.ErrorType == ErrorType.Error && !ruleResultMessage.ErrorMessages.Contains(errMsg))
                    {
                        ruleResultMessage.ErrorMessages.Add(errMsg);
                    }
                    else if (ruleResultTree.Rule.ErrorType == ErrorType.Warning && !ruleResultMessage.WarningMessages.Contains(errMsg))
                    {
                        ruleResultMessage.WarningMessages.Add(errMsg);
                    }
                }
            }
        }

        /// <summary>
        /// To get the child error message recersivly
        /// </summary>
        /// <param name="childResultTree">childResultTree</param>
        /// <param name="ruleResultMessage">ruleResultMessage</param>
        private static void GetChildRuleMessages(IEnumerable<RuleResultTree> childResultTree, ref RuleResultMessage ruleResultMessage)
        {
            foreach (var item in childResultTree)
            {
                ToResultTreeMessages(item, ref ruleResultMessage);
            }
        }
    }
}
