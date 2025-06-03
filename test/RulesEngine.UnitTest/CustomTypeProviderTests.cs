// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class CustomTypeProviderTests : IDisposable
    {
        public void Dispose()
        {
        }

        private CustomTypeProvider CreateProvider(params Type[] customTypes)
        {
            return new CustomTypeProvider(customTypes);
        }

        [Fact]
        public void GetCustomTypes_DefaultProvider_IncludesEnumerableAndObject()
        {
            var provider = CreateProvider();
            var allTypes = provider.GetCustomTypes();
            Assert.NotEmpty(allTypes);
            Assert.Contains(typeof(System.Linq.Enumerable), allTypes);
            Assert.Contains(typeof(object), allTypes);
        }

        [Fact]
        public void GetCustomTypes_WithListOfGuid_ContainsIEnumerableOfGuid()
        {
            var initial = new[] { typeof(List<Guid>) };
            var provider = CreateProvider(initial);
            var allTypes = provider.GetCustomTypes();
            Assert.Contains(typeof(IEnumerable<Guid>), allTypes);
            Assert.Contains(typeof(List<Guid>), allTypes);
            Assert.Contains(typeof(System.Linq.Enumerable), allTypes);
            Assert.Contains(typeof(object), allTypes);
        }

        [Fact]
        public void GetCustomTypes_ListOfListString_ContainsIEnumerableOfListString()
        {
            var nestedListType = typeof(List<List<string>>);
            var provider = CreateProvider(nestedListType);
            var allTypes = provider.GetCustomTypes();
            Assert.Contains(typeof(IEnumerable<List<string>>), allTypes);
            Assert.Contains(nestedListType, allTypes);
            Assert.Contains(typeof(System.Linq.Enumerable), allTypes);
            Assert.Contains(typeof(object), allTypes);
        }

        [Fact]
        public void GetCustomTypes_ArrayOfStringArrays_ContainsIEnumerableOfStringArray()
        {
            var arrayType = typeof(string[][]);
            var provider = CreateProvider(arrayType);
            var allTypes = provider.GetCustomTypes();
            Assert.Contains(typeof(IEnumerable<string[]>), allTypes);
            Assert.Contains(arrayType, allTypes);
            Assert.Contains(typeof(System.Linq.Enumerable), allTypes);
            Assert.Contains(typeof(object), allTypes);
        }

        [Fact]
        public void GetCustomTypes_NullableIntArray_ContainsIEnumerableOfNullableInt()
        {
            var nullableInt = typeof(int?);
            var arrayType = typeof(int?[]);
            var provider = CreateProvider(arrayType);
            var allTypes = provider.GetCustomTypes();
            Assert.Contains(typeof(IEnumerable<int?>), allTypes);
            Assert.Contains(arrayType, allTypes);
            Assert.Contains(typeof(System.Linq.Enumerable), allTypes);
            Assert.Contains(typeof(object), allTypes);
        }

        [Fact]
        public void GetCustomTypes_MultipleTypes_NoDuplicates()
        {
            var repeatedType = typeof(List<string>);
            var provider = CreateProvider(repeatedType, repeatedType);
            var allTypes = provider.GetCustomTypes();
            var matches = allTypes.Where(t => t == repeatedType).ToList();
            Assert.Single(matches);
            var interfaceMatches = allTypes.Where(t => t == typeof(IEnumerable<string>)).ToList();
            Assert.Single(interfaceMatches);
        }
    }
}
