using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>Class LocalParam.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LocalParam
    {

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        /// <value>
        /// The name of the rule.
        /// </value>
        [JsonProperty, JsonRequired]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or Sets the lambda expression. 
        /// </summary>
        [JsonProperty, JsonRequired]
        public string Expression { get; private set; }
    }
}
