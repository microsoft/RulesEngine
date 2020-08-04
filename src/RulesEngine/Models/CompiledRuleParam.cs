using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RulesEngine.Models
{
    [ExcludeFromCodeCoverage]
    internal class CompiledRuleParam
    {
        internal string Name { get; set; }
        internal IEnumerable<CompiledParam> CompiledParameters { get; set; }
        internal IEnumerable<RuleParameter> RuleParameters { get; set; }
    }
}
