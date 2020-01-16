// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine
{
    internal class RulesCache
    {
        private ConcurrentDictionary<string, CompiledRule> _compileRules = new ConcurrentDictionary<string, CompiledRule>();
        private ConcurrentDictionary<string, WorkflowRules> _workflowRules = new ConcurrentDictionary<string, WorkflowRules>();

        public bool ContainsWorkflowRules(string workflowName)
        {
            return _workflowRules.ContainsKey(workflowName);
        }

        public bool ContainsCompiledRules(string workflowName)
        {
            return _compileRules.ContainsKey(workflowName);
        }

        public void AddOrUpdateWorkflowRules(string workflowName, WorkflowRules rules)
        {
            _workflowRules.AddOrUpdate(workflowName, rules, (k, v) => rules);
        }

        public void AddOrUpdateCompiledRule(string compiledRuleKey, CompiledRule compiledRule)
        {
            _compileRules.AddOrUpdate(compiledRuleKey, compiledRule, (k, v) => compiledRule);
        }

        public void Clear()
        {
            _workflowRules.Clear();
            _compileRules.Clear();
        }

        public IEnumerable<Rule> GetRules(string workflowName)
        {
            return _workflowRules[workflowName].Rules;
        }

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
                        workflowRules.Rules.AddRange(injectedWorkflow.Rules);
                    }
                }

                return workflowRules;
            }
        }

        public CompiledRule GetCompiledRules(string compiledRulesKey)
        {
            return _compileRules[compiledRulesKey];
        }

        public void Remove(string workflowName)
        {
            if (_workflowRules.TryRemove(workflowName, out WorkflowRules workflowObj))
            {
                var compiledKeysToRemove = _compileRules.Keys.Where(key => key.StartsWith(workflowName));
                foreach (var key in compiledKeysToRemove)
                {
                    _compileRules.TryRemove(key, out CompiledRule val);
                }
            }
        }
    }
}
