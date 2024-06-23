using RulesEngine.Models;
using System.Collections.Generic;

namespace RulesEngine.Interfaces;

/// <summary>
///     The workflow for the rules engine to execute.
/// </summary>
public interface IWorkflow
{
    /// <summary>
    ///     The name of the workflow, should be unique within the rules engine.
    /// </summary>
    string WorkflowName { get; set; }

    /// <summary>
    ///     Gets or sets the workflows to inject when the <see cref="IWorkflow" /> is executed.
    /// </summary>
    IEnumerable<string> WorkflowsToInject { get; set; }

    /// <summary>
    ///     The <see cref="RuleExpressionType" />
    /// </summary>
    RuleExpressionType RuleExpressionType { get; set; }

    /// <summary>
    ///     Gets or Sets the global params which will be applicable to all rules
    /// </summary>
    IEnumerable<ScopedParam> GlobalParams { get; set; }

    /// <summary>
    ///     List of rules to execute.
    /// </summary>
    IEnumerable<IRule> GetRules();

    /// <summary>
    ///     List of rules to execute.
    /// </summary>
    /// <param name="rules">The rules to execute.</param>
    void SetRules(IEnumerable<IRule> rules);
}
