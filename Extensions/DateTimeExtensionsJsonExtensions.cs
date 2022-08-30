#nullable enable
using System;
using System.Text.Json;

namespace CoolifyCli.Extensions
{
    /// <summary>
    /// JSON serialization helpers for <see cref="DateTime"/> values.
    /// </summary>
    public static class DateTimeExtensionsJsonExtensions
    {
        // Cached serializer options – camelCase naming, can be reused for all calls.
        // Made immutable to ensure thread safety when reused across multiple calls.
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the <see cref="DateTime"/> value to JSON.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> value to serialize.</param>
        /// <param name="indented">If true, the JSON output will be indented.</param>
        /// <returns>A JSON string representing the <see cref="DateTime"/>.</returns>
        public static string ToJson(this DateTime dateTime, bool indented = false)
        {
            // Create a new options instance to avoid mutating the shared cached options.
            var options = new JsonSerializerOptions(_options)
            {
                WriteIndented = indented
            };
            return JsonSerializer.Serialize(dateTime, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="DateTime"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException"><paramref name="json"/> is not a valid JSON representation of a <see cref="DateTime"/>.</exception>
        /// <returns>The deserialized <see cref="DateTime"/> value, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
        public static DateTime? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<DateTime>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="DateTime"/>.</param>
        /// <param name="value">When this method returns, contains the deserialized <see cref="DateTime"/> if the operation succeeded; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson(string json, out DateTime? value)
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