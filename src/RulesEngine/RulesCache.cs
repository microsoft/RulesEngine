// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine;

/// <summary>Class RulesCache.</summary>
internal class RulesCache
{
    /// <summary>The compile rules</summary>
    private readonly MemCache _compileRules;

    /// <summary>The workflow rules</summary>
    private readonly ConcurrentDictionary<string, (IWorkflow, long)> _workflow = new();


    public RulesCache(ReSettings reSettings)
    {
        _compileRules = new MemCache(reSettings.CacheConfig);
    }


    /// <summary>Determines whether [contains workflow rules] [the specified workflow name].</summary>
    /// <param name="workflowName">Name of the workflow.</param>
    /// <returns>
    ///     <c>true</c> if [contains workflow rules] [the specified workflow name]; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsWorkflows(string workflowName)
    {
        return _workflow.ContainsKey(workflowName);
    }

    public List<string> GetAllWorkflowNames()
    {
        return _workflow.Keys.ToList();
    }

    /// <summary>Adds the or update workflow rules.</summary>
    /// <param name="workflowName">Name of the workflow.</param>
    /// <param name="rules">The rules.</param>
    public void AddOrUpdateWorkflows(string workflowName, IWorkflow rules)
    {
        var ticks = DateTime.UtcNow.Ticks;
        _workflow.AddOrUpdate(workflowName, (rules, ticks), (_, _) => (rules, ticks));
    }

    /// <summary>Adds or update compiled rule.</summary>
    /// <param name="compiledRuleKey">The compiled rule key.</param>
    /// <param name="compiledRule">The compiled rule.</param>
    public void AddOrUpdateCompiledRule(string compiledRuleKey,
        IDictionary<string, RuleFunc<RuleResultTree>> compiledRule)
    {
        var ticks = DateTime.UtcNow.Ticks;
        _compileRules.Set(compiledRuleKey, (compiledRule, ticks));
    }

    /// <summary>Checks if the compiled rules are up-to-date.</summary>
    /// <param name="compiledRuleKey">The compiled rule key.</param>
    /// <param name="workflowName">The workflow name.</param>
    /// <returns>
    ///     <c>true</c> if [compiled rules] is newer than the [workflow rules]; otherwise, <c>false</c>.
    /// </returns>
    public bool AreCompiledRulesUpToDate(string compiledRuleKey, string workflowName)
    {
        if (_compileRules.TryGetValue(compiledRuleKey,
                out (IDictionary<string, RuleFunc<RuleResultTree>> rules, long tick) compiledRulesObj) &&
            _workflow.TryGetValue(workflowName, out (IWorkflow rules, long tick) workflowsObj))
        {
            return compiledRulesObj.tick >= workflowsObj.tick;
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
    public IWorkflow GetWorkflow(string workflowName)
    {
        if (!_workflow.TryGetValue(workflowName, out (IWorkflow rules, long tick) workflowsObj))
        {
            return null;
        }

        var workflow = workflowsObj.rules;
        if (workflow.WorkflowsToInject?.Any() != true)
        {
            return workflow;
        }

        if (workflow.GetRules() == null)
        {
            workflow.SetRules(new List<IRule>());
        }

        foreach (var wfname in workflow.WorkflowsToInject)
        {
            var injectedWorkflow = GetWorkflow(wfname);
            if (injectedWorkflow == null)
            {
                throw new Exception($"Could not find injected Workflow: {wfname}");
            }

            workflow.SetRules(workflow.GetRules().Concat(injectedWorkflow.GetRules()).ToList());
        }

        return workflow;
    }


    /// <summary>Gets the compiled rules.</summary>
    /// <param name="compiledRulesKey">The compiled rules key.</param>
    /// <returns>CompiledRule.</returns>
    public IDictionary<string, RuleFunc<RuleResultTree>> GetCompiledRules(string compiledRulesKey)
    {
        return _compileRules.Get<(IDictionary<string, RuleFunc<RuleResultTree>> rules, long tick)>(compiledRulesKey)
            .rules;
    }

    /// <summary>Removes the specified workflow name.</summary>
    /// <param name="workflowName">Name of the workflow.</param>
    public void Remove(string workflowName)
    {
        if (!_workflow.TryRemove(workflowName, out _))
        {
            return;
        }

        var compiledKeysToRemove = _compileRules.GetKeys().Where(key => key.StartsWith(workflowName));
        foreach (var key in compiledKeysToRemove)
        {
            _compileRules.Remove(key);
        }
    }
}