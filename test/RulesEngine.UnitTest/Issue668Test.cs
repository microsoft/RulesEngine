// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue668Test
    {
        // Reporter scenario: JSON inputs deserialized via System.Text.Json end up as
        // JsonElement values inside an ExpandoObject. Rule expressions then compare
        // those JsonElements against strings/numbers and fail with:
        //   The binary operator Equal is not defined for the types
        //   'System.Text.Json.JsonElement' and 'System.String'.
        //
        // The migration from Newtonsoft.Json to System.Text.Json (#599) is the cause —
        // Newtonsoft produced native .NET types into the ExpandoObject; STJ produces
        // JsonElement.
        [Fact]
        public async Task ExpandoObject_WithJsonElementProperty_ComparedToString()
        {
            // Mirrors what STJ does when deserializing a JSON object into an ExpandoObject
            // via a JsonDocument: each property becomes a JsonElement.
            var json = "{\"country\":\"india\",\"loyaltyFactor\":2}";
            using var doc = JsonDocument.Parse(json);

            // Build an ExpandoObject with JsonElement values, the way #599 would.
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                expando[prop.Name] = prop.Value.Clone(); // JsonElement, NOT a string
            }

            var workflow = new Workflow
            {
                WorkflowName = "Discount",
                Rules = new[] {
                    new Rule { RuleName = "R", Expression = "input1.country == \"india\"" }
                }
            };
            var engine = new RulesEngine(new[] { workflow });
            var results = await engine.ExecuteAllRulesAsync(
                "Discount", new[] { RuleParameter.Create("input1", (ExpandoObject)expando) });

            Assert.True(results[0].IsSuccess,
                $"Expected success. Got ExceptionMessage = {results[0].ExceptionMessage}");
        }
    }
}
