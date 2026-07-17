using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CoolifyCli.Infrastructure
{
    /// <summary>
    /// Extension methods for <see cref="IacTemplateOptions"/>.
    /// </summary>
    public static class IacTemplateOptionsExtensions
    {
        /// <summary>
        /// Gets a read-only list of template search paths.
        /// </summary>
        /// <param name="options">The <see cref="IacTemplateOptions"/> instance.</param>
        /// <returns>A read-only list of template search paths.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        public static IReadOnlyList<string> GetTemplateSearchPaths(this IacTemplateOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.TemplateSearchPaths.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the operation timeout is set to a reasonable value (not negative).
        /// </summary>
        /// <param name="options">The <see cref="IacTemplateOptions"/> instance.</param>
        /// <returns>True if the operation timeout is not negative; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        public static bool HasReasonableOperationTimeout(this IacTemplateOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.OperationTimeout >= TimeSpan.Zero;
        }

        /// <summary>
        /// Formats the output based on the specified output format.
        /// </summary>
        /// <param name="options">The <see cref="IacTemplateOptions"/> instance.</param>
        /// <param name="value">The value to format. Cannot be null.</param>
        /// <returns>The formatted string representation of the value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="value"/> is null.</exception>
        public static string FormatOutput(this IacTemplateOptions options, object value)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(value);

            return options.OutputFormat.ToLowerInvariant() switch
            {
                "json" => FormatJson(value),
                _ => value.ToString()
            };
        }

        private static string FormatJson(object value)
        {
            if (value is null)
            {
                return "null";
            }

            return value switch
            {
                string s => $"\"{EscapeJsonString(s)}\"",
                bool b => b.ToString().ToLowerInvariant(),
                int i => i.ToString(CultureInfo.InvariantCulture),
                long l => l.ToString(CultureInfo.InvariantCulture),
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                decimal m => m.ToString(CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }

        private static string EscapeJsonString(string input)
        {
            if (input is null)
            {
                return string.Empty;
            }

            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
