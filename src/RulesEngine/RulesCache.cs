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
        private ConcurrentDictionary<string, (Workflow, Int64)> _workflow = new ConcurrentDictionary<string, (Workflow, Int64)>();

        /// <summary>Determines whether [contains workflow rules] [the specified workflow name].</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <returns>
        ///   <c>true</c> if [contains workflow rules] [the specified workflow name]; otherwise, <c>false</c>.</returns>
        public bool ContainsWorkflows(string workflowName)
        {
            return _workflow.ContainsKey(workflowName);
        }

        public List<string> GetAllWorkflowNames()
        {
            return _workflow.Keys.ToList();
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
        public void AddOrUpdateWorkflows(string workflowName, Workflow rules)
        {
            Int64 ticks = DateTime.UtcNow.Ticks;
            _workflow.AddOrUpdate(workflowName, (rules, ticks), (k, v) => (rules, ticks));
        }

        /// <summary>Adds the or update compiled rule.</summary>
        /// <param name="compiledRuleKey">The compiled rule key.</param>
        /// <param name="compiledRule">The compiled rule.</param>
        public void AddOrUpdateCompiledRule(string compiledRuleKey, IDictionary<string, RuleFunc<RuleResultTree>> compiledRule)
        {
            Int64 ticks = DateTime.UtcNow.Ticks;
            _compileRules.AddOrUpdate(compiledRuleKey, (compiledRule, ticks), (k, v) => (compiledRule, ticks));
        }

        /// <summary>Checks if the compiled rules are up-to-date.</summary>
        /// <param name="compiledRuleKey">The compiled rule key.</param>
        /// <param name="workflowName">The workflow name.</param>
         /// <returns>
        ///   <c>true</c> if [compiled rules] is newer than the [workflow rules]; otherwise, <c>false</c>.</returns>
        public bool AreCompiledRulesUpToDate(string compiledRuleKey, string workflowName)
        {
            if (_compileRules.TryGetValue(compiledRuleKey, out (IDictionary<string, RuleFunc<RuleResultTree>> rules, Int64 tick) compiledRulesObj))
            {
                if (_workflow.TryGetValue(workflowName, out (Workflow rules, Int64 tick) WorkflowsObj))
                {
                    return compiledRulesObj.tick >= WorkflowsObj.tick;
                }
            }

            return false;
        }

        /// <summary>Clears this instance.</summary>
        public void Clear()
        {
            _workflow.Clear();
            _compileRules.Clear();
        }

        /// <summary>Gets the work flow rules.</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <returns>Workflows.</returns>
        /// <exception cref="Exception">Could not find injected Workflow: {wfname}</exception>
        public Workflow GetWorkflow(string workflowName)
        {
            if (_workflow.TryGetValue(workflowName, out (Workflow rules, Int64 tick) WorkflowsObj))
            {
                var workflow = WorkflowsObj.rules;
                if (workflow.WorkflowsToInject?.Any() == true)
                {
                    if (workflow.Rules == null)
                    {
                        workflow.Rules = new List<Rule>();
                    }
                    foreach (string wfname in workflow.WorkflowsToInject)
                    {
                        var injectedWorkflow = GetWorkflow(wfname);
                        if (injectedWorkflow == null)
                        {
                            throw new Exception($"Could not find injected Workflow: {wfname}");
                        }

                        workflow.Rules.ToList().AddRange(injectedWorkflow.Rules);
                    }
                }

                return workflow;
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
            if (_workflow.TryRemove(workflowName, out (Workflow, Int64) workflowObj))
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
