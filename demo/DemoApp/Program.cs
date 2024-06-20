// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DemoApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new Demos.Basic().Run();
            new Demos.JSON().Run();
            new Demos.NestedInput().Run();
            new Demos.EF().Run();
        }
    }
}
