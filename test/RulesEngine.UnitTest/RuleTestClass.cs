using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RulesEngine.UnitTest
{
    /// <summary>
    /// Class RuleTestClass.
    /// </summary>

    [ExcludeFromCodeCoverage]
    public class RuleTestClass
    {
        /// <summary>
        /// Gets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets the loyality factor.
        /// </summary>
        /// <value>
        /// The loyality factor.
        /// </value>
        [JsonProperty("loyalityFactor")]
        public int LoyalityFactor { get; set; }

        /// <summary>
        /// Gets the total purchases to date.
        /// </summary>
        /// <value>
        /// The total purchases to date.
        /// </value>
        [JsonProperty("totalPurchasesToDate")]
        public int TotalPurchasesToDate { get; set; }
    }
}
