using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RulesEngine.Models
{
    /// <summary>
    /// CompiledParam class.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
        internal Func<object[],object> Value { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        internal IEnumerable<RuleParameter> Parameters { get; set; }

        internal RuleParameter AsRuleParameter()
        {
            return new RuleParameter(Name,Value.Method.ReturnType);
        }
    }
}
