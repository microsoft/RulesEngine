using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.UnitTest
{
    /// <summary>
    /// Class RuleTestClass.
    /// </summary>
    public class RuleTestClass
    {
        /// <summary>
        /// Gets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty("country")]
        public string Country { get; private set; }

        /// <summary>
        /// Gets the loyality factor.
        /// </summary>
        /// <value>
        /// The loyality factor.
        /// </value>
        [JsonProperty("loyalityFactor")]
        public int LoyalityFactor { get; private set; }

        /// <summary>
        /// Gets the total purchases to date.
        /// </summary>
        /// <value>
        /// The total purchases to date.
        /// </value>
        [JsonProperty("totalPurchasesToDate")]
        public int TotalPurchasesToDate { get; private set; }
    }
}
