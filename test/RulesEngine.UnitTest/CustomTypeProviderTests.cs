// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Dynamic.Core;
using Xunit;

namespace RulesEngine.UnitTest;

/// <inheritdoc />
[Trait("Category", "Unit")]
[ExcludeFromCodeCoverage]
public sealed  class CustomTypeProviderTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private bool _disposed;

    public CustomTypeProviderTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _mockRepository.VerifyAll();
        }

        _disposed = true;
    }

    private CustomTypeProvider CreateProvider()
    {
        return new CustomTypeProvider(ParsingConfig.Default, null);
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
