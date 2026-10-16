#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// A single problem found while validating a batch of environment variables.
/// Messages name the key but never include the value, so they are safe to print.
/// </summary>
/// <param name="Rule">The rule that was broken.</param>
/// <param name="Key">The key the violation applies to.</param>
/// <param name="Message">Human-readable description of the problem.</param>
/// <param name="LineNumber">The 1-based source line, or <c>null</c> when the entry did not come from a file.</param>
public sealed record EnvVarViolation(EnvVarViolationRule Rule, string Key, string Message, int? LineNumber = null);
