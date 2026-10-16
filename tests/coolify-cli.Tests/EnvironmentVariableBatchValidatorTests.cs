using FluentAssertions;
using Xunit;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for the fail-fast rules applied to batches of environment variables.
/// </summary>
public class EnvironmentVariableBatchValidatorTests
{
    /// <summary>
    /// Tests that POSIX-compliant keys produce no violations.
    /// </summary>
    [Theory]
    [InlineData("A")]
    [InlineData("_")]
    [InlineData("_A")]
    [InlineData("DATABASE_URL")]
    [InlineData("a1_b2")]
    [InlineData("Mixed_Case_123")]
    public void Validate_ShouldAcceptPosixNames(string key)
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry(key, "v")]);

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that keys outside the POSIX name rule are reported as InvalidKeyName.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("1ABC")]
    [InlineData("A-B")]
    [InlineData("A.B")]
    [InlineData("A B")]
    [InlineData("invalid-key!")]
    [InlineData("ÄBC")]
    public void Validate_ShouldRejectNonPosixNames(string key)
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry(key, "v")]);

        violations.Should().ContainSingle()
            .Which.Rule.Should().Be(EnvVarViolationRule.InvalidKeyName);
    }

    /// <summary>
    /// Tests that Coolify-managed SERVICE_* keys are rejected by default.
    /// </summary>
    [Theory]
    [InlineData("SERVICE_FQDN_WEB")]
    [InlineData("SERVICE_URL_API")]
    [InlineData("SERVICE_USER_ADMIN")]
    [InlineData("SERVICE_PASSWORD_DB")]
    [InlineData("SERVICE_BASE64_SECRET")]
    public void Validate_ShouldRejectManagedKeys_ByDefault(string key)
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry(key, "v")]);

        violations.Should().ContainSingle()
            .Which.Rule.Should().Be(EnvVarViolationRule.ManagedKey);
    }

    /// <summary>
    /// Tests that managed keys pass when the caller explicitly allows them.
    /// </summary>
    [Fact]
    public void Validate_ShouldAcceptManagedKeys_WhenAllowed()
    {
        var violations = EnvironmentVariableBatchValidator.Validate(
            [new EnvVarEntry("SERVICE_FQDN_WEB", "example.com")],
            allowManagedKeys: true);

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that only the reserved prefixes are matched, and that matching is case-sensitive.
    /// </summary>
    [Theory]
    [InlineData("SERVICE_FQDNX")]
    [InlineData("SERVICE_URLS")]
    [InlineData("MY_SERVICE_URL_APP")]
    [InlineData("service_fqdn_web")]
    public void Validate_ShouldNotReserveLookalikeKeys(string key)
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry(key, "v")]);

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a raw newline in a value is reported, for both LF and CRLF.
    /// </summary>
    [Theory]
    [InlineData("line1\nline2")]
    [InlineData("line1\r\nline2")]
    [InlineData("line1\rline2")]
    public void Validate_ShouldRejectUnescapedNewlines(string value)
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry("MULTI", value)]);

        violations.Should().ContainSingle()
            .Which.Rule.Should().Be(EnvVarViolationRule.NewlineInValue);
    }

    /// <summary>
    /// Tests that the escaped two-character sequence backslash-n is accepted as a value.
    /// </summary>
    [Fact]
    public void Validate_ShouldAcceptEscapedNewlineSequence()
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry("MULTI", "line1\\nline2")]);

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a repeated key is reported on its second occurrence and points at both lines.
    /// </summary>
    [Fact]
    public void Validate_ShouldReportDuplicateKey_WithBothLineNumbers()
    {
        var entries = new List<EnvVarEntry> {
            new("FOO", "1", 3),
            new("BAR", "2", 4),
            new("FOO", "3", 7)
        };

        var violations = EnvironmentVariableBatchValidator.Validate(entries);

        var violation = violations.Should().ContainSingle().Subject;
        violation.Rule.Should().Be(EnvVarViolationRule.DuplicateKey);
        violation.Key.Should().Be("FOO");
        violation.LineNumber.Should().Be(7);
        violation.Message.Should().Contain("line 7").And.Contain("line 3");
    }

    /// <summary>
    /// Tests that keys differing only by case are not duplicates, because POSIX names are case-sensitive.
    /// </summary>
    [Fact]
    public void Validate_ShouldTreatKeysDifferingByCaseAsDistinct()
    {
        var violations = EnvironmentVariableBatchValidator.Validate([
            new EnvVarEntry("foo", "1"),
            new EnvVarEntry("FOO", "2")
        ]);

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that every violation in the batch is returned in input order, with line numbers
    /// for file entries, rather than stopping at the first problem.
    /// </summary>
    [Fact]
    public void Validate_ShouldAggregateAllViolations_InInputOrder()
    {
        var entries = new List<EnvVarEntry> {
            new("GOOD", "ok", 1),
            new("1BAD", "x", 2),
            new("SERVICE_URL_APP", "x", 3),
            new("MULTI", "a\nb", 4),
            new("GOOD", "again", 5),
            new("", "empty-key", 6)
        };

        var violations = EnvironmentVariableBatchValidator.Validate(entries);

        violations.Select(v => (v.Rule, v.LineNumber)).Should().Equal(
            (EnvVarViolationRule.InvalidKeyName, (int?)2),
            (EnvVarViolationRule.ManagedKey, (int?)3),
            (EnvVarViolationRule.NewlineInValue, (int?)4),
            (EnvVarViolationRule.DuplicateKey, (int?)5),
            (EnvVarViolationRule.InvalidKeyName, (int?)6));
    }

    /// <summary>
    /// Tests that a key breaking both the name and newline rules reports both violations.
    /// </summary>
    [Fact]
    public void Validate_ShouldReportMultipleRulesForOneEntry()
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry("A B", "x\ny", 9)]);

        violations.Select(v => v.Rule).Should().Equal(
            EnvVarViolationRule.InvalidKeyName,
            EnvVarViolationRule.NewlineInValue);
    }

    /// <summary>
    /// Tests that entries without a line number are labelled as coming from --var.
    /// </summary>
    [Fact]
    public void Validate_ShouldLabelCommandLineEntries()
    {
        var violations = EnvironmentVariableBatchValidator.Validate([new EnvVarEntry("1BAD", "x")]);

        violations.Should().ContainSingle()
            .Which.Message.Should().StartWith("--var:");
    }

    /// <summary>
    /// Tests that violation messages name the key but never echo the value.
    /// </summary>
    [Fact]
    public void Validate_ShouldNotIncludeValuesInMessages()
    {
        const string secret = "s3cr3t-value-123";

        var violations = EnvironmentVariableBatchValidator.Validate([
            new EnvVarEntry("API_TOKEN", $"{secret}\nmore", 1),
            new EnvVarEntry("API_TOKEN", secret, 2)
        ]);

        violations.Should().HaveCount(2);
        foreach (var violation in violations)
            violation.Message.Should().NotContain(secret);
    }

    /// <summary>
    /// Tests that ValidateOrThrow does nothing for a valid batch.
    /// </summary>
    [Fact]
    public void ValidateOrThrow_ShouldNotThrow_WhenBatchIsValid()
    {
        var act = () => EnvironmentVariableBatchValidator.ValidateOrThrow([new EnvVarEntry("OK_KEY", "v", 1)]);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that ValidateOrThrow throws one exception carrying every violation.
    /// </summary>
    [Fact]
    public void ValidateOrThrow_ShouldThrowAggregatedException_WithAllViolations()
    {
        var entries = new List<EnvVarEntry> {
            new("1BAD", "x", 1),
            new("OK", "y", 2),
            new("OK", "z", 3)
        };

        var act = () => EnvironmentVariableBatchValidator.ValidateOrThrow(entries);

        var ex = act.Should().Throw<EnvVarValidationException>().Which;
        ex.Violations.Should().HaveCount(2);
        ex.ErrorCode.Should().Be("ENV_VAR_VALIDATION_FAILED");
        ex.ContextData["violationCount"].Should().Be("2");
        ex.Message.Should().Contain("2 violation(s)");
    }

    /// <summary>
    /// Tests that the dedicated CLI exit code is 10 and differs from every other exit code.
    /// </summary>
    [Fact]
    public void EnvVarValidationError_ExitCode_ShouldBeDistinct()
    {
        var others = new[]
        {
            Constants.ExitCodes.Success,
            Constants.ExitCodes.GeneralError,
            Constants.ExitCodes.InvalidArguments,
            Constants.ExitCodes.ConfigurationError,
            Constants.ExitCodes.ApiError,
            Constants.ExitCodes.Timeout,
            Constants.ExitCodes.ValidationError,
            Constants.ExitCodes.UnhandledError,
            Constants.ExitCodes.UnauthorizedAccess,
            Constants.ExitCodes.TimeoutError
        };

        Constants.ExitCodes.EnvVarValidationError.Should().Be(10);
        others.Should().NotContain(Constants.ExitCodes.EnvVarValidationError);
    }
}
