#nullable enable
using CoolifiCli.Extensions;
using FluentAssertions;
using Xunit;

namespace CoolifiCli.Tests;

public sealed class StringExtensionsEdgeCaseTests
{
    [Fact]
    public void ToPascalCase_NullInput_ReturnsNull() =>
        ((string?)null).ToPascalCase().Should().BeNull();

    [Fact]
    public void ToPascalCase_EmptyInput_ReturnsEmpty() =>
        "".ToPascalCase().Should().BeEmpty();

    [Theory]
    [InlineData("hello-world", "HelloWorld")]
    [InlineData("deploy_app", "DeployApp")]
    [InlineData("some name", "SomeName")]
    public void ToPascalCase_VariousInputs(string input, string expected) =>
        input.ToPascalCase().Should().Be(expected);

    [Fact]
    public void ToCamelCase_NullInput_ReturnsNull() =>
        ((string?)null).ToCamelCase().Should().BeNull();

    [Theory]
    [InlineData("hello-world", "helloWorld")]
    [InlineData("Deploy App", "deployApp")]
    public void ToCamelCase_VariousInputs(string input, string expected) =>
        input.ToCamelCase().Should().Be(expected);

    [Fact]
    public void ToSnakeCase_NullInput_ReturnsNull() =>
        ((string?)null).ToSnakeCase().Should().BeNull();

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("deployApp", "deploy_app")]
    public void ToSnakeCase_VariousInputs(string input, string expected) =>
        input.ToSnakeCase().Should().Be(expected);

    [Fact]
    public void ToKebabCase_NullInput_ReturnsNull() =>
        ((string?)null).ToKebabCase().Should().BeNull();

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("deployApp", "deploy-app")]
    public void ToKebabCase_VariousInputs(string input, string expected) =>
        input.ToKebabCase().Should().Be(expected);
}
