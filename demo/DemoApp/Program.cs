// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DemoApp.Demo;

namespace DemoApp;

public static class Program
{
    public static void Main(string[] args)
    {
        new Basic().Run();
        new Json().Run();
        new NestedInput().Run();
        new Ef().Run();
    }
}