// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
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
        public void GetTypedObject_dynamicObject_withExpandoObject()
        {
            // Arrange
            dynamic obj = new ExpandoObject();
            obj.Test = "hello";

            dynamic childObject = new ExpandoObject();
            childObject.Id = 1;
            obj.Child = childObject;

            // Act
            object typedObject = Utils.GetTypedObject(obj);

            // Assert
            Assert.IsNotType<ExpandoObject>(typedObject);

            var childProperty = typedObject.GetType().GetProperty("Child");
            Assert.NotNull(childProperty);
            Assert.NotNull(childProperty.PropertyType.GetProperty("Id"));
        }

        [Fact]
        public void GetTypedObject_dynamicObject_withEmptyList()
        {
            // Arrange
            dynamic obj = new ExpandoObject();
            obj.Test = "hello";
            obj.TestList = new List<int>();

            // Act
            object typedObject = Utils.GetTypedObject(obj);

            // Assert
            Assert.IsNotType<ExpandoObject>(typedObject);
            Assert.NotNull(typedObject.GetType().GetProperty("TestList"));
        }

        [Fact]
        public void GetTypedObject_dynamicObject_withNestedList()
        {
            // Arrange
            dynamic obj = new ExpandoObject();
            obj.Test = "hello";

            dynamic testListItemOne = new ExpandoObject();
            testListItemOne.Id = 1;
            testListItemOne.ChildList = new[] { "ChildListItem" };

            obj.TestList = new[] { testListItemOne };

            // Act
            object typedObject = Utils.GetTypedObject(obj);

            // Assert
            Assert.IsNotType<ExpandoObject>(typedObject);
            var listTypePropertyInfo = typedObject.GetType().GetProperty("TestList");
            Assert.NotNull(listTypePropertyInfo);

            var internalType = listTypePropertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault();
            Assert.NotNull(internalType);
            Assert.NotNull(internalType.GetProperty("ChildList"));
        }

        [Fact]
        public void GetTypedObject_dynamicObject_withListOfExpandoObject()
        {
            // Arrange
            dynamic obj = new ExpandoObject();
            obj.Test = "hello";

            dynamic testListItemOne = new ExpandoObject();
            testListItemOne.Id = 1;
            testListItemOne.FirstName = "FirstName";

            dynamic testListItemSecond = new ExpandoObject();
            testListItemSecond.Id = 2;
            testListItemSecond.LastName = "LastName";

            obj.TestList = new[] { testListItemOne, testListItemSecond };

            // Act
            object typedObject = Utils.GetTypedObject(obj);

            // Assert
            Assert.IsNotType<ExpandoObject>(typedObject);

            var listTypePropertyInfo = typedObject.GetType().GetProperty("TestList");
            Assert.NotNull(listTypePropertyInfo);

            var internalType = listTypePropertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault();
            Assert.NotNull(internalType);

            var internalTypeProperties = internalType.GetProperties();
            Assert.Single(internalTypeProperties, x => x.Name == "Id");
            Assert.Single(internalTypeProperties, x => x.Name == "FirstName");
            Assert.Single(internalTypeProperties, x => x.Name == "LastName");
        }
    }
}