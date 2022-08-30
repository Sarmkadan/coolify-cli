#nullable enable
using System.ComponentModel;
using System.Reflection;

namespace CoolifyCli.Extensions;

/// <summary>
/// Extension methods for Enum manipulation.
/// Provides utilities for enum descriptions, parsing, and display formatting.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the Description attribute value of an enum member.
    /// Falls back to enum name if no Description attribute is found.
    /// </summary>
    /// <param name="value">The enum value to get the description for.</param>
    /// <returns>The description attribute value if present, otherwise the enum name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string GetDescription(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());

        if (field is null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Converts an enum value to a human-readable display string.
    /// Removes underscores, capitalizes words, and applies proper formatting.
    /// </summary>
    /// <param name="value">The enum value to convert.</param>
    /// <returns>A human-readable display string with underscores replaced by spaces and proper capitalization.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToDisplayString(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var description = value.GetDescription();

        // Handle empty or whitespace-only descriptions
        if (string.IsNullOrWhiteSpace(description))
            return value.ToString();

        // Replace underscores with spaces and capitalize each word
        return string.Join(" ",
            description.Split('_')
                .Where(word => !string.IsNullOrEmpty(word))
                .Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
    }

    /// <summary>
    /// Parses a string to an enum value of type T.
    /// Case-insensitive matching.
    /// </summary>
    /// <typeparam name="T">The enum type to parse.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or not a valid enum value.</exception>
    public static T ParseEnum<T>(this string value) where T : struct, Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            return result;

        throw new ArgumentException($"'{value}' is not a valid value for {typeof(T).Name}");
    }

    /// <summary>
    /// Attempts to parse a string to an enum value of type T.
    /// Returns null if parsing fails.
    /// </summary>
    /// <typeparam name="T">The enum type to parse.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed enum value, or null if parsing fails.</returns>
    public static T? TryParseEnum<T>(this string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : null;
    }

    /// <summary>
    /// Gets all values of an enum type as a list.
    /// </summary>
    /// <typeparam name="T">The enum type to get values for.</typeparam>
    /// <returns>A list containing all enum values of type T.</returns>
    public static List<T> GetAllValues<T>() where T : struct, Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Gets all enum values with their descriptions.
    /// Useful for displaying enum options to users.
    /// </summary>
    /// <typeparam name="T">The enum type to get values for.</typeparam>
    /// <returns>A dictionary mapping enum values to their descriptions.</returns>
    public static Dictionary<T, string> GetValueDescriptionMap<T>() where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(e => e, e => e.GetDescription());
    }

    /// <summary>
    /// Gets all display strings for an enum type.
    /// </summary>
    /// <typeparam name="T">The enum type to get display strings for.</typeparam>
    /// <returns>A list of human-readable display strings for all enum values.</returns>
    public static List<string> GetDisplayStrings<T>() where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => e.ToDisplayString())
            .ToList();
    }

    /// <summary>
    /// Checks if an enum has a specific flag set.
    /// Works with [Flags] enums.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to check.</param>
    /// <param name="flag">The flag to check for.</param>
    /// <returns><see langword="true"/> if the enum has the specified flag set; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool HasFlag<T>(this T value, T flag) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(flag);

        var valueAsInt = Convert.ToInt64(value);
        var flagAsInt = Convert.ToInt64(flag);

        return (valueAsInt & flagAsInt) == flagAsInt;
    }

    /// <summary>
    /// Gets the underlying value of an enum as a long.
    /// Useful for numeric operations on enums.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to convert.</param>
    /// <returns>The underlying numeric value as a long.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static long ToLong<T>(this T value) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        return Convert.ToInt64(value);
    }

    /// <summary>
    /// Gets the underlying value of an enum as an int.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to convert.</param>
    /// <returns>The underlying numeric value as an int.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static int ToInt<T>(this T value) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        return Convert.ToInt32(value);
    }

    /// <summary>
    /// Converts an enum to its kebab-case CLI representation.
    /// Used for command-line argument formatting.
    /// </summary>
    /// <param name="value">The enum value to format.</param>
    /// <returns>A kebab-case formatted string suitable for CLI arguments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToCliFormat(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return KebabCaseHelper.ToKebabCase(value.ToString());
    }

    /// <summary>
    /// Gets a random enum value from type T.
    /// </summary>
    /// <typeparam name="T">The enum type to get a random value from.</typeparam>
    /// <returns>A randomly selected enum value of type T.</returns>
    /// <exception cref="ArgumentException">Thrown when the enum type has no values.</exception>
    public static T GetRandomValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        if (values.Length == 0)
            throw new ArgumentException($"Enum type {typeof(T).Name} has no values.");

        return values[Random.Shared.Next(values.Length)];
    }

    /// <summary>
    /// Checks if an enum value equals another by name (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to check.</param>
    /// <param name="name">The name to compare against.</param>
    /// <returns><see langword="true"/> if the enum value's name equals the provided name (case-insensitive); otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.\n    /// Thrown when <paramref name="name"/> is <see langword="null"/>.\n    /// </exception>
    public static bool EqualsIgnoreCase<T>(this T value, string name) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(name);

        return value.ToString().Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets an attribute of type TAttribute from an enum value.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to retrieve.</typeparam>
    /// <param name="value">The enum value to get the attribute from.</param>
    /// <returns>The attribute instance if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttribute<TAttribute>();
    }
}

/// <summary>
/// Extension method helper for converting to kebab-case.
/// </summary>
internal static class KebabCaseHelper
{
    public static string ToKebabCase(this string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        return string.Concat(value.Select((x, i) =>
            i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLowerInvariant();
    }
}