// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Interfaces;
using RulesEngine.Models;
using System.Collections.Generic;

namespace RulesEngine.Extensions;

public static class ListofRuleResultTreeExtension
{
    public delegate void OnFailureFunc();

    public delegate void OnSuccessFunc(string eventName);


    /// <summary>
    ///     Calls the Success Func for the first rule which succeeded among the ruleResults
    /// </summary>
    /// <param name="ruleResultTrees">The <see cref="RuleResultTree" /> of the <see cref="IRule" /> which was running</param>
    /// <param name="onSuccessFunc">The function to be called on success</param>
    /// <returns></returns>
    public static List<RuleResultTree> OnSuccess(this List<RuleResultTree> ruleResultTrees, OnSuccessFunc onSuccessFunc)
    {
        var successfulRuleResult = ruleResultTrees.Find(ruleResult => ruleResult.IsSuccess);
        if (successfulRuleResult is null)
        {
            return ruleResultTrees;
        }

        var eventName = successfulRuleResult.Rule.SuccessEvent ?? successfulRuleResult.Rule.RuleName;
        onSuccessFunc(eventName);

        return ruleResultTrees;
    }

    /// <summary>
    ///     Calls the Failure Func if all rules failed in the ruleReults
    /// </summary>
    /// <param name="ruleResultTrees"></param>
    /// <param name="onFailureFunc"></param>
    /// <returns></returns>
    public static List<RuleResultTree> OnFail(this List<RuleResultTree> ruleResultTrees, OnFailureFunc onFailureFunc)
    {
        var allFailure = ruleResultTrees.TrueForAll(ruleResult => !ruleResult.IsSuccess);
        if (allFailure)
        {
            onFailureFunc();
        }

        return ruleResultTrees;
    }
}
