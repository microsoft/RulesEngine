// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            DateTime start = DateTime.Now;

            using (var cts = new CancellationTokenSource())
            {
                await new Demos.Basic().Run(cts.Token);
                await new Demos.BasicWithCustomTypes().Run(cts.Token);
                await new Demos.JSON().Run(cts.Token);
                await new Demos.NestedInput().Run(cts.Token);
                await new Demos.EF().Run(cts.Token);
                await new Demos.UseFastExpressionCompiler().Run(cts.Token);
                await new Demos.MultipleWorkflows().Run(cts.Token);
                await new Demos.CustomParameterName().Run(cts.Token);
            }

            DateTime end = DateTime.Now;

            Console.WriteLine($"running time: {end-start}");
        }
    }
}
