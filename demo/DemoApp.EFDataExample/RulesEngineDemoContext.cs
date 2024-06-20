using Microsoft.EntityFrameworkCore;
using RulesEngine.Data;
using System;
using System.IO;

namespace DemoApp.EFDataExample;

public class RulesEngineDemoContext : RulesEngineContext
{
    public RulesEngineDemoContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = $"{path}{Path.DirectorySeparatorChar}RulesEngineDemo.db";
    }

    public string DbPath { get; }

    override protected void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}