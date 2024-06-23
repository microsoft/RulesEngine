// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RulesEngine.Interfaces;

public interface IRulesEngine
{
    /// <summary>
    ///     This will execute all the rules of the specified workflow
    /// </summary>
    /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
    /// <param name="inputs">A variable number of inputs</param>
    /// <returns>List of rule results</returns>
    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params object[] inputs);

    /// <summary>
    ///     This will execute all the rules of the specified workflow
    /// </summary>
    /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
    /// <param name="ruleParams">A variable number of rule parameters</param>
    /// <returns>List of rule results</returns>
    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params RuleParameter[] ruleParams);

    ValueTask<ActionRuleResult> ExecuteActionWorkflowAsync(string workflowName, string ruleName,
        RuleParameter[] ruleParameters);

    /// <summary>
    ///     Adds new workflows to RulesEngine
    /// </summary>
    /// <param name="workflows">The workflows to add</param>
    void AddWorkflow(params IWorkflow[] workflows);

    /// <summary>
    ///     Adds new workflows to RulesEngine
    /// </summary>
    /// <param name="workflows">The workflows to add</param>
    [Obsolete("Use AddWorkflow(params IWorkflow[] workflows) instead")]
    void AddWorkflow(params Workflow[] workflows);

    /// <summary>
    ///     Removes all registered workflows from RulesEngine
    /// </summary>
    void ClearWorkflows();

    /// <summary>
    ///     Removes the workflow from RulesEngine
    /// </summary>
    /// <param name="workflowNames"></param>
    void RemoveWorkflow(params string[] workflowNames);

    /// <summary>
    ///     Checks is workflow exist.
    /// </summary>
    /// <param name="workflowName">The workflow name.</param>
    /// <returns> <c>true</c> if contains the specified workflow name; otherwise, <c>false</c>.</returns>
    bool ContainsWorkflow(string workflowName);

    /// <summary>
    ///     Returns the list of all registered workflow names
    /// </summary>
    /// <returns></returns>
    List<string> GetAllRegisteredWorkflowNames();

    /// <summary>
    ///     Adds or updates the workflow in the RulesEngine
    /// </summary>
    /// <param name="workflows">The workflows to add or update</param>
    void AddOrUpdateWorkflow(params IWorkflow[] workflows);

    /// <summary>
    ///     Adds or updates the workflow in the RulesEngine
    /// </summary>
    /// <param name="workflows">The workflows to add or update</param>
    [Obsolete("Use AddWorkflow(params IWorkflow[] workflows) instead")]
    void AddOrUpdateWorkflow(params Workflow[] workflows);
}
