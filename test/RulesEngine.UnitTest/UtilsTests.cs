// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Xunit;

namespace RulesEngine.UnitTest
{
    public class TestClass
    {
        public string test { get; set; }
        public List<int> testList { get; set; }
    }

    [Trait("Category","Unit")]
    public class UtilsTests
    {

        [Fact]
        public void GetTypedObject_dynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.test = "hello";
            obj.testList = new List<int> { 1, 2, 3 };
            object typedobj = Utils.GetTypedObject(obj);
            Assert.IsNotType<ExpandoObject>(typedobj);
            Assert.NotNull(typedobj.GetType().GetProperty("test"));
        }

        [Fact]
        public void GetTypedObject_nonDynamicObject()
        {
            var obj = new {
                test = "hello"
            };
            object typedobj = Utils.GetTypedObject(obj);
            Assert.IsNotType<ExpandoObject>(typedobj);
            Assert.NotNull(typedobj.GetType().GetProperty("test"));
        }

       [Fact]
        public void CreateObject_dynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.test = "test";
            obj.testList = new List<int> { 1, 2, 3 };

            object newObj = Utils.CreateObject(typeof(TestClass), obj);
            Assert.IsNotType<ExpandoObject>(newObj);
            Assert.NotNull(newObj.GetType().GetProperty("test"));

        }

        [Fact]
        public void CreateAbstractType_dynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.test = "test";
            obj.testList = new List<int> { 1, 2, 3 };
            obj.testEmptyList = new List<object>();

            Type type = Utils.CreateAbstractClassType( obj);
            Assert.NotEqual(typeof(ExpandoObject), type);
            Assert.NotNull(type.GetProperty("test"));

        }


    }
}
