#nullable enable

using CoolifyCli.Extensions;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for round-tripping behavior of enum values through string parsing.
/// Ensures that enum values can be converted to strings and back without loss of information.
/// </summary>
public class EnumRoundtripTests
{
    /// <summary>
    /// Tests that all DeploymentStatus values can round-trip through string conversion.
    /// </summary>
    [Fact]
    public void DeploymentStatus_Roundtrip_AllValues()
    {
        foreach (DeploymentStatus status in Enum.GetValues(typeof(DeploymentStatus)))
        {
            // Convert enum to string and back
            string statusString = status.ToString();
            DeploymentStatus parsedStatus = statusString.ParseEnum<DeploymentStatus>();

            // Verify round-trip preserves the original value
            parsedStatus.Should().Be(status, $"Failed to round-trip {status}");
        }
    }

    /// <summary>
    /// Tests that all DatabaseType values can round-trip through string conversion.
    /// </summary>
    [Fact]
    public void DatabaseType_Roundtrip_AllValues()
    {
        foreach (DatabaseType dbType in Enum.GetValues(typeof(DatabaseType)))
        {
            string dbTypeString = dbType.ToString();
            DatabaseType parsedDbType = dbTypeString.ParseEnum<DatabaseType>();

            parsedDbType.Should().Be(dbType, $"Failed to round-trip {dbType}");
        }
    }

    /// <summary>
    /// Tests that all RuntimeEnvironment values can round-trip through string conversion.
    /// </summary>
    [Fact]
    public void RuntimeEnvironment_Roundtrip_AllValues()
    {
        foreach (RuntimeEnvironment env in Enum.GetValues(typeof(RuntimeEnvironment)))
        {
            string envString = env.ToString();
            RuntimeEnvironment parsedEnv = envString.ParseEnum<RuntimeEnvironment>();

            parsedEnv.Should().Be(env, $"Failed to round-trip {env}");
        }
    }

    /// <summary>
    /// Tests that all BackupStrategy values can round-trip through string conversion.
    /// </summary>
    [Fact]
    public void BackupStrategy_Roundtrip_AllValues()
    {
        foreach (BackupStrategy strategy in Enum.GetValues(typeof(BackupStrategy)))
        {
            string strategyString = strategy.ToString();
            BackupStrategy parsedStrategy = strategyString.ParseEnum<BackupStrategy>();

            parsedStrategy.Should().Be(strategy, $"Failed to round-trip {strategy}");
        }
    }

    /// <summary>
    /// Tests that all SeverityLevel values can round-trip through string conversion.
    /// </summary>
    [Fact]
    public void SeverityLevel_Roundtrip_AllValues()
    {
        foreach (SeverityLevel level in Enum.GetValues(typeof(SeverityLevel)))
        {
            string levelString = level.ToString();
            SeverityLevel parsedLevel = levelString.ParseEnum<SeverityLevel>();

            parsedLevel.Should().Be(level, $"Failed to round-trip {level}");
        }
    }

    /// <summary>
    /// Tests that all ScalingPolicy values can round-trip through string conversion.
    /// </summary>
    [Fact]
    public void ScalingPolicy_Roundtrip_AllValues()
    {
        foreach (ScalingPolicy policy in Enum.GetValues(typeof(ScalingPolicy)))
        {
            string policyString = policy.ToString();
            ScalingPolicy parsedPolicy = policyString.ParseEnum<ScalingPolicy>();

            parsedPolicy.Should().Be(policy, $"Failed to round-trip {policy}");
        }
    }

    /// <summary>
    /// Tests round-tripping with case-insensitive parsing for DeploymentStatus.
    /// </summary>
    [Fact]
    public void DeploymentStatus_Roundtrip_CaseInsensitive()
    {
        foreach (DeploymentStatus status in Enum.GetValues(typeof(DeploymentStatus)))
        {
            string statusString = status.ToString().ToLowerInvariant();
            DeploymentStatus parsedStatus = statusString.ParseEnum<DeploymentStatus>();

            parsedStatus.Should().Be(status, $"Failed to round-trip {status} with lowercase");
        }
    }

    /// <summary>
    /// Tests round-tripping with mixed-case parsing for DatabaseType.
    /// </summary>
    [Fact]
    public void DatabaseType_Roundtrip_MixedCase()
    {
        foreach (DatabaseType dbType in Enum.GetValues(typeof(DatabaseType)))
        {
            string dbTypeString = char.ToUpperInvariant(dbType.ToString()[0]) + dbType.ToString().Substring(1).ToLowerInvariant();
            DatabaseType parsedDbType = dbTypeString.ParseEnum<DatabaseType>();

            parsedDbType.Should().Be(dbType, $"Failed to round-trip {dbType} with mixed case");
        }
    }

