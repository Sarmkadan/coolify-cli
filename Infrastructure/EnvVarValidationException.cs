#nullable enable
using CoolifyCli.Models;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Raised when a batch of environment variables fails fail-fast validation. Carries every
/// violation found in the batch so the caller can report them all at once.
/// </summary>
public class EnvVarValidationException : CoolifyException
{
    /// <summary>Gets the complete list of violations that triggered this exception.</summary>
    public IReadOnlyList<EnvVarViolation> Violations { get; }

    /// <summary>
    /// Initialises the exception from a non-empty list of violations.
    /// </summary>
    /// <param name="violations">The violations found while validating the batch.</param>
    public EnvVarValidationException(IReadOnlyList<EnvVarViolation> violations)
        : base(
            $"Environment variable validation failed with {violations.Count} violation(s): " +
            string.Join("; ", violations.Select(v => v.Message)),
            "ENV_VAR_VALIDATION_FAILED")
    {
        Violations = violations;
        AddContextData("violationCount", violations.Count.ToString());
    }
}
