using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoolifyCli.Models
{
    /// <summary>
    /// Provides JSON serialization and deserialization helpers for <see cref="LogEntry"/>.
    /// </summary>
    public static class LogEntryJsonExtensions
    {
        // Cached options: camelCase naming policy, ignore null values.
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the <see cref="LogEntry"/> to a JSON string.
        /// </summary>
        /// <param name="value">The log entry to serialize.</param>
        /// <param name="indented">If true, the output JSON will be indented.</param>
        /// <returns>A JSON representation of the log entry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this LogEntry value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            // Create a temporary options instance that copies the base options and sets WriteIndented.
            var options = new JsonSerializerOptions(_options)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="json">The JSON string representing a log entry.</param>
        /// <returns>The deserialized <see cref="LogEntry"/> instance, or null if the JSON is empty or whitespace.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
        public static LogEntry? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<LogEntry>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="json">The JSON string representing a log entry.</param>
        /// <param name="value">When this method returns, contains the deserialized <see cref="LogEntry"/> if the operation succeeded; otherwise, null.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out LogEntry? value)
        {
            ArgumentNullException.ThrowIfNull(json);

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
}
