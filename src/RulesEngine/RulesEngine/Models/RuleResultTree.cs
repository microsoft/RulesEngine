// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RulesEngine.Models
{
    /// <summary>
    /// Rule result class with child result heirarchy
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RuleResultTree
    {
        /// <summary>
        /// Gets or sets the rule.
        /// </summary>
        /// <value>
        /// The rule.
        /// </value>
        public Rule Rule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the child result.
        /// </summary>
        /// <value>
        /// The child result.
        /// </value>
        public IEnumerable<RuleResultTree> ChildResults { get; set; }

        /// <summary>
        /// Gets or sets the input object
        /// </summary>
        public object Input { get; set; }

        /// <summary>
        /// Gets the exception message in case an error is thrown during rules calculation.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the rule evaluated parameters.
        /// </summary>
        /// <value>
        /// The rule evaluated parameters.
        /// </value>
        public IEnumerable<RuleParameter> RuleEvaluatedParams { get; set; }

        /// <summary>
        /// This method will return all the error and warning messages to caller
        /// </summary>
        /// <returns>RuleResultMessage</returns>
        [ExcludeFromCodeCoverage]
        public RuleResultMessage GetMessages()
        {
            RuleResultMessage ruleResultMessage = new RuleResultMessage();

            Helpers.ToResultTreeMessages(this, ref ruleResultMessage);

            return ruleResultMessage;
        }
    }

    /// <summary>
    /// This class will hold the error messages
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RuleResultMessage
    {
        /// <summary>
        /// Constructor will innitilaze the List 
        /// </summary>
        public RuleResultMessage()
        {
            ErrorMessages = new List<string>();
            WarningMessages = new List<string>();
        }

        /// <summary>
        /// This will hold the list of error messages
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// This will hold the list of warning messages
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
