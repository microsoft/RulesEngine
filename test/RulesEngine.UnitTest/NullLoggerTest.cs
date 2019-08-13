// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RulesEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RulesEngine.UnitTest
{
    [Trait("Category","Unit")]
    public class NullLoggerTest
    {
        [Fact]
        public void NullLogger_LogTrace()
        {
            var logger = new NullLogger();
            logger.LogTrace("hello");
        }


        [Fact]
        public void NullLogger_LogError()
        {
            var logger = new NullLogger();
            logger.LogError(new Exception("hello"));
        }
    }
}
