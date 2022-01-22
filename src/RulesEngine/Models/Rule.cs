// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RulesEngine.Models
{
    /// <summary>
    /// Rule class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Rule
    {
        /// <summary>
        /// default contructor
        /// </summary>
        public Rule()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Id is Primary Key in Database
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Name for the Rule
        /// </summary>
        public string Name { get; set; }                
        public string Operator { get; set; }
        public string ErrorMessage { get; set; }
        public string Expression { get; set; }
        public string SuccessEvent { get; set; }
        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<ScopedParam> LocalParams { get; set; }

        /// <summary>
        /// Gets or sets whether the rule is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        [JsonConverter(typeof(StringEnumConverter))]
        public RuleExpressionType RuleExpressionType { get; set; } = RuleExpressionType.LambdaExpression;

        /// <summary>	
        /// Gets or sets the custom property or tags of the rule.	
        /// </summary>	
        /// <value>	
        /// The properties of the rule.	
        /// </value>
        public Dictionary<string, object> Properties { get; set; }
        public RuleActions Actions { get; set; }

        [NotMapped]
        public IEnumerable<string> WorkflowsToInject { get; set; }

        [Obsolete("will be removed in next major version")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; } = ErrorType.Warning;
    }

    public class RuleConfiguration : IEntityTypeConfiguration<Rule>
    {
        public void Configure(EntityTypeBuilder<Rule> builder)
        {
            builder.Property(b => b.Properties)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v));

            builder.Property(p => p.Actions)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<RuleActions>(v));
        }
    }
}
