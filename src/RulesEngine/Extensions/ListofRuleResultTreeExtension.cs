// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine.Extensions;

public static class ListofRuleResultTreeExtension
{
    public delegate void OnFailureFunc();

    public delegate void OnSuccessFunc(string eventName);


    /// <summary>
    ///     Calls the Success Func for the first rule which succeeded among the ruleResults
    /// </summary>
    /// <param name="ruleResultTrees"></param>
    /// <param name="onSuccessFunc"></param>
    /// <returns></returns>
    public static List<RuleResultTree> OnSuccess(this List<RuleResultTree> ruleResultTrees, OnSuccessFunc onSuccessFunc)
    {
        var successfulRuleResult = ruleResultTrees.FirstOrDefault(ruleResult => ruleResult.IsSuccess);
        if (successfulRuleResult != null)
        {
            var eventName = successfulRuleResult.Rule.SuccessEvent ?? successfulRuleResult.Rule.RuleName;
            onSuccessFunc(eventName);
        }

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
        var allFailure = ruleResultTrees.All(ruleResult => ruleResult.IsSuccess == false);
        if (allFailure)
        {
            onFailureFunc();
        }

        return ruleResultTrees;
    }
}