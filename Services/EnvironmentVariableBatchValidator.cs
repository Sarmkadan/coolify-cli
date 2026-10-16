#nullable enable
using System.Text.RegularExpressions;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Fail-fast validation for a batch of environment variables. Runs before any API call and
/// collects every violation instead of stopping at the first one.
/// </summary>
public static class EnvironmentVariableBatchValidator
{
    /// <summary>
    /// Key prefixes that Coolify generates and manages itself. Keys with these prefixes are
    /// rejected unless the caller explicitly allows them.
    /// </summary>
    public static readonly IReadOnlyList<string> ManagedKeyPrefixes =
        ["SERVICE_FQDN_", "SERVICE_URL_", "SERVICE_USER_", "SERVICE_PASSWORD_", "SERVICE_BASE64_"];

    private static readonly Regex PosixNamePattern = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant);

    /// <summary>
    /// Checks every entry against all rules and returns every violation found.
    /// Values are never included in messages.
    /// </summary>
    /// <param name="entries">The entries to validate.</param>
    /// <param name="allowManagedKeys">When <c>true</c>, keys matching <see cref="ManagedKeyPrefixes"/> are accepted.</param>
    /// <returns>All violations in input order; empty when the batch is valid.</returns>
    public static IReadOnlyList<EnvVarViolation> Validate(IEnumerable<EnvVarEntry> entries, bool allowManagedKeys = false)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var violations = new List<EnvVarViolation>();
        var firstSeenAt = new Dictionary<string, EnvVarEntry>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            var key = entry.Key ?? string.Empty;

            if (!PosixNamePattern.IsMatch(key))
            {
                var message = key.Length == 0
                    ? $"{Location(entry)}: key is empty."
                    : $"{Location(entry)}: key '{key}' must match [A-Za-z_][A-Za-z0-9_]*.";
                violations.Add(new EnvVarViolation(EnvVarViolationRule.InvalidKeyName, key, message, entry.LineNumber));
            }
            else if (!allowManagedKeys && TryGetManagedPrefix(key, out var prefix))
            {
                violations.Add(new EnvVarViolation(
                    EnvVarViolationRule.ManagedKey,
                    key,
                    $"{Location(entry)}: key '{key}' is reserved for Coolify-managed '{prefix}*' variables.",
                    entry.LineNumber));
            }

            if (entry.Value is not null && entry.Value.IndexOfAny(['\r', '\n']) >= 0)
            {
                violations.Add(new EnvVarViolation(
                    EnvVarViolationRule.NewlineInValue,
                    key,
                    $"{Location(entry)}: value of '{key}' contains an unescaped newline; use the \\n escape instead.",
                    entry.LineNumber));
            }

            if (key.Length == 0)
                continue;

            if (firstSeenAt.TryGetValue(key, out var first))
            {
                violations.Add(new EnvVarViolation(
                    EnvVarViolationRule.DuplicateKey,
                    key,
                    $"{Location(entry)}: key '{key}' is already defined at {Location(first)}.",
                    entry.LineNumber));
            }
            else
            {
                firstSeenAt[key] = entry;
            }
        }

        return violations.AsReadOnly();
    }

    /// <summary>
    /// Validates the batch and throws if any rule is broken.
    /// </summary>
    /// <param name="entries">The entries to validate.</param>
    /// <param name="allowManagedKeys">When <c>true</c>, keys matching <see cref="ManagedKeyPrefixes"/> are accepted.</param>
    /// <exception cref="EnvVarValidationException">Thrown with every violation when the batch is invalid.</exception>
    public static void ValidateOrThrow(IEnumerable<EnvVarEntry> entries, bool allowManagedKeys = false)
    {
        var violations = Validate(entries, allowManagedKeys);
        if (violations.Count > 0)
            throw new EnvVarValidationException(violations);
    }

    private static bool TryGetManagedPrefix(string key, out string prefix)
    {
        foreach (var candidate in ManagedKeyPrefixes)
        {
            if (key.StartsWith(candidate, StringComparison.Ordinal))
            {
                prefix = candidate;
                return true;
            }
        }

        prefix = string.Empty;
        return false;
    }

    private static string Location(EnvVarEntry entry) =>
        entry.LineNumber is int line ? $"line {line}" : "--var";
}
