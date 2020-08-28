using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.Models
{
    /// <summary>
    /// CompiledParam class.
    /// </summary>
    internal class CompiledParam
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        internal string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        internal Delegate Value { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        internal IEnumerable<RuleParameter> Parameters { get; set; }
    }
}
