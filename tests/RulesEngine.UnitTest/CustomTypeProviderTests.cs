// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category", "Unit")]
    [ExcludeFromCodeCoverage]
    public class CustomTypeProviderTests
    {
        [Fact]
        public void GetCustomTypes_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = new CustomTypeProvider(null);

            // Act
            var result = unitUnderTest.GetCustomTypes();

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
