// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Interfaces;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RulesEngine.Models;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class Rule : IRule
{
    /// <summary>
    ///     Gets or sets the rules.
    /// </summary>
    public IEnumerable<Rule> Rules { get; set; }

    /// <inheritdoc />
    public string RuleName { get; set; }

    /// <inheritdoc />
    public Dictionary<string, object> Properties { get; set; }

    /// <inheritdoc />
    public string Operator { get; set; }

    /// <inheritdoc />
    public string ErrorMessage { get; set; }

    /// <inheritdoc />
    public bool Enabled { get; set; } = true;

    /// <inheritdoc />
    [JsonConverter(typeof(StringEnumConverter))]
    public RuleExpressionType RuleExpressionType { get; set; } = RuleExpressionType.LambdaExpression;

    /// <inheritdoc />
    public IEnumerable<string> WorkflowsToInject { get; set; }

    /// <summary>
    ///     Gets or sets the nested rules.
    /// </summary>
    public IEnumerable<IRule> GetNestedRules()
    {
        return Rules;
    }

    /// <inheritdoc />
    public void SetRules(IEnumerable<IRule> rules)
    {
        Rules = rules.OfType<Rule>().ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<ScopedParam> LocalParams { get; set; }

    /// <inheritdoc />
    public string Expression { get; set; }

    /// <inheritdoc />
    public RuleActions Actions { get; set; }

    /// <inheritdoc />
    public string SuccessEvent { get; set; }
}
