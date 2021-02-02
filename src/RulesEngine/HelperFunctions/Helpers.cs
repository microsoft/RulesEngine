// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine.HelperFunctions
{
    /// <summary>
    /// Helpers
    /// </summary>
    internal static class Helpers
    {
        internal static RuleFunc<RuleResultTree> ToResultTree(Rule rule, IEnumerable<RuleResultTree> childRuleResults, Func<object[], bool> isSuccessFunc, string exceptionMessage = "")
        {
            return (inputs) => new RuleResultTree {
                Rule = rule,
                Inputs = inputs.ToDictionary(c => c.Name, c => c.Value),
                IsSuccess = isSuccessFunc(inputs.Select(c => c.Value).ToArray()),
                ChildResults = childRuleResults,
                ExceptionMessage = exceptionMessage
            };
        }

        /// <summary>
        /// To the result tree error messages
        /// </summary>
        /// <param name="ruleResultTree">ruleResultTree</param>
        /// <param name="ruleResultMessage">ruleResultMessage</param>
        [Obsolete]
        internal static void ToResultTreeMessages(RuleResultTree ruleResultTree, ref RuleResultMessage ruleResultMessage)
        {
            if (ruleResultTree.ChildResults != null)
            {
                GetChildRuleMessages(ruleResultTree.ChildResults, ref ruleResultMessage);
            }
            else
            {
                if (!ruleResultTree.IsSuccess)
                {
                    string errMsg = ruleResultTree.Rule.ErrorMessage;
                    errMsg = string.IsNullOrEmpty(errMsg) ? $"Error message is not configured for {ruleResultTree.Rule.RuleName}" : errMsg;

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
        [Obsolete]
        private static void GetChildRuleMessages(IEnumerable<RuleResultTree> childResultTree, ref RuleResultMessage ruleResultMessage)
        {
            foreach (var item in childResultTree)
            {
                ToResultTreeMessages(item, ref ruleResultMessage);
            }
        }
    }
}
