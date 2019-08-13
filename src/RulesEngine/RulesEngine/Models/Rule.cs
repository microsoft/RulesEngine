// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace RulesEngine.Models
{
    /// <summary>
    /// Rule class
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        /// <value>
        /// The name of the rule.
        /// </value>
        public string RuleName { get; set; }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the error.
        /// </summary>
        /// <value>
        /// The type of the error.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the type of the rule expression.
        /// </summary>
        /// <value>
        /// The type of the rule expression.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleExpressionType? RuleExpressionType { get; set; }


        /// <summary>
        /// Gets or sets the names of common workflows
        /// </summary>
        public List<string> WorkflowRulesToInject { get; set; }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// Gets or Sets the lambda expression. 
        /// </summary>
        public string Expression { get; set; }


        public string SuccessEvent { get; set; }

    }

}
