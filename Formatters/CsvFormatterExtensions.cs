#nullable enable
using System.Globalization;

namespace CoolifyCli.Formatters;

/// <summary>
/// Extension methods for <see cref="CsvFormatter"/> providing additional CSV formatting capabilities.
/// </summary>
public static class CsvFormatterExtensions
{
    /// <summary>
    /// Formats a collection of objects as CSV with custom field selection.
    /// Creates a new formatter with the specified field selection for this operation.
    /// </summary>
    /// <typeparam name="T">The type of objects to format</typeparam>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <param name="fields">The fields to include in the output</param>
    /// <returns>CSV formatted string with only the specified fields</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="items"/> or <paramref name="fields"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="fields"/> is empty</exception>
    public static string FormatCollection<T>(this CsvFormatter formatter, IEnumerable<T> items, List<string> fields)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(fields);
        if (fields.Count == 0)
            throw new ArgumentException("Fields collection cannot be empty", nameof(fields));

        // Create a new formatter with the specified fields
        var tempFormatter = new CsvFormatter(formatter.Delimiter, formatter.IncludeHeader, new List<string>(fields));
        return tempFormatter.FormatCollection(items);
    }

    /// <summary>
    /// Formats a collection of objects as CSV with custom delimiter.
    /// Creates a new formatter with the specified delimiter for this operation.
    /// </summary>
    /// <typeparam name="T">The type of objects to format</typeparam>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <param name="delimiter">The delimiter character to use</param>
    /// <returns>CSV formatted string using the specified delimiter</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="items"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiter"/> is whitespace or control character</exception>
    public static string FormatCollection<T>(this CsvFormatter formatter, IEnumerable<T> items, char delimiter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfEqual(delimiter, '\0');
        ArgumentOutOfRangeException.ThrowIfEqual(char.IsWhiteSpace(delimiter), true);

        // Create a temporary formatter with the requested delimiter
        var tempFormatter = new CsvFormatter(delimiter, formatter.IncludeHeader, formatter.SelectedFields != null ? new List<string>(formatter.SelectedFields) : null);
        return tempFormatter.FormatCollection(items);
    }

    /// <summary>
    /// Formats a collection of objects as CSV without header row.
    /// </summary>
    /// <typeparam name="T">The type of objects to format</typeparam>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <returns>CSV formatted string without header row</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="items"/> is null</exception>
    public static string FormatCollectionWithoutHeader<T>(this CsvFormatter formatter, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(items);

        // Create a temporary formatter without header
        var tempFormatter = new CsvFormatter(formatter.Delimiter, false, formatter.SelectedFields != null ? new List<string>(formatter.SelectedFields) : null);
        return tempFormatter.FormatCollection(items);
    }

    /// <summary>
    /// Parses CSV content and returns strongly-typed objects.
    /// Uses reflection to map CSV columns to object properties.
    /// </summary>
    /// <typeparam name="T">The target type to map CSV data to</typeparam>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="csvContent">The CSV content to parse</param>
    /// <returns>List of strongly-typed objects</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="csvContent"/> is empty or whitespace</exception>
    public static List<T> ParseCsv<T>(this CsvFormatter formatter, string csvContent) where T : new()
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentException.ThrowIfNullOrWhiteSpace(csvContent);

        var parsedData = formatter.ParseCsv(csvContent);
        var result = new List<T>();

        if (parsedData.Count == 0)
            return result;

        var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var row in parsedData)
        {
            var instance = new T();
            foreach (var (header, value) in row)
            {
                if (properties.TryGetValue(header, out var property))
                {
                    try
                    {
                        object? convertedValue = null;
                        try
                        {
                            convertedValue = ConvertToType(value, property.PropertyType);
                        }
                        catch (FormatException) when (property.PropertyType == typeof(string))
                        {
                            // Keep as string if conversion fails
                            convertedValue = value;
                        }
                        catch (OverflowException) when (property.PropertyType == typeof(string))
                        {
                            // Keep as string if conversion fails
                            convertedValue = value;
                        }

                        property.SetValue(instance, convertedValue);
                    }
                    catch (Exception ex) when (ex is not FormatException and not OverflowException)
                    {
                        // Re-throw unexpected exceptions
                        throw new InvalidOperationException(
                            $"Failed to set property '{property.Name}' of type {typeof(T).Name}", ex);
                    }
                }
            }
            result.Add(instance);
        }

        return result;
    }

    /// <summary>
    /// Formats a single object as CSV with custom field selection.
    /// Creates a new formatter with the specified field selection for this operation.
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="data">The object to format</param>
    /// <param name="fields">The fields to include in the output</param>
    /// <returns>CSV formatted string with only the specified fields</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="data"/> or <paramref name="fields"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="fields"/> is empty</exception>
    public static string Format(this CsvFormatter formatter, object data, List<string> fields)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(fields);
        if (fields.Count == 0)
            throw new ArgumentException("Fields collection cannot be empty", nameof(fields));

        // Create a new formatter with the specified fields
        var tempFormatter = new CsvFormatter(formatter.Delimiter, formatter.IncludeHeader, new List<string>(fields));
        return tempFormatter.Format(data);
    }

    /// <summary>
    /// Converts a string value to the specified target type using culture-invariant formatting.
    /// </summary>
    /// <param name="value">The string value to convert</param>
    /// <param name="targetType">The target type to convert to</param>
    /// <returns>The converted value</returns>
    private static object? ConvertToType(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value) || value == "")
            return null;

        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(int))
            return int.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(long))
            return long.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(short))
            return short.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(byte))
            return byte.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(uint))
            return uint.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(ulong))
            return ulong.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(ushort))
            return ushort.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(float))
            return float.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(double))
            return double.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(decimal))
            return decimal.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(bool))
            return bool.Parse(value);

        if (targetType == typeof(DateTime))
            return DateTime.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(Guid))
            return Guid.Parse(value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, ignoreCase: true);

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}