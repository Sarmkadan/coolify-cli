#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// A raw key/value pair from user input (a <c>.env</c> file or a <c>--var</c> argument),
/// optionally tagged with the 1-based line it was read from.
/// </summary>
/// <param name="Key">The variable name as written by the user.</param>
/// <param name="Value">The variable value as written by the user.</param>
/// <param name="LineNumber">The 1-based source line, or <c>null</c> when the entry did not come from a file.</param>
public sealed record EnvVarEntry(string Key, string Value, int? LineNumber = null);
