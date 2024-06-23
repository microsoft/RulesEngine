// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models;

/// <summary>
///     A Rule is a condition that can be evaluated to true or false.
///     If the Rule evaluates to true, the corresponding actions are executed.
///     A Rule can contain other Rules, which are evaluated based on the Operator.
///     There should be Rules and Leaf Rules.
///     Rules can contain other Rules and Leaf Rules.
///     Leaf Rules are the actual conditions that are evaluated.
///     (Interpreted from the Schema)
/// </summary>
[ExcludeFromCodeCoverage]
public class Rule
{
    /// <summary>
    ///     Gets or sets the nested rules.
    /// </summary>
    public IEnumerable<Rule> Rules { get; set; }

    /// <summary>
    ///     Rule name for the Rule, should be unique within the workflow.
    /// </summary>
    public string RuleName { get; set; }

    /// <summary>
    ///     Gets or sets the custom property or tags of the rule.
    /// </summary>
    /// <value>
    ///     The properties of the rule.
    /// </value>
    // ToDo: Check if this is even in use anymore
    public Dictionary<string, object> Properties { get; set; }

    /// <summary>
    ///     The Operator to be used to combine the <see cref="Rules" />
    ///     Currently either "And"/"AndAlso" or "Or"/"OrElse"
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    ///     Gets or sets the error message, which will be set if the rule fails.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     Flag to tell if the rule is enabled or not and should be evaluated.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     The <see cref="RuleExpressionType" />
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public RuleExpressionType RuleExpressionType { get; set; } = RuleExpressionType.LambdaExpression;

    /// <summary>
    ///     The workflows to inject when the rule is executed.
    /// </summary>
    public IEnumerable<string> WorkflowsToInject { get; set; }

    /// <summary>
    ///     The parameters scoped to the Rule.
    /// </summary>
    public IEnumerable<ScopedParam> LocalParams { get; set; }

    /// <summary>
    ///     The expression to be evaluated.
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    ///     The <see cref="RuleActions" /> to be executed when the Rule was evaluated.
    ///     Either OnSuccess or OnFailure will be executed based on the evaluation of the Rule.
    /// </summary>
    public RuleActions Actions { get; set; }

    /// <summary>
    ///     The event to be given to the OnSuccess <see cref="RuleActions" />
    ///     or the respecting default behaviour if no <see cref="RuleActions" /> are provided.
    /// </summary>
    public string SuccessEvent { get; set; }
}