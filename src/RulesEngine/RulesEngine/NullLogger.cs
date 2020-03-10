// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace RulesEngine
{
    internal class NullLogger : ILogger
    {
        public void LogError(Exception ex)
        {
        }

        public void LogTrace(string msg)
        {
        }
    }
}
