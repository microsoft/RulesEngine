// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>Class LocalParam.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ScopedParam
    {

        /// <summary>
        /// Gets or sets the name of the param.
        /// </summary>
        /// <value>
        /// The name of the rule.
        /// </value>]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the lambda expression which can be reference in Rule. 
        /// </summary>
        public string Expression { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class LocalParam : ScopedParam { }
}
