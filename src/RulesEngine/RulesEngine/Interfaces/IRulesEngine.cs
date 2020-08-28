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
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="inputs">A variable number of inputs</param>
        /// <returns>List of rule results</returns>
        List<RuleResultTree> ExecuteRule(string workflowName, params object[] inputs);

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="ruleParams">A variable number of rule parameters</param>
        /// <returns>List of rule results</returns>
        List<RuleResultTree> ExecuteRule(string workflowName, params RuleParameter[] ruleParams);
    }
}
