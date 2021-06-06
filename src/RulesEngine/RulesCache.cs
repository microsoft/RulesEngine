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
        private ConcurrentDictionary<string, (IDictionary<string, RuleFunc<RuleResultTree>>, Int64)> _compileRules = new ConcurrentDictionary<string,  (IDictionary<string, RuleFunc<RuleResultTree>>, Int64)>();

        /// <summary>The workflow rules</summary>
        private ConcurrentDictionary<string, (WorkflowRules, Int64)> _workflowRules = new ConcurrentDictionary<string, (WorkflowRules, Int64)>();

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
            Int64 ticks = DateTime.UtcNow.Ticks;
            _workflowRules.AddOrUpdate(workflowName, (rules, ticks), (k, v) => (rules, ticks));
        }

        /// <summary>Adds the or update compiled rule.</summary>
        /// <param name="compiledRuleKey">The compiled rule key.</param>
        /// <param name="compiledRule">The compiled rule.</param>
        public void AddOrUpdateCompiledRule(string compiledRuleKey, IDictionary<string, RuleFunc<RuleResultTree>> compiledRule)
        {
            Int64 ticks = DateTime.UtcNow.Ticks;
            _compileRules.AddOrUpdate(compiledRuleKey, (compiledRule, ticks), (k, v) => (compiledRule, ticks));
        }

        /// <summary>Checks if the compiled rules is up-to-date.</summary>
        /// <param name="compiledRuleKey">The compiled rule key.</param>
        /// <param name="workflowName">The workflow name.</param>
         /// <returns>
        ///   <c>true</c> if [compiled rules] is newer than the [workflow rules]; otherwise, <c>false</c>.</returns>
        public bool IsCompiledRulesUpToDate(string compiledRuleKey, string workflowName)
        {
            if (_compileRules.TryGetValue(compiledRuleKey, out (IDictionary<string, RuleFunc<RuleResultTree>> rules, Int64 tick) compiledRulesObj))
            {
                if (_workflowRules.TryGetValue(workflowName, out (WorkflowRules rules, Int64 tick) workflowRulesObj))
                {
                    return compiledRulesObj.tick >= workflowRulesObj.tick;
                }
            }

            return false;
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
            if (_workflowRules.TryGetValue(workflowName, out (WorkflowRules rules, Int64 tick) workflowRulesObj))
            {
                var workflowRules = workflowRulesObj.rules;
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
            else
            {
                return null;
            }
        }


        /// <summary>Gets the compiled rules.</summary>
        /// <param name="compiledRulesKey">The compiled rules key.</param>
        /// <returns>CompiledRule.</returns>
        public IDictionary<string, RuleFunc<RuleResultTree>> GetCompiledRules(string compiledRulesKey)
        {
            return _compileRules[compiledRulesKey].Item1;
        }

        /// <summary>Removes the specified workflow name.</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        public void Remove(string workflowName)
        {
            if (_workflowRules.TryRemove(workflowName, out (WorkflowRules, Int64) workflowObj))
            {
                var compiledKeysToRemove = _compileRules.Keys.Where(key => key.StartsWith(workflowName));
                foreach (var key in compiledKeysToRemove)
                {
                    _compileRules.TryRemove(key, out (IDictionary<string, RuleFunc<RuleResultTree>>, Int64) val);
                }
            }
        }
    }
}
