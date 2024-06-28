using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using RulesEngine.Models;

namespace DemoApp.Demos
{
    public class RulesEngineContext : DbContext
    {
        public string DbPath { get; private set; }

        public RulesEngineContext()
        {
            var folder = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = System.IO.Path.GetDirectoryName(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}RulesEngineDemo.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScopedParam>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<Workflow>(entity => {
                entity.HasKey(k => k.WorkflowName);
                entity.Ignore(b => b.WorkflowsToInject);
            });

            modelBuilder.Entity<Rule>().HasOne<Rule>().WithMany(r => r.Rules).HasForeignKey("RuleNameFK");

            modelBuilder.Entity<Rule>(entity => {
                entity.HasKey(k => k.RuleName);

                var valueComparer = new ValueComparer<Dictionary<string, object>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c);

                entity.Property(b => b.Properties).HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v))
                .Metadata
                .SetValueComparer(valueComparer);

                entity.Property(p => p.Actions).HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<RuleActions>(v));

                entity.Ignore(b => b.WorkflowsToInject);
            });
        }

        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<Rule> Rules { get; set; }
    }
}
