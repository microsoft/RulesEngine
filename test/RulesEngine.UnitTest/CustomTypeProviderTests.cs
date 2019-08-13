// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using Moq;
using System;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    public class CustomTypeProviderTests : IDisposable
    {
        private MockRepository mockRepository;



        public CustomTypeProviderTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private CustomTypeProvider CreateProvider()
        {
            return new CustomTypeProvider(null);
        }

        [Fact]
        public void GetCustomTypes_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();

            // Act
            var result = unitUnderTest.GetCustomTypes();

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
