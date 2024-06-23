// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Exceptions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine.HelperFunctions;

/// <summary>
///     Helpers
/// </summary>
internal static class Helpers
{
    /// <summary>
    ///     Converts the function to result tree.
    /// </summary>
    /// <param name="reSettings">The <see cref="ReSettings" /> to use for this operation.</param>
    /// <param name="rule">The <see cref="IRule" /> for Exception handling.</param>
    /// <param name="childRuleResults">The <see cref="RuleResultTree" /> of the child rules.</param>
    /// <param name="isSuccessFunc">The function to check if the rule is successful.</param>
    /// <param name="exceptionMessage">The exception message if the isSuccessFunc throws an Exception</param>
    /// <returns>The <see cref="RuleFunc{RuleResultTree}" /> function.</returns>
    internal static RuleFunc<RuleResultTree> ToResultTree(ReSettings reSettings, Rule rule,
        IEnumerable<RuleResultTree> childRuleResults, Func<object[], bool> isSuccessFunc, string exceptionMessage = "")
    {
        return inputs => {
            bool isSuccess;
            var inputsDict = new Dictionary<string, object>();
            try
            {
                inputsDict = inputs.ToDictionary(c => c.Name, c => c.Value);
                isSuccess = isSuccessFunc(inputs.Select(c => c.Value).ToArray());
            }
            catch (Exception ex)
            {
                exceptionMessage = TryGetExceptionMessage(
                    $"Error while executing rule : {rule?.RuleName} - {ex.Message}",
                    reSettings);
                HandleRuleException(new RuleException(exceptionMessage, ex), rule, reSettings);
                isSuccess = false;
            }

            return new RuleResultTree {
                Rule = rule,
                Inputs = inputsDict,
                IsSuccess = isSuccess,
                ChildResults = childRuleResults,
                ExceptionMessage = exceptionMessage
            };
        };
    }

    /// <summary>
    ///     Takes the <see cref="ReSettings" />, <see cref="Rule" />, and <see cref="Exception" /> and returns a
    ///     <see cref="RuleFunc{RuleResultTree}" /> function.
    ///     This function is used to handle exceptions in the Rule.
    /// </summary>
    /// <param name="reSettings">The <see cref="ReSettings" /> to use for this operation.</param>
    /// <param name="rule">The <see cref="Rule" /> for Exception handling.</param>
    /// <param name="ex">The <see cref="Exception" /> to handle.</param>
    /// <returns>The <see cref="RuleFunc{RuleResultTree}" /> function.</returns>
    internal static RuleFunc<RuleResultTree> ToRuleExceptionResult(ReSettings reSettings, Rule rule, Exception ex)
    {
        HandleRuleException(ex, rule, reSettings);
        return ToResultTree(reSettings, rule, null, _ => false, ex.Message);
    }

    /// <summary>
    ///     Handles the rule exception.
    /// </summary>
    /// <param name="ex">The <see cref="Exception" /> to handle.</param>
    /// <param name="rule">The <see cref="Rule" /> for Exception handling.</param>
    /// <param name="reSettings">The <see cref="ReSettings" /> to check if the exception should be thrown or not.</param>
    /// <exception cref="Exception">
    ///     If the <see cref="ReSettings.EnableExceptionAsErrorMessage" /> is false, the exception is
    ///     thrown.
    /// </exception>
    internal static void HandleRuleException(Exception ex, Rule rule, ReSettings reSettings)
    {
        ex.Data.Add(nameof(rule.RuleName), rule.RuleName);
        ex.Data.Add(nameof(rule.Expression), rule.Expression);

        if (!reSettings.EnableExceptionAsErrorMessage)
        {
            throw ex;
        }
    }

    /// <summary>
    ///     Gets the exception message
    /// </summary>
    /// <param name="message">The message to check if it should be returned or not.</param>
    /// <param name="reSettings">The <see cref="ReSettings" /> to check if the exception should be ignored or not.</param>
    /// <returns>The exception message if the <see cref="ReSettings.IgnoreException" /> is false, else empty string.</returns>
    internal static string TryGetExceptionMessage(string message, ReSettings reSettings)
    {
        return reSettings.IgnoreException ? "" : message;
    }
}
