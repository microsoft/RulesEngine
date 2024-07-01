﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>
    /// Rule class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Rule
    {
        /// <summary>
        /// Rule name for the Rule
        /// </summary>
        public string RuleName { get; set; }
        /// <summary>	
        /// Gets or sets the custom property or tags of the rule.	
        /// </summary>	
        /// <value>	
        /// The properties of the rule.	
        /// </value>	
        public Dictionary<string, object> Properties { get; set; }
        public string Operator { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether the rule is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleExpressionType RuleExpressionType { get; set; } = RuleExpressionType.LambdaExpression;
        public IEnumerable<string> WorkflowsToInject { get; set; }
        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<ScopedParam> LocalParams { get; set; }
        public string Expression { get; set; }
        public RuleActions Actions { get; set; }
        public string SuccessEvent { get; set; }

    }
}
