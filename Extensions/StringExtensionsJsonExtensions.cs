#nullable enable

namespace CoolifyCli.Extensions;

/// <summary>
/// Provides System.Text.Json serialization/deserialization extensions for strings.
/// These methods extend the functionality of <see cref="StringExtensions"/> by adding JSON serialization capabilities.
/// </summary>
public static class StringExtensionsJsonExtensions
{
	private static readonly System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes a string to a JSON string.
	/// </summary>
	/// <param name="value">The string value to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the value.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this string value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return value.Length == 0
			? "\"\""
			: System.Text.Json.JsonSerializer.Serialize(value, indented
				? new System.Text.Json.JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
					: _jsonSerializerOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a string value.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized string value, or null if the JSON is null or empty.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static string? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return json.Length == 0
			? null
			: System.Text.Json.JsonSerializer.Deserialize<string>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a string value.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized string value, or null if deserialization fails.</param>
	/// <returns>True if deserialization succeeds; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out string? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = System.Text.Json.JsonSerializer.Deserialize<string>(json, _jsonSerializerOptions);
			return true;
		}
		catch (System.Text.Json.JsonException)
		{
			return false;
		}
	}
}