// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AutoFixture;
using RulesEngine.Models;
using System;
using Xunit;

namespace RulesEngine.UnitTest;
public class RuleParameterTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var fixture = new Fixture();
        var name = fixture.Create<string>();
        var type = fixture.Create<Type>();

        var result = RuleParameter.Create(name, type);

        Assert.Equal(name, result.Name);
        Assert.Equal(type, result.Type);
    }
}
