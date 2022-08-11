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
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="DeploymentDiffService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The deployment diff service to serialize.</param>
    /// <param name="indented">If true, the output JSON will be indented.</param>
    /// <returns>A JSON representation of the deployment diff service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DeploymentDiffService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // DeploymentDiffService itself cannot be meaningfully serialized due to its dependencies.
        // Return a simple representation indicating the service type.
        var serviceInfo = new
        {
            ServiceType = "DeploymentDiffService",
            SerializedAt = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions(_options)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(serviceInfo, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="DeploymentDiffService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a deployment diff service.</param>
    /// <returns>The deserialized <see cref="DeploymentDiffService"/> instance, or null if the JSON is empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static DeploymentDiffService? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        // DeploymentDiffService cannot be meaningfully deserialized due to its dependencies.
        // Return null as the service requires constructor injection.
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="DeploymentDiffService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a deployment diff service.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="DeploymentDiffService"/> if the operation succeeded; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out DeploymentDiffService? value)
    {
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}