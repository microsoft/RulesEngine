// Copyright (c) Microsoft Corporation and others.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;

namespace RulesEngine.Interfaces;

/// <summary>
///     A Rule is a condition that can be evaluated to true or false.
///     If the Rule evaluates to true, the corresponding actions are executed.
///     A Rule can contain other Rules, which are evaluated based on the Operator.
///     There should be Rules and Leaf Rules.
///     Rules can contain other Rules and Leaf Rules.
///     Leaf Rules are the actual conditions that are evaluated.
///     (Interpreted from the Schema)
/// </summary>
public interface IRule
{
    /// <summary>
    ///     Rule name for the Rule, should be unique within the workflow.
    /// </summary>
    string RuleName { get; set; }

    /// <summary>
    ///     Gets or sets the custom property or tags of the rule.
    /// </summary>
    /// <value>
    ///     The properties of the rule.
    /// </value>
    // ToDo: Check if this is even in use anymore
    Dictionary<string, object> Properties { get; set; }

    /// <summary>
    ///     The Operator to be used to combine the <see cref="GetNestedRules" />
    ///     Currently either "And"/"AndAlso" or "Or"/"OrElse"
    /// </summary>
    string Operator { get; set; }

    /// <summary>
    ///     Gets or sets the error message, which will be set if the rule fails.
    /// </summary>
    string ErrorMessage { get; set; }

    /// <summary>
    ///     Flag to tell if the rule is enabled or not and should be evaluated.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    ///     The <see cref="RuleExpressionType" />
    /// </summary>
    RuleExpressionType RuleExpressionType { get; set; }

    /// <summary>
    ///     The workflows to inject when the rule is executed.
    /// </summary>
    IEnumerable<string> WorkflowsToInject { get; set; }

    /// <summary>
    ///     The parameters scoped to the Rule.
    /// </summary>
    IEnumerable<ScopedParam> LocalParams { get; set; }

    /// <summary>
    ///     The expression to be evaluated.
    /// </summary>
    string Expression { get; set; }

    /// <summary>
    ///     The <see cref="RuleActions" /> to be executed when the Rule was evaluated.
    ///     Either OnSuccess or OnFailure will be executed based on the evaluation of the Rule.
    /// </summary>
    RuleActions Actions { get; set; }

    /// <summary>
    ///     The event to be given to the OnSuccess <see cref="RuleActions" />
    ///     or the respecting default behaviour if no <see cref="RuleActions" /> are provided.
    /// </summary>
    string SuccessEvent { get; set; }

    /// <summary>
    ///     Get Nested Rules or Leaf Rules.
    /// </summary>
    IEnumerable<IRule> GetNestedRules();

    /// <summary>
    ///     Set Nested Rules or Leaf Rules.
    /// </summary>
    /// <param name="rules">The rules to execute.</param>
    void SetRules(IEnumerable<IRule> rules);
}
