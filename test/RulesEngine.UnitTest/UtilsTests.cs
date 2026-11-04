// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using Xunit;

namespace RulesEngine.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class TestClass
    {
        public string Test { get; set; }
        public List<int> TestList { get; set; }
    }

    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class UtilsTests
    {

        [Fact]
        public void GetTypedObject_dynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.Test = "hello";
            obj.TestList = new List<int> { 1, 2, 3 };
            object typedobj = Utils.GetTypedObject(obj);
            Assert.IsNotType<ExpandoObject>(typedobj);
            Assert.NotNull(typedobj.GetType().GetProperty("Test"));
        }

        [Fact]
        public void GetTypedObject_dynamicObject_multipleObjects()
        {
            dynamic obj = new ExpandoObject();
            obj.Test = "hello";
            obj.TestList = new List<int> { 1, 2, 3 };
            dynamic obj2 = new ExpandoObject();
            obj2.Test = "world";
            obj2.TestList = new List<int> { 1, 2, 3 };
            object typedobj = Utils.GetTypedObject(obj);
            object typedobj2 = Utils.GetTypedObject(obj2);
            Assert.IsNotType<ExpandoObject>(typedobj);
            Assert.NotNull(typedobj.GetType().GetProperty("Test"));
            Assert.Equal(typedobj.GetType(), typedobj2.GetType());
        }


        [Fact]
        public void GetTypedObject_nonDynamicObject()
        {
            var obj = new {
                Test = "hello"
            };
            var typedobj = Utils.GetTypedObject(obj);
            Assert.IsNotType<ExpandoObject>(typedobj);
            Assert.NotNull(typedobj.GetType().GetProperty("Test"));
        }


        [Fact]
        public void GetJObject_nonDynamicObject()
        {
            dynamic obj = JObject.FromObject(new {
                Test = "hello"
            });
            dynamic typedobj = Utils.GetTypedObject(obj);
            Assert.IsNotType<ExpandoObject>(typedobj);
            Assert.IsType<JObject>(typedobj);
            Assert.NotNull(typedobj.Test);
        }


        [Fact]
        public void CreateObject_dynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.Test = "test";
            obj.TestList = new List<int> { 1, 2, 3 };

            object newObj = Utils.CreateObject(typeof(TestClass), obj);
            Assert.IsNotType<ExpandoObject>(newObj);
            Assert.NotNull(newObj.GetType().GetProperty("Test"));

        }

        [Fact]
        public void CreateAbstractType_dynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.Test = "test";
            obj.TestList = new List<int> { 1, 2, 3 };
            obj.testEmptyList = new List<object>();

            Type type = Utils.CreateAbstractClassType(obj);
            Assert.NotEqual(typeof(ExpandoObject), type);
            Assert.NotNull(type.GetProperty("Test"));

        }

        [Fact]
        public void GetTypedObject_Dictionary_ReturnsTypedObject()
        {
            var dict = new Dictionary<string, object>
            {
                { "Name", "Alice" },
                { "Age", 25 }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            Assert.NotNull(result.GetType().GetProperty("Name"));
            Assert.NotNull(result.GetType().GetProperty("Age"));
        }

        [Fact]
        public void GetTypedObject_Dictionary_NestedDictionary()
        {
            var dict = new Dictionary<string, object>
            {
                { "Name", "Alice" },
                { "Address", new Dictionary<string, object>
                    {
                        { "City", "Seattle" },
                        { "Zip", "98101" }
                    }
                }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            var addressProp = result.GetType().GetProperty("Address");
            Assert.NotNull(addressProp);
            var address = addressProp.GetValue(result);
            Assert.NotNull(address.GetType().GetProperty("City"));
            Assert.NotNull(address.GetType().GetProperty("Zip"));
        }

        [Fact]
        public void GetTypedObject_Dictionary_WithList()
        {
            var dict = new Dictionary<string, object>
            {
                { "Name", "Alice" },
                { "Scores", new List<object> { 90, 85, 92 } }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            Assert.NotNull(result.GetType().GetProperty("Name"));
            Assert.NotNull(result.GetType().GetProperty("Scores"));
        }

        [Fact]
        public void GetTypedObject_Dictionary_WithEmptyList()
        {
            var dict = new Dictionary<string, object>
            {
                { "Items", new List<object>() }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            Assert.NotNull(result.GetType().GetProperty("Items"));
        }

        [Fact]
        public void GetTypedObject_Dictionary_WithNestedExpandoObject()
        {
            dynamic nested = new ExpandoObject();
            nested.Value = "test";

            var dict = new Dictionary<string, object>
            {
                { "Nested", (object)nested }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            var nestedProp = result.GetType().GetProperty("Nested");
            Assert.NotNull(nestedProp);
        }

        [Fact]
        public void GetTypedObject_Dictionary_WithListOfDictionaries()
        {
            var dict = new Dictionary<string, object>
            {
                { "People", new List<object>
                    {
                        new Dictionary<string, object> { { "Name", "Alice" } },
                        new Dictionary<string, object> { { "Name", "Bob" } }
                    }
                }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            Assert.NotNull(result.GetType().GetProperty("People"));
        }

        [Fact]
        public void GetTypedObject_Dictionary_WithNullValue()
        {
            var dict = new Dictionary<string, object>
            {
                { "Name", "Alice" },
                { "Middle", null }
            };

            var result = Utils.GetTypedObject(dict);
            Assert.IsNotType<Dictionary<string, object>>(result);
            Assert.NotNull(result.GetType().GetProperty("Name"));
        }

    }
}
