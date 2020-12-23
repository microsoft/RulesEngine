// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Enums;
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

        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RuleExpressionType? RuleExpressionType { get; set; }

        public List<string> WorkflowRulesToInject { get; set; }

        public List<Rule> Rules { get; set; }

        [JsonProperty]
        public IEnumerable<LocalParam> LocalParams { get; set; }
        public string Expression { get; set; }

        public Dictionary<ActionTriggerType, ActionInfo> Actions { get; set; }
        public string SuccessEvent { get; set; }

    }
}
