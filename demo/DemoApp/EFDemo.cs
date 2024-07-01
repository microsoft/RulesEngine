// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using DemoApp.EFDataExample;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Extensions;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public class EFDemo : IDemo
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Running {nameof(EFDemo)}....");
        var basicInfo =
            "{\"name\": \"hello\",\"email\": \"abcy@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyaltyFactor\": 3,\"totalPurchasesToDate\": 10000}";
        var orderInfo = "{\"totalOrders\": 5,\"recurringItems\": 2}";
        var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";

        var converter = new ExpandoObjectConverter();

        dynamic input1 = JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
        dynamic input2 = JsonConvert.DeserializeObject<ExpandoObject>(orderInfo, converter);
        dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo, converter);

        var inputs = new[] {input1, input2, input3};

        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Discount.json", SearchOption.AllDirectories);
        if (files == null || files.Length == 0)
        {
            throw new FileNotFoundException("Rules not found.");
        }

        var fileData = await File.ReadAllTextAsync(files[0], cancellationToken);
        var workflow = JsonConvert.DeserializeObject<List<Workflow>>(fileData)!;

        var db = new RulesEngineDemoContext();
        if (await db.Database.EnsureCreatedAsync(cancellationToken))
        {
            db.Workflows.AddRange(workflow);
            await db.SaveChangesAsync(cancellationToken);
        }

        var wfr = db.Workflows.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();

        var bre = new RulesEngine.RulesEngine(wfr);

        var discountOffered = "No discount offered.";

        var resultList = await bre.ExecuteAllRulesAsync("Discount", cancellationToken, inputs);

        resultList.OnSuccess(eventName => {
            discountOffered = $"Discount offered is {eventName} % over MRP.";
        });

        resultList.OnFail(() => {
            discountOffered = "The user is not eligible for any discount.";
        });

        Console.WriteLine(discountOffered);
    }
}