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
                //initiate cancel, set to something lower than time it takes for program to complete to initiate cancel
                //cts.CancelAfter(TimeSpan.FromMilliseconds(94));

                await new Demos.Basic().Run(cts.Token);
                await new Demos.JSON().Run(cts.Token);
                await new Demos.NestedInput().Run(cts.Token);
                await new Demos.EF().Run(cts.Token);
                await new Demos.UseFastExpressionCompiler().Run(cts.Token);
            }

            DateTime end = DateTime.Now;

            Console.WriteLine($"running time: {end-start}");
        }
    }
}
