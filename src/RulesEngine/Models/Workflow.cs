// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models;

/// <summary>
///     Outdated class. Use <see cref="Workflow" /> class instead.
/// </summary>
[Obsolete("WorkflowRules class is deprecated. Use Workflow class instead.")]
[ExcludeFromCodeCoverage]
public class WorkflowRules : Workflow
{
}

/// <summary>
///     The workflow for the rules engine to execute.
/// </summary>
[ExcludeFromCodeCoverage]
public class Workflow
{
    /// <summary>
    ///     Outdated property. Use <see cref="WorkflowsToInject" /> instead.
    /// </summary>
    [Obsolete("WorkflowRulesToInject is deprecated. Use WorkflowsToInject instead.")]
    public IEnumerable<string> WorkflowRulesToInject {
        set => WorkflowsToInject = value;
    }

    /// <summary>
    ///     List of rules to execute.
    /// </summary>
    public IEnumerable<Rule> Rules { get; set; }

    /// <summary>
    ///     The name of the workflow, should be unique within the rules engine.
    /// </summary>
    public string WorkflowName { get; set; }

    /// <summary>
    ///     Gets or sets the workflows to inject when the <see cref="Workflow" /> is executed.
    /// </summary>
    public IEnumerable<string> WorkflowsToInject { get; set; }

    /// <summary>
    ///     The <see cref="RuleExpressionType" />
    /// </summary>
    public RuleExpressionType RuleExpressionType { get; set; } = RuleExpressionType.LambdaExpression;

    /// <summary>
    ///     Gets or Sets the global params which will be applicable to all rules
    /// </summary>
    public IEnumerable<ScopedParam> GlobalParams { get; set; }
}