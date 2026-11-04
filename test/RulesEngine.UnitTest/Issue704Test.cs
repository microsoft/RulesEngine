// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class Issue704Test
    {
        [Fact]
        public void GetTypedObject_ListOfExpandoWithDifferentProperties_KeepsLaterElementsProperties()
        {
            // Element 0 has properties { a, b }, element 1 has { a, b, c }.
            // Expected: the generated type includes 'c', and element 1's 'c' value survives.
            IDictionary<string, object> e0 = new ExpandoObject();
            e0["a"] = 1;
            e0["b"] = "hello";

            IDictionary<string, object> e1 = new ExpandoObject();
            e1["a"] = 2;
            e1["b"] = "world";
            e1["c"] = 42;

            IDictionary<string, object> root = new ExpandoObject();
            root["items"] = new List<object> { e0, e1 };

            var typed = Utils.GetTypedObject(root);
            var itemsProp = typed.GetType().GetProperty("items");
            Assert.NotNull(itemsProp);

            var itemsList = (System.Collections.IList)itemsProp.GetValue(typed);
            Assert.Equal(2, itemsList.Count);

            var elemType = itemsList[0].GetType();
            Assert.NotNull(elemType.GetProperty("a"));
            Assert.NotNull(elemType.GetProperty("b"));
            // The key assertion: 'c' must be in the unified type
            Assert.NotNull(elemType.GetProperty("c"));

            // And element 1 must actually carry the 'c' value
            var elem1C = elemType.GetProperty("c").GetValue(itemsList[1]);
            Assert.Equal(42, elem1C);
        }

        [Fact]
        public void GetTypedObject_NestedListPropertiesAreUnioned()
        {
            // records[0].product.details has { type, sku }; records[1].product.details has { type, sku, loc }
            IDictionary<string, object> details0 = new ExpandoObject();
            details0["type"] = "electronic";
            details0["sku"] = "123";

            IDictionary<string, object> product0 = new ExpandoObject();
            product0["details"] = details0;

            IDictionary<string, object> record0 = new ExpandoObject();
            record0["product"] = product0;

            IDictionary<string, object> details1 = new ExpandoObject();
            details1["type"] = "electronic";
            details1["sku"] = "45";
            details1["loc"] = "TR";

            IDictionary<string, object> product1 = new ExpandoObject();
            product1["details"] = details1;

            IDictionary<string, object> record1 = new ExpandoObject();
            record1["product"] = product1;

            IDictionary<string, object> root = new ExpandoObject();
            root["records"] = new List<object> { record0, record1 };

            var typed = Utils.GetTypedObject(root);
            var records = (System.Collections.IList)typed.GetType().GetProperty("records").GetValue(typed);

            var detailsType = records[1].GetType()
                .GetProperty("product").PropertyType
                .GetProperty("details").PropertyType;

            Assert.NotNull(detailsType.GetProperty("type"));
            Assert.NotNull(detailsType.GetProperty("sku"));
            Assert.NotNull(detailsType.GetProperty("loc"));

            var product = records[1].GetType().GetProperty("product").GetValue(records[1]);
            var details = product.GetType().GetProperty("details").GetValue(product);
            Assert.Equal("TR", details.GetType().GetProperty("loc").GetValue(details));
        }
    }
}
