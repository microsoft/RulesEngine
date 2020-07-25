using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.Models
{
    /// <summary>Class CompiledRule.</summary>
    internal class CompiledRuleParam
    {
        /// <summary>
        /// Gets or sets the compiled rules.
        /// </summary>
        /// <value>
        /// The compiled rules.
        /// </value>
        internal string Name { get; set; }

        /// <summary>Gets or sets the rule parameters.</summary>
        /// <value>The rule parameters.</value>
        internal IEnumerable<CompiledParam> CompiledParameters { get; set; }

        /// <summary>
        /// Gets or sets the rule parameters.
        /// </summary>
        /// <value>
        /// The rule parameters.
        /// </value>
        internal IEnumerable<RuleParameter> RuleParameters { get; set; }
    }
}
