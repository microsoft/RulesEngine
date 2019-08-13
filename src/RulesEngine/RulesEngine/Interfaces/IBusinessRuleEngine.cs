// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;

namespace RulesEngine.Interfaces
{
    public interface IRulesEngine
    { 
        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="input"></param>
        /// <param name="otherInputs"></param>
        /// <returns>List of Result</returns>
        List<RuleResultTree> ExecuteRule(string workflowName, IEnumerable<dynamic> input, object[] otherInputs);


        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="inputs"></param>
        /// <returns>List of Result</returns>
        List<RuleResultTree> ExecuteRule(string workflowName, object[] inputs);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        List<RuleResultTree> ExecuteRule(string workflowName, object input);


        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="ruleParams"></param>
        /// <returns>List of Result</returns>
        List<RuleResultTree> ExecuteRule(string workflowName, RuleParameter[] ruleParams);
    }
}
