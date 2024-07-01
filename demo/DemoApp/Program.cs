// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        using (var cts = new CancellationTokenSource())
        {
            var assembly = Assembly.GetExecutingAssembly();

            var demoTypes = assembly.GetTypes()
                .Where(t => typeof(IDemo).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();

            foreach (var type in demoTypes)
            {
                try
                {
                    if (Activator.CreateInstance(type) is IDemo demo)
                    {
                        await demo.Run(cts.Token);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing Run on {type.Name}: {ex.Message}");
                }
            }
        }

        Console.WriteLine($"running time: {stopwatch.Elapsed}");
    }
}