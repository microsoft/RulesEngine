// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RulesEngine.Interfaces
{
    public interface IRulesEngine
    {
        IAsyncEnumerable<List<RuleResultTree>> ExecuteAllWorkflows(RuleParameter[] inputs, CancellationToken ct = default);
        Task<List<RuleResultTree>> ExecuteWorkflow(string workflow_name, RuleParameter[] inputs, CancellationToken ct = default);
        Task<RuleResultTree> ExecuteRule(string workflow_name, string rule_name, RuleParameter[] ruleParams, CancellationToken ct = default);
        Task<ActionRuleResult> ExecuteRuleActions(string workflow_name, string rule_name, RuleParameter[] inputs, CancellationToken ct = default);

        #region Obsolete Methods

        [Obsolete]
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params object[] inputs);
        [Obsolete]
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, object[] inputs, CancellationToken ct = default);
        [Obsolete]
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params RuleParameter[] ruleParams);
        [Obsolete]
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, RuleParameter[] ruleParams, CancellationToken ct = default);
        [Obsolete]
        ValueTask<ActionRuleResult> ExecuteActionWorkflowAsync(string workflowName, string ruleName, RuleParameter[] ruleParameters);
        [Obsolete]
        ValueTask<ActionRuleResult> ExecuteActionWorkflowAsync(string workflowName, string ruleName, RuleParameter[] ruleParameters, CancellationToken ct = default);

        #endregion

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
