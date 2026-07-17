using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="DeploymentDiffService"/>.
/// </summary>
public static class DeploymentDiffServiceJsonExtensions
{
    // Cached options: camelCase naming policy, ignore null values.
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="DeploymentDiffService"/> instance to a JSON string.
    /// </summary>
    /// <remarks>
    /// This method serializes metadata about the service rather than the service itself,
    /// as <see cref="DeploymentDiffService"/> contains dependencies that cannot be meaningfully serialized.
    /// </remarks>
    /// <param name="value">The deployment diff service to serialize.</param>
    /// <param name="indented">If true, the output JSON will be indented.</param>
    /// <returns>A JSON representation containing service metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DeploymentDiffService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var serviceInfo = new
        {
            ServiceType = "DeploymentDiffService",
            SerializedAt = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(
            serviceInfo,
            new JsonSerializerOptions(_options) { WriteIndented = indented }
        );
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="DeploymentDiffService"/> instance.
    /// </summary>
    /// <remarks>
    /// <see cref="DeploymentDiffService"/> cannot be meaningfully deserialized due to constructor injection requirements.
    /// This method always returns null.
    /// </remarks>
    /// <param name="json">The JSON string representing a deployment diff service.</param>
    /// <returns>Always null, as the service requires constructor injection.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static DeploymentDiffService? FromJson(string json)
    {
        _ = string.IsNullOrWhiteSpace(json);
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="DeploymentDiffService"/> instance.
    /// </summary>
    /// <remarks>
    /// This method always returns false and sets <paramref name="value"/> to null,
    /// as <see cref="DeploymentDiffService"/> cannot be deserialized.
    /// </remarks>
    /// <param name="json">The JSON string representing a deployment diff service.</param>
    /// <param name="value">When this method returns, contains null.</param>
    /// <returns>Always false.</returns>
    public static bool TryFromJson(string json, out DeploymentDiffService? value)
    {
        value = null;
        return false;
    }
}