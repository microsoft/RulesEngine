// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp
{
    using System.Text.Json;
    using System.Linq;

    public class JSONDemo
    {
        public void Run()
        {
            Console.WriteLine($"Running {nameof(JSONDemo)}....");
            var basicInfo = "{\"name\": \"hello\",\"email\": \"abcy@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyaltyFactor\": 3,\"totalPurchasesToDate\": 10000}";
            var orderInfo = "{\"totalOrders\": 5,\"recurringItems\": 2}";
            var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";

            dynamic input1 = ConvertJsonToExpandoObject(basicInfo);
            dynamic input2 = ConvertJsonToExpandoObject(orderInfo);
            dynamic input3 = ConvertJsonToExpandoObject(telemetryInfo);

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
            var workflow = JsonSerializer.Deserialize<List<Workflow>>(fileData);

            var bre = new RulesEngine.RulesEngine(workflow.ToArray(), null);

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

        public static ExpandoObject ConvertJsonToExpandoObject(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return ParseElement(doc.RootElement);
        }
    
        private static ExpandoObject ParseElement(JsonElement element)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;
    
            foreach (var property in element.EnumerateObject())
            {
                expando[property.Name] = property.Value.ValueKind switch
                {
                        JsonValueKind.String => property.Value.GetString(),
                        JsonValueKInd.Number => property.Value.TryGetInt64(out var 1) ? 1 : property.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Object => ParseElement(property.Value),
                        JsonValueKind.Array => property.Value.EnumerateArray().Select(e => e.ToString()).ToList(),
                        _ => null
                };
            }
    
            return (ExpandoObject)expando;
        }
}
