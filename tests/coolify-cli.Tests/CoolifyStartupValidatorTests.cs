using CoolifyCli.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class CoolifyStartupValidatorTests
{
    private const string RealToken = "9f2c1a7e-4b3d-4e5f-8a6b-0c1d2e3f4a5b";

    /// <summary>Environment lookup backed by a fixed set of variables. Also points the config file at a missing path.</summary>
    private static Func<string, string?> Env(params (string Name, string Value)[] variables)
    {
        var map = variables.ToDictionary(v => v.Name, v => v.Value);
        map["COOLIFY_CONFIG_FILE"] = "/nonexistent/coolify/config.json";
        return name => map.TryGetValue(name, out var value) ? value : null;
    }

    [Fact]
    public async Task ValidateAsync_WithValidConfiguration_ReturnsSuccessAndWritesNothing()
    {
        var output = new StringWriter();

        var exitCode = await CoolifyStartupValidator.ValidateAsync(
            [],
            output,
            Env(("COOLIFY_URL", "https://coolify.example.com"), ("COOLIFY_TOKEN", RealToken)));

        exitCode.Should().Be(Constants.ExitCodes.Success);
        output.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithMissingToken_ReturnsConfigurationErrorExitCode()
    {
        var output = new StringWriter();

        var exitCode = await CoolifyStartupValidator.ValidateAsync(
            [],
            output,
            Env(("COOLIFY_URL", "https://coolify.example.com")));

        exitCode.Should().Be(Constants.ExitCodes.ConfigurationError);
        output.ToString().Should().Contain("ApiToken is required");
    }

    [Fact]
    public async Task ValidateAsync_WithSeveralProblems_ListsEveryProblemInOneMessage()
    {
        var output = new StringWriter();

        var exitCode = await CoolifyStartupValidator.ValidateAsync(
            [],
            output,
            Env(
                ("COOLIFY_URL", "ftp://coolify.example.com"),
                ("COOLIFY_TOKEN", "changeme"),
                ("COOLIFY_TIMEOUT", "0")));

        var message = output.ToString();
        exitCode.Should().Be(Constants.ExitCodes.ConfigurationError);
        message.Should().Contain("Invalid Coolify configuration (3 problems):");
        message.Should().Contain("  - BaseUrl must use the http or https scheme.");
        message.Should().Contain("  - ApiToken looks like a placeholder value.");
        message.Should().Contain("  - TimeoutSeconds must be between 1 and 600 (got 0).");
        message.Should().Contain(CoolifyStartupValidator.DocumentationPath);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidConfiguration_NeverEchoesTokenValue()
    {
        const string secret = "s3cr3t-token-VALUE-123";
        var output = new StringWriter();

        await CoolifyStartupValidator.ValidateAsync(
            [],
            output,
            Env(
                ("COOLIFY_URL", "ftp://coolify.example.com"),
                ("COOLIFY_TOKEN", secret)));

        output.ToString().Should().NotContain(secret);
    }

    [Fact]
    public async Task ValidateAsync_WithNonNumericTimeout_ReportsItAsAProblem()
    {
        var output = new StringWriter();

        var exitCode = await CoolifyStartupValidator.ValidateAsync(
            [],
            output,
            Env(
                ("COOLIFY_URL", "https://coolify.example.com"),
                ("COOLIFY_TOKEN", RealToken),
                ("COOLIFY_TIMEOUT", "soon")));

        exitCode.Should().Be(Constants.ExitCodes.ConfigurationError);
        output.ToString().Should().Contain("TimeoutSeconds");
    }

    [Fact]
    public async Task ValidateAsync_WithMalformedConfigFile_ReturnsConfigurationErrorExitCode()
    {
        var path = Path.Combine(Path.GetTempPath(), "coolify-startup-" + Guid.NewGuid() + ".json");
        await File.WriteAllTextAsync(path, "{ not json");

        try
        {
            var output = new StringWriter();

            var exitCode = await CoolifyStartupValidator.ValidateAsync(
                ["--coolify-config", path],
                output,
                Env());

            exitCode.Should().Be(Constants.ExitCodes.ConfigurationError);
            output.ToString().Should().StartWith("Invalid Coolify configuration (1 problem):");
            output.ToString().Should().Contain("Failed to load configuration from file");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Format_WithSingleProblem_UsesSingularNoun()
    {
        CoolifyStartupValidator.Format(["BaseUrl is required."])
            .Should().StartWith("Invalid Coolify configuration (1 problem):");
    }
}
