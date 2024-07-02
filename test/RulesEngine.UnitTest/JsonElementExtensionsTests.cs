using RulesEngine.HelperFunctions;
using System.Text.Json;
using Xunit;

namespace RulesEngine.UnitTest;

public class JsonElementExtensionsTests
{
    [Fact]
    public void TestObjectConversion()
    {
        var jsonString = @"{""name"":""John"", ""age"":30, ""isStudent"":false}";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Equal("John", expando.name);
        Assert.Equal(30, expando.age);
        Assert.False(expando.isStudent);
    }

    [Fact]
    public void TestArrayConversion()
    {
        var jsonString = @"[""apple"", ""banana"", ""cherry""]";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Equal("apple", expando[0]);
        Assert.Equal("banana", expando[1]);
        Assert.Equal("cherry", expando[2]);
    }

    [Fact]
    public void TestStringConversion()
    {
        var jsonString = @"""Hello, World!""";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Equal("Hello, World!", expando);
    }

    [Fact]
    public void TestNumberConversion_Int()
    {
        var jsonString = "42";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Equal(42, expando);
    }

    [Fact]
    public void TestNumberConversion_Long()
    {
        var jsonString = "9223372036854775807";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Equal(9223372036854775807L, expando);
    }

    [Fact]
    public void TestNumberConversion_Decimal()
    {
        const string jsonString = "12345.6789";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Equal(12345.6789m, expando);
    }

    [Fact]
    public void TestBooleanConversion_True()
    {
        const string jsonString = "true";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.True(expando);
    }

    [Fact]
    public void TestBooleanConversion_False()
    {
        const string jsonString = "false";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.False(expando);
    }

    [Fact]
    public void TestNullConversion()
    {
        const string jsonString = "null";
        var document = JsonDocument.Parse(jsonString);
        var expando = document.RootElement.ToExpandoObject();

        Assert.Null(expando);
    }

    [Fact]
    public void TestUndefinedConversion()
    {
        JsonElement undefinedElement = default;
        var expando = undefinedElement.ToExpandoObject();

        Assert.Null(expando);
    }
}