// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RulesEngine.Models;

/// <inheritdoc />
[Obsolete("WorkflowRules class is deprecated. Use Workflow class instead.")]
[ExcludeFromCodeCoverage]
public class WorkflowRules : Workflow
{
}

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class Workflow : IWorkflow
{
    /// <inheritdoc />
    [Obsolete("WorkflowRulesToInject is deprecated. Use WorkflowsToInject instead.")]
    public IEnumerable<string> WorkflowRulesToInject {
        set => WorkflowsToInject = value;
    }

    /// <summary>
    ///     List of rules to execute.
    /// </summary>
    public IEnumerable<Rule> Rules { get; set; }

    /// <inheritdoc />
    public string WorkflowName { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> WorkflowsToInject { get; set; }

    /// <inheritdoc />
    public RuleExpressionType RuleExpressionType { get; set; } = RuleExpressionType.LambdaExpression;

    /// <inheritdoc />
    public IEnumerable<ScopedParam> GlobalParams { get; set; }

    /// <inheritdoc />
    public IEnumerable<IRule> GetRules()
    {
        return Rules;
    }

    /// <inheritdoc />
    public void SetRules(IEnumerable<IRule> rules)
    {
        Rules = rules.OfType<Rule>().ToArray();
    }
}