// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace RulesEngine.Models
{
    /// <summary>
    /// Workflow rules class for deserialization  the json config file
    /// </summary>
    public class WorkflowRules
    {
        /// <summary>
        /// Gets the workflow name.
        /// </summary>
        public string WorkflowName { get; set; }

        public List<string> WorkflowRulesToInject { get; set; }
        /// <summary>
        /// list of rules.
        /// </summary>
        public List<Rule> Rules { get; set; }
    }
}
