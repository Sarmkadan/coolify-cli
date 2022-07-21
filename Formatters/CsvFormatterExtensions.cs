#nullable enable
using System.Reflection;
using System.Text;

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
    public static string FormatCollection<T>(this CsvFormatter formatter, IEnumerable<T> items, List<string> fields)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (items is null)
            throw new ArgumentNullException(nameof(items));

        if (fields is null || fields.Count == 0)
            throw new ArgumentNullException(nameof(fields));

        // Create a new formatter with the specified fields
        var tempFormatter = new CsvFormatter(formatter.GetDelimiter(), formatter.GetIncludeHeader(), fields);
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
    public static string FormatCollection<T>(this CsvFormatter formatter, IEnumerable<T> items, char delimiter)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (items is null)
            throw new ArgumentNullException(nameof(items));

        // Create a temporary formatter with the requested delimiter
        var tempFormatter = new CsvFormatter(delimiter, formatter.GetIncludeHeader(), formatter.GetSelectedFields());
        return tempFormatter.FormatCollection(items);
    }

    /// <summary>
    /// Formats a collection of objects as CSV without header row.
    /// </summary>
    /// <typeparam name="T">The type of objects to format</typeparam>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <returns>CSV formatted string without header row</returns>
    public static string FormatCollectionWithoutHeader<T>(this CsvFormatter formatter, IEnumerable<T> items)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (items is null)
            throw new ArgumentNullException(nameof(items));

        // Create a temporary formatter without header
        var tempFormatter = new CsvFormatter(formatter.GetDelimiter(), false, formatter.GetSelectedFields());
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
    public static List<T> ParseCsv<T>(this CsvFormatter formatter, string csvContent) where T : new()
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (string.IsNullOrWhiteSpace(csvContent))
            return new List<T>();

        var parsedData = formatter.ParseCsv(csvContent);
        var result = new List<T>();

        if (parsedData.Count == 0)
            return result;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
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
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(instance, convertedValue);
                    }
                    catch
                    {
                        // If conversion fails, try to parse as string
                        property.SetValue(instance, value);
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
    public static string Format(this CsvFormatter formatter, object data, List<string> fields)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        if (fields is null || fields.Count == 0)
            throw new ArgumentNullException(nameof(fields));

        // Create a new formatter with the specified fields
        var tempFormatter = new CsvFormatter(formatter.GetDelimiter(), formatter.GetIncludeHeader(), fields);
        return tempFormatter.Format(data);
    }

    /// <summary>
    /// Gets the currently selected fields from the formatter.
    /// </summary>
    private static List<string>? GetSelectedFields(this CsvFormatter formatter)
    {
        var fieldInfo = typeof(CsvFormatter).GetField("_selectedFields", BindingFlags.NonPublic | BindingFlags.Instance);
        return fieldInfo?.GetValue(formatter) as List<string>;
    }

    /// <summary>
    /// Gets the include header setting from the formatter.
    /// </summary>
    private static bool GetIncludeHeader(this CsvFormatter formatter)
    {
        var headerInfo = typeof(CsvFormatter).GetField("_includeHeader", BindingFlags.NonPublic | BindingFlags.Instance);
        return (bool)(headerInfo?.GetValue(formatter) ?? true);
    }

    /// <summary>
    /// Gets the delimiter from the formatter.
    /// </summary>
    private static char GetDelimiter(this CsvFormatter formatter)
    {
        var delimiterInfo = typeof(CsvFormatter).GetField("_delimiter", BindingFlags.NonPublic | BindingFlags.Instance);
        return (char)(delimiterInfo?.GetValue(formatter) ?? ',');
    }
}