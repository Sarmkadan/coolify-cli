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
    public static string GetDescription(this Enum value)
    {
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
    public static string ToDisplayString(this Enum value)
    {
        var description = value.GetDescription();

        // Replace underscores with spaces and capitalize each word
        return string.Join(" ",
            description.Split('_')
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Parses a string to an enum value of type T.
    /// Case-insensitive matching.
    /// </summary>
    public static T ParseEnum<T>(this string value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Value cannot be null or empty for enum {typeof(T).Name}");

        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            return result;

        throw new ArgumentException($"'{value}' is not a valid value for {typeof(T).Name}");
    }

    /// <summary>
    /// Attempts to parse a string to an enum value of type T.
    /// Returns null if parsing fails.
    /// </summary>
    public static T? TryParseEnum<T>(this string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : null;
    }

    /// <summary>
    /// Gets all values of an enum type as a list.
    /// </summary>
    public static List<T> GetAllValues<T>() where T : struct, Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Gets all enum values with their descriptions.
    /// Useful for displaying enum options to users.
    /// </summary>
    public static Dictionary<T, string> GetValueDescriptionMap<T>() where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(e => e, e => e.GetDescription());
    }

    /// <summary>
    /// Gets all display strings for an enum type.
    /// </summary>
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
    public static bool HasFlag<T>(this T value, T flag) where T : struct, Enum
    {
        var valueAsInt = Convert.ToInt64(value);
        var flagAsInt = Convert.ToInt64(flag);

        return (valueAsInt & flagAsInt) == flagAsInt;
    }

    /// <summary>
    /// Gets the underlying value of an enum as a long.
    /// Useful for numeric operations on enums.
    /// </summary>
    public static long ToLong<T>(this T value) where T : struct, Enum
    {
        return Convert.ToInt64(value);
    }

    /// <summary>
    /// Gets the underlying value of an enum as an int.
    /// </summary>
    public static int ToInt<T>(this T value) where T : struct, Enum
    {
        return Convert.ToInt32(value);
    }

    /// <summary>
    /// Converts an enum to its kebab-case CLI representation.
    /// Used for command-line argument formatting.
    /// </summary>
    public static string ToCliFormat(this Enum value)
    {
        return KebabCaseHelper.ToKebabCase(value.ToString());
    }

    /// <summary>
    /// Gets a random enum value from type T.
    /// </summary>
    public static T GetRandomValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        return values[new Random().Next(values.Length)];
    }

    /// <summary>
    /// Checks if an enum value equals another by name (case-insensitive).
    /// </summary>
    public static bool EqualsIgnoreCase<T>(this T value, string name) where T : struct, Enum
    {
        return value.ToString().Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets an attribute of type TAttribute from an enum value.
    /// </summary>
    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
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
        return string.Concat(value.Select((x, i) =>
            i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }
}
