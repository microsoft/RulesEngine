﻿// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using DemoApp.EFDataExample;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;
using Microsoft.EntityFrameworkCore;

namespace DemoApp
{
    public class EFDemo
    {
        public void Run()
        {
            Console.WriteLine($"Running {nameof(EFDemo)}....");
            var basicInfo = "{\"name\": \"hello\",\"email\": \"abcy@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyalityFactor\": 3,\"totalPurchasesToDate\": 10000}";
            var orderInfo = "{\"totalOrders\": 5,\"recurringItems\": 2}";
            var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";

            var converter = new ExpandoObjectConverter();

            dynamic input1 = JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
            dynamic input2 = JsonConvert.DeserializeObject<ExpandoObject>(orderInfo, converter);
            dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo, converter);

            var inputs = new dynamic[]
                {
                    input1,
                    input2,
                    input3
                };

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Discount.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");

            var fileData = File.ReadAllText(files[0]);
            var workflowRules = JsonConvert.DeserializeObject<List<WorkflowRules>>(fileData);

            RulesEngineDemoContext db = new RulesEngineDemoContext();
            if (db.Database.EnsureCreated())
            {
                db.WorkflowRules.AddRange(workflowRules);
                db.SaveChanges();
            }

            var wfr = db.WorkflowRules.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();

            var bre = new RulesEngine.RulesEngine(wfr, null);

            string discountOffered = "No discount offered.";

            List<RuleResultTree> resultList = bre.ExecuteAllRulesAsync("Discount", inputs).Result;

            resultList.OnSuccess((eventName) => {
                discountOffered = $"Discount offered is {eventName} % over MRP.";
            });

            resultList.OnFail(() => {
                discountOffered = "The user is not eligible for any discount.";
            });

            Console.WriteLine(discountOffered);
        }
    }
}
