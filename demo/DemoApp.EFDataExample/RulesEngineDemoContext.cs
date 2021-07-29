using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Data;
using RulesEngine.Models;

namespace DemoApp.EFDataExample
{
    public class RulesEngineDemoContext : RulesEngineContext
    {
        public string DbPath { get; private set; }

        public RulesEngineDemoContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}RulesEngineDemo.db";
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
          => options.UseSqlite($"Data Source={DbPath}");

    }

}