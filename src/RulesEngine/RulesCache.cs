// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine
{
    /// <summary>Class RulesCache.</summary>
    internal class RulesCache
    {
        /// <summary>The compile rules</summary>
        private ConcurrentDictionary<string, IDictionary<string, RuleFunc<RuleResultTree>>> _compileRules = new ConcurrentDictionary<string, IDictionary<string, RuleFunc<RuleResultTree>>>();

        /// <summary>The workflow rules</summary>
        private ConcurrentDictionary<string, WorkflowRules> _workflowRules = new ConcurrentDictionary<string, WorkflowRules>();

        /// <summary>Determines whether [contains workflow rules] [the specified workflow name].</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <returns>
        ///   <c>true</c> if [contains workflow rules] [the specified workflow name]; otherwise, <c>false</c>.</returns>
        public bool ContainsWorkflowRules(string workflowName)
        {
            return _workflowRules.ContainsKey(workflowName);
        }

        public List<string> GetAllWorkflowNames()
        {
            return _workflowRules.Keys.ToList();
        }

        /// <summary>Determines whether [contains compiled rules] [the specified workflow name].</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <returns>
        ///   <c>true</c> if [contains compiled rules] [the specified workflow name]; otherwise, <c>false</c>.</returns>
        public bool ContainsCompiledRules(string workflowName)
        {
            return _compileRules.ContainsKey(workflowName);
        }

        /// <summary>Adds the or update workflow rules.</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="rules">The rules.</param>
        public void AddOrUpdateWorkflowRules(string workflowName, WorkflowRules rules)
        {
            _workflowRules.AddOrUpdate(workflowName, rules, (k, v) => rules);
        }

        /// <summary>Adds the or update compiled rule.</summary>
        /// <param name="compiledRuleKey">The compiled rule key.</param>
        /// <param name="compiledRule">The compiled rule.</param>
        public void AddOrUpdateCompiledRule(string compiledRuleKey, IDictionary<string, RuleFunc<RuleResultTree>> compiledRule)
        {
            _compileRules.AddOrUpdate(compiledRuleKey, compiledRule, (k, v) => compiledRule);
        }

        /// <summary>Clears this instance.</summary>
        public void Clear()
        {
            _workflowRules.Clear();
            _compileRules.Clear();
        }

        /// <summary>Gets the work flow rules.</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <returns>WorkflowRules.</returns>
        /// <exception cref="Exception">Could not find injected Workflow: {wfname}</exception>
        public WorkflowRules GetWorkFlowRules(string workflowName)
        {
            _workflowRules.TryGetValue(workflowName, out var workflowRules);
            if (workflowRules == null) return null;
            else
            {
                if (workflowRules.WorkflowRulesToInject?.Any() == true)
                {
                    if (workflowRules.Rules == null)
                    {
                        workflowRules.Rules = new List<Rule>();
                    }
                    foreach (string wfname in workflowRules.WorkflowRulesToInject)
                    {
                        var injectedWorkflow = GetWorkFlowRules(wfname);
                        if (injectedWorkflow == null)
                        {
                            throw new Exception($"Could not find injected Workflow: {wfname}");
                        }

                        workflowRules.Rules.ToList().AddRange(injectedWorkflow.Rules);
                    }
                }

                return workflowRules;
            }
        }


        /// <summary>Gets the compiled rules.</summary>
        /// <param name="compiledRulesKey">The compiled rules key.</param>
        /// <returns>CompiledRule.</returns>
        public IDictionary<string, RuleFunc<RuleResultTree>> GetCompiledRules(string compiledRulesKey)
        {
            return _compileRules[compiledRulesKey];
        }

        /// <summary>Removes the specified workflow name.</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        public void Remove(string workflowName)
        {
            if (_workflowRules.TryRemove(workflowName, out WorkflowRules workflowObj))
            {
                var compiledKeysToRemove = _compileRules.Keys.Where(key => key.StartsWith(workflowName));
                foreach (var key in compiledKeysToRemove)
                {
                    _compileRules.TryRemove(key, out IDictionary<string, RuleFunc<RuleResultTree>> val);
                }
            }
        }
    }
}
