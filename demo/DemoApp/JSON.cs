// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class JSON : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(JSON)}....");

        var rp = new RuleParameter[] {
            new("input1", new {
                name = "hello",
                email = "abcy@xyz.com",
                creditHistory = "good",
                country = "canada",
                loyaltyFactor = 3,
                totalPurchasesToDate = 10000
            }),
            new("input2", new {totalOrders = 5, recurringItems = 2}),
            new("input3", new {noOfVisitsPerMonth = 10, percentageOfBuyingToVisit = 15})
        };

        var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Workflows");
        var files = Directory.GetFiles(dir, "Discount.json", SearchOption.AllDirectories);
        if (files == null || files.Length == 0)
        {
            throw new FileNotFoundException("Rules not found.");
        }

        var fileData = await File.ReadAllTextAsync(files[0], cancellationToken);
        var workflow = JsonConvert.DeserializeObject<Workflow[]>(fileData);

        var bre = new RulesEngine.RulesEngine(workflow);

        var ret = await bre.ExecuteAllRulesAsync("Discount", cancellationToken, rp);

        ret.OnSuccess(eventName => {
            Console.WriteLine($"Discount offered is {eventName} % over MRP.");
        });

        ret.OnFail(() => {
            Console.WriteLine("The user is not eligible for any discount.");
        });
    }
}