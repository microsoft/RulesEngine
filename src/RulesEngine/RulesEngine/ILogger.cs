// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace RulesEngine
{
    public interface ILogger
    {
        void LogTrace(string msg);
        void LogError(Exception ex);
    }
}
