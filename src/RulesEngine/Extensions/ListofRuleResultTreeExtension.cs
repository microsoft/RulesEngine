// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Linq;


namespace RulesEngine.Extensions
{
    public static class ListofRuleResultTreeExtension
    {
        public delegate void OnSuccessFunc(string eventName);
        public delegate void OnFailureFunc();


        /// <summary>
        /// Calls the Success Func for the first rule which succeeded among the ruleResults
        /// </summary>
        /// <param name="ruleResultTrees"></param>
        /// <param name="onSuccessFunc"></param>
        /// <returns></returns>
        public static List<RuleResultTree> OnSuccess(this List<RuleResultTree> ruleResultTrees, OnSuccessFunc onSuccessFunc)
        {
            var successfulRuleResult = ruleResultTrees.FirstOrDefault(ruleResult => ruleResult.IsSuccess == true);
            if (successfulRuleResult != null)
            {
                var eventName = successfulRuleResult.Rule.SuccessEvent ?? successfulRuleResult.Rule.RuleName;
                onSuccessFunc(eventName);
            }

            return ruleResultTrees;
        }

        /// <summary>
        /// Calls the Failure Func if all rules failed in the ruleReults
        /// </summary>
        /// <param name="ruleResultTrees"></param>
        /// <param name="onSuccessFunc"></param>
        /// <returns></returns>
        public static List<RuleResultTree> OnFail(this List<RuleResultTree> ruleResultTrees, OnFailureFunc onFailureFunc)
        {
            bool allFailure = ruleResultTrees.All(ruleResult => ruleResult.IsSuccess == false);
            if (allFailure)
                onFailureFunc();
            return ruleResultTrees;
        }
    }
}
