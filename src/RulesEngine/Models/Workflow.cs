// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    [Obsolete("WorkflowRules class is deprecated. Use Workflow class instead.")]
    [ExcludeFromCodeCoverage]
    public class WorkflowRules : Workflow {
    }

    /// <summary>
    /// Workflow rules class for deserialization  the json config file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Workflow
    {
        /// <summary>
        /// Gets the workflow name.
        /// </summary>
        public string WorkflowName { get; set; }

    /// <summary>Gets or sets the workflow rules to inject.</summary>
    /// <value>The workflow rules to inject.</value>
    [Obsolete("WorkflowRulesToInject is deprecated. Use WorkflowsToInject instead.")]
    public IEnumerable<string> WorkflowRulesToInject {
      get { return WorkflowsToInject; }
      set { WorkflowsToInject = value; }
    }
    public IEnumerable<string> WorkflowsToInject { get; set; }

        /// <summary>
        /// Gets or Sets the global params which will be applicable to all rules
        /// </summary>
        public IEnumerable<ScopedParam> GlobalParams { get; set; }

        /// <summary>
        /// list of rules.
        /// </summary>
        public IEnumerable<Rule> Rules { get; set; }
    }
}
