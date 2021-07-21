﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RulesEngine.Models;

namespace DemoApp
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
                   v => JsonConvert.SerializeObject(v),
                   v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v));

            modelBuilder.Entity<ActionInfo>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<ScopedParam>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<WorkflowRules>(entity => {
                entity.HasKey(k => k.WorkflowName);
               // entity.Property(b => b.Rules)
               //.HasConversion(
               //   v => v,
               //   v => ((List<Rule>)v).Count == 0 ? null : v);
            });




            //modelBuilder.Entity<Rule>()
            //  .Property(b => b.Actions.OnSuccess.Context)
            //  .HasConversion(
            //     v => JsonConvert.SerializeObject(v),
            //     v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v));

            //modelBuilder.Entity<Rule>()
            // .Property(b => b.Actions.OnFailure.Context)
            // .HasConversion(
            //    v => JsonConvert.SerializeObject(v),
            //    v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v));


            modelBuilder.Entity<RuleActions>(entity => {
                entity.HasNoKey();
                entity.HasOne(o => o.OnSuccess).WithMany();
                entity.HasOne(o => o.OnFailure).WithMany();
            });


            modelBuilder.Entity<Rule>(entity => {
                entity.HasKey(k => k.RuleName);
                //entity.Property(p => p.Rules)
                //    .HasConversion(
                //    r => r, 
                //    r => r);
                //entity.Property(b => b.Rules)
                //.HasConversion(
                // v => v,
                // v => ((List<Rule>)v).Count == 0 ? null : v);

                entity.Property(b => b.Properties)
                .HasConversion(
                   v => JsonConvert.SerializeObject(v),
                   v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v));
                entity.Ignore(e => e.Actions);
                //entity.HasNoKey();
                //entity.HasOne(o => o.Actions).WithMany().HasForeignKey(k => k.RuleName);
            });


            //modelBuilder.Entity<ActionInfo>(entity => {
            //    entity.HasNoKey();
            //});

            //modelBuilder.Entity<RuleActions>()
            //.Property(p => p.Id).ValueGeneratedOnAdd();

            //modelBuilder.Entity<RuleActions>()
            //.HasKey(k => k.Id);

            //modelBuilder.Entity<RuleActions>()
            //.HasOne(o => o.OnFailure).WithOne(o => o.Actions);//.HasForeignKey("Rule");

            modelBuilder.Entity<WorkflowRules>()
               .Ignore(b => b.WorkflowRulesToInject);

            modelBuilder.Entity<Rule>()
              .Ignore(b => b.WorkflowRulesToInject);

        }
    }

}