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
        public DbSet<ActionInfo> ActionInfos { get; set; }

        public DbSet<RuleAction> RuleActions { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<ScopedParam> ScopedParams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<ActionInfo>();
            modelBuilder.Ignore<ScopedParam>();
            modelBuilder.Ignore<RuleAction>();

            modelBuilder.Entity<Workflow>(entity => {
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

                entity.Ignore(b => b.WorkflowsToInject);
            });

            modelBuilder.Entity<Rule>(entity => {
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

                entity.Property(b => b.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, null));

                entity.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                   v => JsonSerializer.Deserialize<RuleAction>(v, null));

                entity.Ignore(b => b.WorkflowsToInject);
            });
        }
    }

}