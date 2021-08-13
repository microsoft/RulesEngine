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

        /// <summary>
        /// Gets the input alias. (Optional - defaults to Input1, Input2, etc.)
        /// </summary>
        /// <remarks>
        /// When set, passing 1 input parameter, uses exact input alias (e.g. MyAlias.InputProperty1)
        /// When passing 2 or more input parameters, uses input alias plus increment (e.g. MyAlias1.InputProperty1, MyAlias2.InputProperty1)
        /// </remarks>

        public string InputAlias { get; set; }

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