    /// <summary>
    /// Tests that TryParseEnum also supports round-tripping for valid values.
    /// </summary>
    [Fact]
    public void TryParseEnum_Roundtrip_ValidValues()
    {
        foreach (DeploymentStatus status in Enum.GetValues(typeof(DeploymentStatus)))
        {
            string statusString = status.ToString();
            var parsedStatus = statusString.TryParseEnum<DeploymentStatus>();

            parsedStatus.Should().NotBeNull("TryParseEnum should not return null for valid enum value");
            parsedStatus.Should().Be(status, $"Failed to round-trip {status} with TryParseEnum");
        }
    }

    /// <summary>
    /// Tests round-tripping preserves underlying integer values.
    /// </summary>
    [Fact]
    public void Roundtrip_PreservesIntegerValues()
    {
        foreach (DeploymentStatus status in Enum.GetValues(typeof(DeploymentStatus)))
        {
            string statusString = status.ToString();
            DeploymentStatus parsedStatus = statusString.ParseEnum<DeploymentStatus>();

            parsedStatus.ToInt().Should().Be(status.ToInt(), $"Integer values should be preserved for {status}");
        }
    }

    /// <summary>
    /// Tests round-tripping preserves underlying long values.
    /// </summary>
    [Fact]
    public void Roundtrip_PreservesLongValues()
    {
        foreach (DeploymentStatus status in Enum.GetValues(typeof(DeploymentStatus)))
        {
            string statusString = status.ToString();
            DeploymentStatus parsedStatus = statusString.ParseEnum<DeploymentStatus>();

            parsedStatus.ToLong().Should().Be(status.ToLong(), $"Long values should be preserved for {status}");
        }
    }

    /// <summary>
    /// Tests round-tripping for enum values with CLI format conversion.
    /// </summary>
    [Fact]
    public void Roundtrip_WithCliFormat()
    {
        // Test that CLI format can be parsed back
        var status = DeploymentStatus.InProgress;
        string cliFormat = status.ToCliFormat();

        // Parse the kebab-case CLI format back to enum
        DeploymentStatus parsedStatus = cliFormat.ParseEnum<DeploymentStatus>();

        parsedStatus.Should().Be(status, "CLI format should be parseable back to original enum");
    }

    /// <summary>
    /// Tests round-tripping for enum values with display string conversion.
    /// </summary>
    [Fact]
    public void Roundtrip_WithDisplayString()
    {
        // Test that display string can be parsed back (if it matches the enum name)
        var status = DeploymentStatus.Deployed;
        string displayString = status.ToDisplayString();

        // Display string for Deployed should be "Deployed" which matches the enum name
        if (displayString == status.ToString())
        {
            DeploymentStatus parsedStatus = displayString.ParseEnum<DeploymentStatus>();
            parsedStatus.Should().Be(status, "Display string should be parseable back to original enum when it matches enum name");
        }
    }

    /// <summary>
    /// Tests round-tripping all enum types in a single test to ensure no conflicts.
    /// </summary>
    [Fact]
    public void AllEnums_Roundtrip_NoConflicts()
    {
        // DeploymentStatus
        foreach (DeploymentStatus status in Enum.GetValues(typeof(DeploymentStatus)))
        {
            string s = status.ToString();
            s.ParseEnum<DeploymentStatus>().Should().Be(status);
        }

        // DatabaseType
        foreach (DatabaseType db in Enum.GetValues(typeof(DatabaseType)))
        {
            string s = db.ToString();
            s.ParseEnum<DatabaseType>().Should().Be(db);
        }

        // RuntimeEnvironment
        foreach (RuntimeEnvironment env in Enum.GetValues(typeof(RuntimeEnvironment)))
        {
            string s = env.ToString();
            s.ParseEnum<RuntimeEnvironment>().Should().Be(env);
        }

        // BackupStrategy
        foreach (BackupStrategy bs in Enum.GetValues(typeof(BackupStrategy)))
        {
            string s = bs.ToString();
            s.ParseEnum<BackupStrategy>().Should().Be(bs);
        }

        // SeverityLevel
        foreach (SeverityLevel sl in Enum.GetValues(typeof(SeverityLevel)))
        {
            string s = sl.ToString();
            s.ParseEnum<SeverityLevel>().Should().Be(sl);
        }

        // ScalingPolicy
        foreach (ScalingPolicy sp in Enum.GetValues(typeof(ScalingPolicy)))
        {
            string s = sp.ToString();
            s.ParseEnum<ScalingPolicy>().Should().Be(sp);
        }
    }
}
