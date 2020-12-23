// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

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
