﻿// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>Class LocalParam.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ScopedParam
    {

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        /// <value>
        /// The name of the rule.
        /// </value>
        [JsonProperty, JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the lambda expression. 
        /// </summary>
        [JsonProperty, JsonRequired]
        public string Expression { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class LocalParam : ScopedParam { }
}
