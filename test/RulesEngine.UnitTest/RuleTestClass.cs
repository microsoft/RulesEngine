using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class RuleTestClass
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loyalityFactor")]
        public int LoyalityFactor { get; set; }
        public int TotalPurchasesToDate { get; set; }
    }
}
