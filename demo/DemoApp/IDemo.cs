// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace DemoApp;

public interface IDemo
{
    Task Run(CancellationToken cancellationToken = default);
}