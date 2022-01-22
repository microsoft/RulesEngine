// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>
    /// Workflow rules class for deserialization  the json config file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Workflow
    {
        /// <summary>
        /// default contructor
        /// </summary>
        public Workflow()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Id is Primary Key in Database
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Get/Set the workflow name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>Gets or sets the workflow rules to inject.</summary>
        /// <value>The workflow rules to inject.</value>
        [NotMapped]
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
