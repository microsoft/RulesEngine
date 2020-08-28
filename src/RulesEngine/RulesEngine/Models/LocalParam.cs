using Newtonsoft.Json;

namespace RulesEngine.Models
{
    /// <summary>Class Param.
    /// Implements the <see cref="RulesEngine.Models.Rule" /></summary>
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
