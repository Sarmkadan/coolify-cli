#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Identifies which fail-fast rule an <see cref="EnvVarViolation"/> broke.
/// </summary>
public enum EnvVarViolationRule
{
    /// <summary>The key is empty or does not match the POSIX name rule <c>[A-Za-z_][A-Za-z0-9_]*</c>.</summary>
    InvalidKeyName,

    /// <summary>The key uses a prefix reserved for Coolify-managed variables.</summary>
    ManagedKey,

    /// <summary>The value contains a raw CR or LF character instead of the escaped <c>\n</c> form.</summary>
    NewlineInValue,

    /// <summary>The same key appears more than once in a single batch.</summary>
    DuplicateKey
}
