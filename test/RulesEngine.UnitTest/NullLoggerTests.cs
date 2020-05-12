// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category","Unit")]
    public class NullLoggerTest
    {
        [Fact]
        public void NullLogger_BeginScope_DoesNotThrow()
        {
            var logger = new NullLogger<RulesEngine>();

            using (logger.BeginScope("test-value"))
            { }
        }

        [Theory]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void NullLogger_IsEnabled_ReturnsTrue(LogLevel logLevel)
        {
            var logger = new NullLogger<RulesEngine>();
            logger.IsEnabled(logLevel).Equals(true);
        }

        [Fact]
        public void NullLogger_Log_DoesNotThrow() 
        {
            var logger = new NullLogger<RulesEngine>();
            logger.Log(LogLevel.Critical, 1, "This is a critical message.");
        }
    }
}