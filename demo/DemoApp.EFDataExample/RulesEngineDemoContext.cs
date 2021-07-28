using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;

namespace DemoApp.EFDataExample
{
    public class RulesEngineDemoContext : DbContext
    {
        public DbSet<WorkflowRules> WorkflowRules { get; set; }
        public DbSet<ActionInfo> ActionInfos { get; set; }

        public DbSet<RuleActions> RuleActions { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<ScopedParam> ScopedParams { get; set; }

        public string DbPath { get; private set; }

        public RulesEngineDemoContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}RulesEngineDemo.db";
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
          => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ActionInfo>()
                .Property(b => b.Context)
                .HasConversion(
                   v => JsonSerializer.Serialize(v, null),
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, null));

            modelBuilder.Entity<ActionInfo>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<ScopedParam>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<WorkflowRules>(entity => {
                entity.HasKey(k => k.WorkflowName);
            });

            modelBuilder.Entity<RuleActions>(entity => {
                entity.HasNoKey();
                entity.HasOne(o => o.OnSuccess).WithMany();
                entity.HasOne(o => o.OnFailure).WithMany();
            });

            modelBuilder.Entity<Rule>(entity => {
                entity.HasKey(k => k.RuleName);

                entity.Property(b => b.Properties)
                .HasConversion(
                   v => JsonSerializer.Serialize(v, null),
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, null));
                entity.Ignore(e => e.Actions);
            });

            modelBuilder.Entity<WorkflowRules>()
               .Ignore(b => b.WorkflowRulesToInject);

            modelBuilder.Entity<Rule>()
              .Ignore(b => b.WorkflowRulesToInject);
        }
    }

}