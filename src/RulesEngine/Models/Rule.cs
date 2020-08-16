// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RulesEngine.Models
{
    /// <summary>
    /// Rule class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Rule
    {
        public string RuleName { get; set; }
        public string Operator { get; set; }
        public string ErrorMessage { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RuleExpressionType? RuleExpressionType { get; set; }

        public List<string> WorkflowRulesToInject { get; set; }

        public List<Rule> Rules { get; set; }

        [JsonProperty]
        public IEnumerable<LocalParam> LocalParams { get; private set; }
        public string Expression { get; set; }
        public string SuccessEvent { get; set; }

    }
}
