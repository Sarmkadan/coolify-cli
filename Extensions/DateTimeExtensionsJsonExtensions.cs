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
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
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
            // Adjust the WriteIndented flag based on the caller's request.
            _options.WriteIndented = indented;
            return JsonSerializer.Serialize(dateTime, _options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="DateTime"/>.</param>
        /// <returns>The deserialized <see cref="DateTime"/> value, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
        public static DateTime? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<DateTime>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="DateTime"/>.</param>
        /// <param name="value">When this method returns, contains the deserialized <see cref="DateTime"/> if the operation succeeded; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
        public static bool TryFromJson(string json, out DateTime? value)
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
}
