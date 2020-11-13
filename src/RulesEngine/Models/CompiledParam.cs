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
        internal string Name { get; set; }
        internal Type ReturnType { get; set; }
        internal Func<object[],object> Value { get; set; }
        internal RuleParameter AsRuleParameter()
        {
            return new RuleParameter(Name,ReturnType);
        }
    }
}
