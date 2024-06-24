// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class EF
    {
        public async Task Run(CancellationToken ct = default)
        {
            Console.WriteLine($"Running {nameof(EF)}....");
            
            var rp = new RuleParameter[] {
                new RuleParameter("input1", new { name = "hello", email = "abcy@xyz.com", creditHistory = "good", country = "canada", loyaltyFactor = 3, totalPurchasesToDate = 10000 }),
                new RuleParameter("input2", new { totalOrders = 5 , recurringItems = 2 }),
                new RuleParameter("input3", new { noOfVisitsPerMonth = 10, percentageOfBuyingToVisit = 15 })
            };

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Workflows";
            var files = Directory.GetFiles(dir, "Discount.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");

            var fileData = await File.ReadAllTextAsync(files[0]);
            var workflow = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

            Workflow[] wfr = null;
            using (RulesEngineContext db = new RulesEngineContext())
            {
                if (db.Database.EnsureCreated())
                {
                    await db.Workflows.AddRangeAsync(workflow, ct);
                    await db.SaveChangesAsync(ct);
                }

                wfr = db.Workflows.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();
            }

            if (wfr != null)
            {
                var bre = new RulesEngine.RulesEngine(wfr, null);

                await foreach (var rrt in bre.ExecuteAllWorkflows(rp, ct))
                {
                    rrt.OnSuccess((eventName) => {
                        Console.WriteLine($"Discount offered is {eventName} % over MRP.");
                    });

                    rrt.OnFail(() => {
                        Console.WriteLine("The user is not eligible for any discount.");
                    });
                }
            }
        }
    }
}
