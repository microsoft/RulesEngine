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
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, null));

                entity.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                   v => JsonSerializer.Deserialize<RuleActions>(v, null));

                entity.Ignore(b => b.WorkflowsToInject);
            });
        }
    }

}