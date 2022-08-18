#nullable enable

using System;
using System.Collections.Generic;
using CoolifyCli.Extensions;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

/// <summary>
/// Validation helpers for enum values used in EnumExtensionsTests.
/// Provides validation methods for testing enum extension methods.
/// </summary>
public static class EnumExtensionsTestsValidation
{
    /// <summary>
    /// Validates a DeploymentStatus enum value and returns a list of human-readable problems.
    /// Checks for null values, out-of-range numbers, and default/uninitialized values.
    /// </summary>
    /// <param name="value">The DeploymentStatus value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this DeploymentStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the enum value is within expected range
        // DeploymentStatus has values from 0 to 14 (15 members)
        var intValue = (int)value;
        if (intValue < 0 || intValue > 14)
        {
            problems.Add($"DeploymentStatus value '{value}' has underlying integer value {intValue} which is out of expected range [0, 14]");
        }

        // Validate that the enum is properly defined
        if (!Enum.IsDefined(typeof(DeploymentStatus), value))
        {
            problems.Add($"DeploymentStatus value '{value}' is not a defined enum member");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified DeploymentStatus value is valid.
    /// </summary>
    /// <param name="value">The DeploymentStatus value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this DeploymentStatus value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified DeploymentStatus value is valid.
    /// </summary>
    /// <param name="value">The DeploymentStatus value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this DeploymentStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DeploymentStatus value '{value}' is not valid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a DatabaseType enum value and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The DatabaseType value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseType value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the enum value is within expected range
        // DatabaseType has values from 0 to 5 (6 members)
        var intValue = (int)value;
        if (intValue < 0 || intValue > 5)
        {
            problems.Add($"DatabaseType value '{value}' has underlying integer value {intValue} which is out of expected range [0, 5]");
        }

        // Validate that the enum is properly defined
        if (!Enum.IsDefined(typeof(DatabaseType), value))
        {
            problems.Add($"DatabaseType value '{value}' is not a defined enum member");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified DatabaseType value is valid.
    /// </summary>
    /// <param name="value">The DatabaseType value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this DatabaseType value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified DatabaseType value is valid.
    /// </summary>
    /// <param name="value">The DatabaseType value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this DatabaseType value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DatabaseType value '{value}' is not valid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a RuntimeEnvironment enum value and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The RuntimeEnvironment value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this RuntimeEnvironment value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the enum value is within expected range
        // RuntimeEnvironment has values from 0 to 7 (8 members)
        var intValue = (int)value;
        if (intValue < 0 || intValue > 7)
        {
            problems.Add($"RuntimeEnvironment value '{value}' has underlying integer value {intValue} which is out of expected range [0, 7]");
        }

        // Validate that the enum is properly defined
        if (!Enum.IsDefined(typeof(RuntimeEnvironment), value))
        {
            problems.Add($"RuntimeEnvironment value '{value}' is not a defined enum member");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified RuntimeEnvironment value is valid.
    /// </summary>
    /// <param name="value">The RuntimeEnvironment value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this RuntimeEnvironment value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified RuntimeEnvironment value is valid.
    /// </summary>
    /// <param name="value">The RuntimeEnvironment value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this RuntimeEnvironment value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RuntimeEnvironment value '{value}' is not valid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a SeverityLevel enum value and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The SeverityLevel value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this SeverityLevel value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the enum value is within expected range
        // SeverityLevel has values from 0 to 4 (5 members)
        var intValue = (int)value;
        if (intValue < 0 || intValue > 4)
        {
            problems.Add($"SeverityLevel value '{value}' has underlying integer value {intValue} which is out of expected range [0, 4]");
        }

        // Validate that the enum is properly defined
        if (!Enum.IsDefined(typeof(SeverityLevel), value))
        {
            problems.Add($"SeverityLevel value '{value}' is not a defined enum member");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified SeverityLevel value is valid.
    /// </summary>
    /// <param name="value">The SeverityLevel value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this SeverityLevel value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified SeverityLevel value is valid.
    /// </summary>
    /// <param name="value">The SeverityLevel value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this SeverityLevel value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SeverityLevel value '{value}' is not valid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a ScalingPolicy enum value and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The ScalingPolicy value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this ScalingPolicy value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the enum value is within expected range
        // ScalingPolicy has values from 0 to 4 (5 members)
        var intValue = (int)value;
        if (intValue < 0 || intValue > 4)
        {
            problems.Add($"ScalingPolicy value '{value}' has underlying integer value {intValue} which is out of expected range [0, 4]");
        }

        // Validate that the enum is properly defined
        if (!Enum.IsDefined(typeof(ScalingPolicy), value))
        {
            problems.Add($"ScalingPolicy value '{value}' is not a defined enum member");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified ScalingPolicy value is valid.
    /// </summary>
    /// <param name="value">The ScalingPolicy value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this ScalingPolicy value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified ScalingPolicy value is valid.
    /// </summary>
    /// <param name="value">The ScalingPolicy value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this ScalingPolicy value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ScalingPolicy value '{value}' is not valid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a BackupStrategy enum value and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The BackupStrategy value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this BackupStrategy value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the enum value is within expected range
        // BackupStrategy has values from 0 to 3 (4 members)
        var intValue = (int)value;
        if (intValue < 0 || intValue > 3)
        {
            problems.Add($"BackupStrategy value '{value}' has underlying integer value {intValue} which is out of expected range [0, 3]");
        }

        // Validate that the enum is properly defined
        if (!Enum.IsDefined(typeof(BackupStrategy), value))
        {
            problems.Add($"BackupStrategy value '{value}' is not a defined enum member");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified BackupStrategy value is valid.
    /// </summary>
    /// <param name="value">The BackupStrategy value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this BackupStrategy value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified BackupStrategy value is valid.
    /// </summary>
    /// <param name="value">The BackupStrategy value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this BackupStrategy value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BackupStrategy value '{value}' is not valid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }
}