// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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


    }
}
