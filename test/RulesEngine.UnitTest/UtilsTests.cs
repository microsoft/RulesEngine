﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
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
        public void CreateAbstractClassType_WithJsonElement_ShouldConvertToExpandoObject()
        {
            const string jsonString = @"{""name"":""John"", ""age"":30, ""isStudent"":false}";
            var document = JsonDocument.Parse(jsonString);
            var jsonElement = document.RootElement;

            var type = Utils.CreateAbstractClassType(jsonElement);

            var propertyNames = type.GetProperties().Select(p => p.Name).ToArray();

            Assert.Contains("name", propertyNames);
            Assert.Contains("age", propertyNames);
            Assert.Contains("isStudent", propertyNames);
        }

        [Fact]
        public void CreateObject_WithJsonElement_ShouldConvertToExpandoObject()
        {
            var jsonString = @"{""name"":""John"", ""age"":30, ""isStudent"":false}";
            var document = JsonDocument.Parse(jsonString);
            var jsonElement = document.RootElement;
            var expando = jsonElement.ToExpandoObject();

            Type type = Utils.CreateAbstractClassType(expando);
            var result = Utils.CreateObject(type, expando);

            Assert.Equal("John", result.name);
            Assert.Equal(30, result.age);
            Assert.False(result.isStudent);
        }

        [Fact]
        public void CreateObject_WithJsonElementNested_ShouldConvertToExpandoObject()
        {
            var jsonString = @"{""name"":""John"", ""details"":{""age"":30, ""isStudent"":false}}";
            var document = JsonDocument.Parse(jsonString);
            var jsonElement = document.RootElement;
            var expando = jsonElement.ToExpandoObject();

            Type type = Utils.CreateAbstractClassType(expando);
            var result = Utils.CreateObject(type, expando);

            Assert.Equal("John", result.name);
            Assert.Equal(30, result.details.age);
            Assert.False(result.details.isStudent);
        }

        [Fact]
        public void CreateObject_WithJsonElementArray_ShouldConvertToExpandoObject()
        {
            const string jsonString = @"{""name"":""John"", ""scores"":[100, 95, 85]}";
            var document = JsonDocument.Parse(jsonString);
            var jsonElement = document.RootElement;
            var expando = jsonElement.ToExpandoObject();

            var type = Utils.CreateAbstractClassType(expando);
            var result = Utils.CreateObject(type, expando);

            Assert.Equal("John", result.name);

            var scores = (List<int>)result["scores"];
            Assert.Equal(100, scores[0]);
            Assert.Equal(95, scores[1]);
            Assert.Equal(85, scores[2]);
        }
    }
}