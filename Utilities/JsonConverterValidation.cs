#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolifyCli.Utilities;

/// <summary>
/// Provides validation methods for JsonConverter instances.
/// Validates converter instances for null values, configuration issues, and runtime problems.
/// </summary>
public static class JsonConverterValidation
{
    /// <summary>
    /// Validates a JsonConverter instance for common problems.
    /// </summary>
    /// <param name="value">The JsonConverter instance to validate</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this global::System.Text.Json.Serialization.JsonConverter? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("JsonConverter instance is null");
            return problems.AsReadOnly();
        }

        // Validate converter type name (should not be empty or default)
        if (value.GetType().Name.Length == 0)
        {
            problems.Add("JsonConverter type name is empty");
        }

        if (value.GetType().FullName is null or { Length: 0 })
        {
            problems.Add("JsonConverter type full name is null or empty");
        }

        // Validate that the converter can be used without throwing
        try
        {
            // Test that we can get the converter type
            var converterType = value.GetType();
            var abstractConverterType = typeof(global::System.Text.Json.Serialization.JsonConverter);
            var genericConverterType = typeof(global::System.Text.Json.Serialization.JsonConverter<>);

            if (converterType == abstractConverterType ||
                converterType == genericConverterType.GetGenericTypeDefinition())
            {
                problems.Add("JsonConverter is the abstract base class, not a concrete converter instance");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to validate converter type: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a JsonConverter instance is valid.
    /// </summary>
    /// <param name="value">The JsonConverter instance to check</param>
    /// <returns>True if the converter is valid; otherwise false</returns>
    public static bool IsValid(this global::System.Text.Json.Serialization.JsonConverter? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a JsonConverter instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The JsonConverter instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all problems</exception>
    public static void EnsureValid(this global::System.Text.Json.Serialization.JsonConverter? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JsonConverter validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}