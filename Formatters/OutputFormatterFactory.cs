#nullable enable

namespace CoolifyCli.Formatters;

/// <summary>
/// Factory for creating output formatters based on format type.
/// Provides a unified way to dispatch formatters via CLI flags like --output json|csv|table.
/// Formats are looked up in a registry, so new formatters can be added without changing the dispatch logic.
/// </summary>
public static class OutputFormatterFactory
{
    private static readonly Dictionary<string, Func<FormatterOptions?, IOutputFormatter>> Registry = new()
    {
        ["json"] = CreateJsonFormatter,
        ["csv"] = CreateCsvFormatter,
        ["table"] = CreateTableFormatter,
        ["text"] = CreateTableFormatter
    };

    private static readonly Dictionary<string, string> ExtensionFormats = new()
    {
        ["json"] = "json",
        ["csv"] = "csv",
        ["txt"] = "table"
    };

    /// <summary>
    /// Registers a formatter under the given format name, replacing any existing registration.
    /// </summary>
    /// <param name="format">The format name used in lookups (case-insensitive).</param>
    /// <param name="factory">Creates a formatter from optional formatter-specific options.</param>
    /// <exception cref="ArgumentNullException">Thrown when format or factory is null.</exception>
    public static void Register(string format, Func<FormatterOptions?, IOutputFormatter> factory)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(factory);

        Registry[format.ToLowerInvariant()] = factory;
    }

    /// <summary>
    /// Creates a formatter for the specified output format.
    /// </summary>
    /// <param name="format">The output format ("json", "csv", "table", "text").</param>
    /// <param name="options">Optional formatter-specific options.</param>
    /// <returns>A configured formatter instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when format is null.</exception>
    /// <exception cref="ArgumentException">Thrown when format is not supported.</exception>
    public static IOutputFormatter CreateFormatter(string format, FormatterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(format);

        if (!Registry.TryGetValue(format.ToLowerInvariant(), out var factory))
            throw new ArgumentException($"Unsupported output format: {format}. Supported formats: json, csv, table", nameof(format));

        return factory(options);
    }

    /// <summary>
    /// Creates a formatter based on file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension (e.g., ".json", ".csv", ".txt").</param>
    /// <param name="options">Optional formatter-specific options.</param>
    /// <returns>A configured formatter instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fileExtension is null.</exception>
    /// <exception cref="ArgumentException">Thrown when extension is not supported.</exception>
    public static IOutputFormatter CreateFormatterByExtension(string fileExtension, FormatterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fileExtension);

        var ext = fileExtension.TrimStart('.').ToLowerInvariant();
        if (!ExtensionFormats.TryGetValue(ext, out var format))
            throw new ArgumentException($"Unsupported file extension: {fileExtension}. Supported: .json, .csv, .txt", nameof(fileExtension));

        return CreateFormatter(format, options);
    }

    private static JsonFormatter CreateJsonFormatter(FormatterOptions? options)
    {
        if (options is null)
            return new JsonFormatter();

        return new JsonFormatter(
            prettyPrint: options.PrettyPrint.GetValueOrDefault(false),
            includeFields: options.IncludeFields,
            excludeFields: options.ExcludeFields
        );
    }

    private static CsvFormatter CreateCsvFormatter(FormatterOptions? options)
    {
        if (options is null)
            return new CsvFormatter();

        return new CsvFormatter(
            delimiter: options.Delimiter ?? ',',
            includeHeader: options.IncludeHeader.GetValueOrDefault(true),
            selectedFields: options.SelectedFields
        );
    }

    private static TableFormatter CreateTableFormatter(FormatterOptions? options)
    {
        if (options is null)
            return new TableFormatter();

        return new TableFormatter(
            style: options.TableStyle ?? TableStyle.Simple,
            columnNames: options.ColumnNames
        );
    }
}

/// <summary>
/// Options for configuring formatters.
/// </summary>
public class FormatterOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to pretty-print JSON output.
    /// </summary>
    public bool? PrettyPrint { get; set; }

    /// <summary>
    /// Gets or sets the list of fields to include in JSON output.
    /// </summary>
    public List<string>? IncludeFields { get; set; }

    /// <summary>
    /// Gets or sets the list of fields to exclude from JSON output.
    /// </summary>
    public List<string>? ExcludeFields { get; set; }

    /// <summary>
    /// Gets or sets the CSV delimiter character.
    /// </summary>
    public char? Delimiter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include header row in CSV output.
    /// </summary>
    public bool? IncludeHeader { get; set; }

    /// <summary>
    /// Gets or sets the list of fields to include in CSV output.
    /// </summary>
    public List<string>? SelectedFields { get; set; }

    /// <summary>
    /// Gets or sets the table style.
    /// </summary>
    public TableStyle? TableStyle { get; set; }

    /// <summary>
    /// Gets or sets the list of column names for table output.
    /// </summary>
    public List<string>? ColumnNames { get; set; }
}
