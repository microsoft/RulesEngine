// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace RulesEngine
{
    internal class NullLogger : ILogger
    {
        public void LogError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            Console.WriteLine(ex);
        }

        public void LogTrace(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }
    }
}
