// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class CustomTypeProviderTests : IDisposable
    {
        private readonly MockRepository _mockRepository;
        public CustomTypeProviderTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
        }

        public void Dispose()
        {
            _mockRepository.VerifyAll();
        }

        private CustomTypeProvider CreateProvider()
        {
            return new CustomTypeProvider(null);
        }

        [Fact]
        public void GetCustomTypes_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateProvider();

            // Act
            var result = unitUnderTest.GetCustomTypes();

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
