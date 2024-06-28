// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp.Demos
{
    public class JSON
    {
        public async Task Run(CancellationToken ct = default)
        {            
            Console.WriteLine($"Running {nameof(JSON)}....");

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
            var workflow = JsonConvert.DeserializeObject<Workflow[]>(fileData);

            var bre = new RulesEngine.RulesEngine(workflow, null);

            var resultList = await bre.ExecuteWorkflow("Discount", rp, ct);

            if (resultList != null && resultList.Count > 0)
            {
                resultList.OnSuccess((eventName) => {
                    Console.WriteLine($"Discount offered is {eventName} % over MRP.");
                });

                resultList.OnFail(() => {
                    Console.WriteLine("The user is not eligible for any discount.");
                });
            }
        }
    }
}
