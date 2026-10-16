using CoolifyCli.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CoolifyCli.Tests;

public class CoolifyOptionsConfigurationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "coolify-options-" + Guid.NewGuid());

    public CoolifyOptionsConfigurationTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private string ConfigFilePath => Path.Combine(_directory, "config.json");

    /// <summary>Environment lookup backed by a fixed set of variables, so tests never read the real process environment.</summary>
    private static Func<string, string?> Env(params (string Name, string Value)[] variables)
    {
        var map = variables.ToDictionary(v => v.Name, v => v.Value);
        return name => map.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>Points the config file at a path that does not exist, so no real user config is read.</summary>
    private static string[] NoFile(params string[] args) => ["--coolify-config", "/nonexistent/coolify/config.json", .. args];

    private static CoolifyOptions Bind(IConfiguration configuration) =>
        configuration.GetSection(CoolifyOptions.SectionName).Get<CoolifyOptions>() ?? new CoolifyOptions();

    private void WriteConfigFile(string json) => File.WriteAllText(ConfigFilePath, json);

    [Fact]
    public void Build_WithNoSources_UsesDefaults()
    {
        var options = Bind(CoolifyOptionsConfiguration.Build(NoFile(), Env()));

        options.BaseUrl.Should().Be("https://api.coolify.io");
        options.ApiToken.Should().BeEmpty();
        options.TimeoutSeconds.Should().Be(30);
        options.DefaultProjectUuid.Should().BeNull();
    }

    [Fact]
    public void Build_ReadsValuesFromConfigFile()
    {
        WriteConfigFile("""
            {
              "Coolify": {
                "BaseUrl": "https://file.example.com",
                "ApiToken": "file-token-123",
                "TimeoutSeconds": 45,
                "DefaultProjectUuid": "project-from-file"
              }
            }
            """);

        var options = Bind(CoolifyOptionsConfiguration.Build(
            ["--coolify-config", ConfigFilePath], Env()));

        options.BaseUrl.Should().Be("https://file.example.com");
        options.ApiToken.Should().Be("file-token-123");
        options.TimeoutSeconds.Should().Be(45);
        options.DefaultProjectUuid.Should().Be("project-from-file");
    }

    [Fact]
    public void Build_EnvironmentVariablesOverrideConfigFile()
    {
        WriteConfigFile("""{ "Coolify": { "BaseUrl": "https://file.example.com", "ApiToken": "file-token", "TimeoutSeconds": 45 } }""");

        var options = Bind(CoolifyOptionsConfiguration.Build(
            ["--coolify-config", ConfigFilePath],
            Env(
                ("COOLIFY_URL", "https://env.example.com"),
                ("COOLIFY_TOKEN", "env-token"),
                ("COOLIFY_TIMEOUT", "90"),
                ("COOLIFY_PROJECT_UUID", "project-from-env"))));

        options.BaseUrl.Should().Be("https://env.example.com");
        options.ApiToken.Should().Be("env-token");
        options.TimeoutSeconds.Should().Be(90);
        options.DefaultProjectUuid.Should().Be("project-from-env");
    }

    [Fact]
    public void Build_CommandLineFlagsOverrideEnvironmentAndConfigFile()
    {
        WriteConfigFile("""{ "Coolify": { "BaseUrl": "https://file.example.com", "ApiToken": "file-token", "TimeoutSeconds": 45 } }""");

        var options = Bind(CoolifyOptionsConfiguration.Build(
            ["--coolify-config", ConfigFilePath, "--coolify-url", "https://cli.example.com", "--coolify-token=cli-token", "--coolify-timeout", "120", "--coolify-project", "project-from-cli"],
            Env(
                ("COOLIFY_URL", "https://env.example.com"),
                ("COOLIFY_TOKEN", "env-token"),
                ("COOLIFY_TIMEOUT", "90"))));

        options.BaseUrl.Should().Be("https://cli.example.com");
        options.ApiToken.Should().Be("cli-token");
        options.TimeoutSeconds.Should().Be(120);
        options.DefaultProjectUuid.Should().Be("project-from-cli");
    }

    [Fact]
    public void Build_WithCurrentAndLegacyEnvironmentNames_PrefersCurrentNames()
    {
        var options = Bind(CoolifyOptionsConfiguration.Build(
            NoFile(),
            Env(
                ("COOLIFY_URL", "https://new.example.com"),
                ("COOLIFY_API_URL", "https://legacy.example.com"),
                ("COOLIFY_TOKEN", "new-token"),
                ("COOLIFY_API_KEY", "legacy-token"))));

        options.BaseUrl.Should().Be("https://new.example.com");
        options.ApiToken.Should().Be("new-token");
    }

    [Fact]
    public void Build_WithOnlyLegacyEnvironmentNames_UsesThem()
    {
        var options = Bind(CoolifyOptionsConfiguration.Build(
            NoFile(),
            Env(
                ("COOLIFY_API_URL", "https://legacy.example.com"),
                ("COOLIFY_API_KEY", "legacy-token"))));

        options.BaseUrl.Should().Be("https://legacy.example.com");
        options.ApiToken.Should().Be("legacy-token");
    }

    [Fact]
    public void Build_WithBlankEnvironmentVariable_FallsBackToConfigFile()
    {
        WriteConfigFile("""{ "Coolify": { "ApiToken": "file-token" } }""");

        var options = Bind(CoolifyOptionsConfiguration.Build(
            ["--coolify-config", ConfigFilePath],
            Env(("COOLIFY_TOKEN", "   "))));

        options.ApiToken.Should().Be("file-token");
    }

    [Fact]
    public void Build_IgnoresArgumentsThatAreNotCoolifyFlags()
    {
        var options = Bind(CoolifyOptionsConfiguration.Build(
            NoFile("app", "list", "--verbose", "--coolify-token", "cli-token", "--no-color"),
            Env()));

        options.ApiToken.Should().Be("cli-token");
        options.BaseUrl.Should().Be("https://api.coolify.io");
    }

    [Fact]
    public void Build_ReadsConfigFilePathFromEnvironmentWhenNoFlagIsGiven()
    {
        WriteConfigFile("""{ "Coolify": { "ApiToken": "from-env-path" } }""");

        var options = Bind(CoolifyOptionsConfiguration.Build(
            Array.Empty<string>(),
            Env(("COOLIFY_CONFIG_FILE", ConfigFilePath))));

        options.ApiToken.Should().Be("from-env-path");
    }

    [Fact]
    public void Build_ReadsConfigFilePathFromEqualsForm()
    {
        WriteConfigFile("""{ "Coolify": { "ApiToken": "from-equals-form" } }""");

        var options = Bind(CoolifyOptionsConfiguration.Build(
            [$"--coolify-config={ConfigFilePath}"],
            Env()));

        options.ApiToken.Should().Be("from-equals-form");
    }

    [Fact]
    public void Build_WithMalformedConfigFile_ThrowsInvalidDataException()
    {
        WriteConfigFile("{ \"Coolify\": { \"ApiToken\": ");

        var act = () => CoolifyOptionsConfiguration.Build(["--coolify-config", ConfigFilePath], Env());

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Build_WithFlagMissingItsValue_LeavesValueEmptyForValidationToReject()
    {
        var options = Bind(CoolifyOptionsConfiguration.Build(NoFile("--coolify-token"), Env()));

        options.ApiToken.Should().BeEmpty();
    }
}
