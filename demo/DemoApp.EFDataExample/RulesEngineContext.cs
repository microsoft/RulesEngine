using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;

namespace RulesEngine.Data
{
    public class RulesEngineContext : DbContext
    {
        public DbSet<Workflow> Workflows { get; set; }

        public DbSet<Rule> Rules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScopedParam>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<Workflow>(entity => {
                entity.HasKey(k => k.WorkflowName);
                entity.Ignore(b => b.WorkflowsToInject);
            });

            modelBuilder.Entity<Rule>(entity => {
                entity.HasKey(k => k.RuleName);

                entity.Property(b => b.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null));

                entity.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<RuleActions>(v, (JsonSerializerOptions)null));

                entity.Ignore(b => b.WorkflowsToInject);
            });

            //modelBuilder.Entity<RulesEngine.Models.Rule>(entity => {
            //    entity.HasKey(k => k.RuleName); //adds shadow property RuleName1
            //    entity.Ignore(b => b.WorkflowsToInject);
            //    entity.Ignore(b => b.WorkflowRulesToInject); //has a get which is why this line is necessary

            //    entity.Property(b => b.Properties).HasConversion(
            //        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            //        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null),
            //        new ValueComparer<Dictionary<string, object>>(
            //            (c1, c2) => c1.SequenceEqual(c2),
            //            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            //            c => c));

            //    entity.Property(p => p.Actions).HasConversion(
            //        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            //        v => JsonSerializer.Deserialize<RulesEngine.Models.RuleActions>(v, (JsonSerializerOptions)null));                
            //});
        }
    }

}