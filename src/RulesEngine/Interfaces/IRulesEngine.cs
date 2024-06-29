// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params object[] inputs);

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="ruleParams">A variable number of rule parameters</param>
        /// <returns>List of rule results</returns>
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params RuleParameter[] ruleParams);
        ValueTask<ActionRuleResult> ExecuteActionWorkflowAsync(string workflowName, string ruleName, RuleParameter[] ruleParameters);

        /// <summary>
        /// Adds new workflows to RulesEngine
        /// </summary>
        /// <param name="workflow"></param>
        void AddWorkflow(params Workflow[] Workflows);

        /// <summary>
        /// Removes all registered workflows from RulesEngine
        /// </summary>
        void ClearWorkflows();

        /// <summary>
        /// Removes the workflow from RulesEngine
        /// </summary>
        /// <param name="workflowNames"></param>
        void RemoveWorkflow(params string[] workflowNames);

        /// <summary>
        /// Checks is workflow exist.
        /// </summary>
        /// <param name="workflowName">The workflow name.</param>
        /// <returns> <c>true</c> if contains the specified workflow name; otherwise, <c>false</c>.</returns>
        bool ContainsWorkflow(string workflowName);

        /// <summary>
        /// Returns the list of all registered workflow names
        /// </summary>
        /// <returns></returns>
        List<string> GetAllRegisteredWorkflowNames();
        void AddOrUpdateWorkflow(params Workflow[] Workflows);
    }
}
