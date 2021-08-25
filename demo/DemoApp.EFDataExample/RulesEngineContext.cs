using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;

namespace RulesEngine.Data
{
    public class WorkflowData : Workflow
    {
        [JsonIgnore]
        public int Id { get; set; }
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> GlobalParams { get; set; }
    }
    public class RuleData : Rule
    {
        [JsonIgnore]
        public int Id { get; set; }
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> LocalParams { get; set; }
    }
    public class ScopedParamData : ScopedParam
    {
        [JsonIgnore]
        public int Id { get; set; }
    }

    public class RulesEngineContext : DbContext
    {
        public DbSet<WorkflowData> Workflows { get; set; }

        public DbSet<RuleData> Rules { get; set; }

        public DbSet<ScopedParamData> ScopedParams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScopedParamData>(entity => {
                entity.ToTable("ScopedParam");
                entity.HasKey(k => k.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });


            modelBuilder.Entity<WorkflowData>(entity => {
                entity.ToTable("Workflow");
                entity.HasKey(k => k.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Ignore(b => b.WorkflowRulesToInject);
                entity.Ignore(b => b.WorkflowsToInject);
            });

            modelBuilder.Entity<RuleData>(entity => {
                entity.ToTable("Rule");
                entity.HasKey(k => k.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(b => b.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, null));

                entity.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                   v => JsonSerializer.Deserialize<RuleActions>(v, null));

                entity.Ignore(b => b.WorkflowRulesToInject);
                entity.Ignore(b => b.WorkflowsToInject);
            });
        }
        public Workflow[] GetWorkflows(List<WorkflowData> wfs)
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<WorkflowData, Workflow>();
                cfg.CreateMap<RuleData, Rule>();
                cfg.CreateMap<ScopedParamData, ScopedParam>();
            });

            IMapper iMapper = config.CreateMapper();
            return iMapper.Map<List<WorkflowData>, List<Workflow>>(wfs).ToArray();
        }
    }
}