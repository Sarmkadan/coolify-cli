using CoolifyCli.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CoolifyCli.Tests;

public class CoolifyOptionsValidatorTests
{
    private const string RealToken = "9f2c1a7e-4b3d-4e5f-8a6b-0c1d2e3f4a5b";

    private readonly CoolifyOptionsValidator _validator = new();

    private static CoolifyOptions ValidOptions() => new()
    {
        BaseUrl = "https://coolify.example.com",
        ApiToken = RealToken,
        TimeoutSeconds = 30
    };

    private ValidateOptionsResult Validate(CoolifyOptions options) => _validator.Validate(null, options);

    [Fact]
    public void Validate_WithValidOptions_Succeeds()
    {
        Validate(ValidOptions()).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://coolify.example.com")]
    [InlineData("https://coolify.example.com/")]
    [InlineData("http://localhost:8000")]
    [InlineData("  https://coolify.example.com  ")]
    public void Validate_WithAbsoluteHttpOrHttpsBaseUrlWithoutPath_Succeeds(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        Validate(options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingBaseUrl_ReportsRequiredAndNamesVariable(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("BaseUrl is required").And.Contain("COOLIFY_URL");
    }

    [Theory]
    [InlineData("coolify.example.com")]
    [InlineData("/relative/path")]
    [InlineData("https://")]
    public void Validate_WithBaseUrlThatIsNotAbsolute_ReportsAbsoluteUrlRule(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().Be("BaseUrl must be an absolute URL such as https://coolify.example.com.");
    }

    [Theory]
    [InlineData("ftp://coolify.example.com")]
    [InlineData("ws://coolify.example.com")]
    public void Validate_WithNonHttpScheme_ReportsSchemeRule(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().Be("BaseUrl must use the http or https scheme.");
    }

    [Fact]
    public void Validate_WithCredentialsInBaseUrl_ReportsCredentialRuleWithoutEchoingThem()
    {
        var options = ValidOptions();
        options.BaseUrl = "https://deploy-user:hunter2-password@coolify.example.com";

        var failures = Validate(options).Failures!.ToList();

        failures.Should().ContainSingle()
            .Which.Should().Be("BaseUrl must not contain credentials; use ApiToken instead.");
        failures.Should().NotContain(f => f.Contains("hunter2-password"));
    }

    [Theory]
    [InlineData("https://coolify.example.com/?token=abc")]
    [InlineData("https://coolify.example.com/#section")]
    public void Validate_WithQueryOrFragment_ReportsQueryRule(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().Be("BaseUrl must not contain a query string or fragment.");
    }

    [Theory]
    [InlineData("https://coolify.example.com/api/v1")]
    [InlineData("https://coolify.example.com/api/v1/")]
    [InlineData("https://coolify.example.com/coolify")]
    public void Validate_WithPathOnBaseUrl_ReportsPathRule(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().StartWith("BaseUrl must not contain a path");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingApiToken_ReportsRequiredAndNamesVariable(string token)
    {
        var options = ValidOptions();
        options.ApiToken = token;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().StartWith("ApiToken is required").And.Contain("COOLIFY_TOKEN");
    }

    [Theory]
    [InlineData("changeme")]
    [InlineData("CHANGE-ME")]
    [InlineData("your-token-here")]
    [InlineData("your_api_token")]
    [InlineData("replace-me")]
    [InlineData("xxx")]
    [InlineData("TODO")]
    [InlineData("<your-token>")]
    [InlineData("${COOLIFY_TOKEN}")]
    [InlineData("$COOLIFY_TOKEN")]
    public void Validate_WithPlaceholderToken_ReportsPlaceholderRule(string token)
    {
        var options = ValidOptions();
        options.ApiToken = token;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().StartWith("ApiToken looks like a placeholder value");
    }

    [Theory]
    [InlineData(RealToken)]
    [InlineData("  " + RealToken + "  ")]
    [InlineData("tok_live_4f8a2c9d")]
    public void Validate_WithRealLookingToken_Succeeds(string token)
    {
        var options = ValidOptions();
        options.ApiToken = token;

        Validate(options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(600)]
    public void Validate_WithTimeoutAtOrWithinBounds_Succeeds(int timeoutSeconds)
    {
        var options = ValidOptions();
        options.TimeoutSeconds = timeoutSeconds;

        Validate(options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(601)]
    [InlineData(3600)]
    public void Validate_WithTimeoutOutsideBounds_ReportsTimeoutRuleWithValue(int timeoutSeconds)
    {
        var options = ValidOptions();
        options.TimeoutSeconds = timeoutSeconds;

        Validate(options).Failures.Should().ContainSingle()
            .Which.Should().Be(
                $"TimeoutSeconds must be between 1 and 600 (got {timeoutSeconds}). " +
                "Set COOLIFY_TIMEOUT, --coolify-timeout, or Coolify:TimeoutSeconds in the config file.");
    }

    [Fact]
    public void Validate_WithDefaultProjectUuidUnset_Succeeds()
    {
        var options = ValidOptions();
        options.DefaultProjectUuid = null;

        Validate(options).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSeveralProblems_ReportsEachOfThem()
    {
        var options = new CoolifyOptions
        {
            BaseUrl = "ftp://coolify.example.com",
            ApiToken = "changeme",
            TimeoutSeconds = 0
        };

        Validate(options).Failures.Should().HaveCount(3);
    }

    [Fact]
    public void Validate_WithInvalidOptions_NeverEchoesTokenValue()
    {
        const string secret = "s3cr3t-token-VALUE-123";
        var options = new CoolifyOptions
        {
            BaseUrl = "ftp://coolify.example.com",
            ApiToken = secret,
            TimeoutSeconds = 0
        };

        var failures = Validate(options).Failures!.ToList();

        failures.Should().HaveCount(2);
        failures.Should().NotContain(f => f.Contains(secret));
    }
}
