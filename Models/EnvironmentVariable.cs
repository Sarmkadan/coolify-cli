#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Represents an environment variable for an application or service.
/// Supports value encryption, scoping to environments, and change tracking.
/// </summary>
public class EnvironmentVariable
{
    /// <summary>
    /// Gets or sets the unique identifier of the environment variable.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the application or service this variable belongs to.
    /// </summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the environment variable.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the environment variable.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the variable holds a secret value.
    /// </summary>
    public bool IsSecret { get; set; } = false;

    /// <summary>
    /// Gets or sets an optional human-readable description of the variable.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the environment scope (for example, "production") the variable applies to.
    /// </summary>
    public string EnvironmentScope { get; set; } = "production";

    /// <summary>
    /// Gets or sets the timestamp when the variable was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the variable was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user or system that created the variable.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user or system that last updated the variable.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the variable is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Key substrings (case-insensitive) that indicate a variable likely holds a secret
    /// even when the API has not explicitly flagged it via <see cref="IsSecret"/>.
    /// </summary>
    private static readonly string[] SensitiveKeyPatterns =
    [
        "SECRET", "TOKEN", "PASSWORD", "KEY", "CREDENTIAL"
    ];

    /// <summary>
    /// Determines whether <see cref="Key"/> matches a naming pattern commonly used for secrets
    /// (e.g. contains SECRET, TOKEN, PASSWORD, KEY, or CREDENTIAL), regardless of the
    /// explicit <see cref="IsSecret"/> flag.
    /// </summary>
    /// <returns>True if the key name looks like it holds a secret value.</returns>
    private bool HasSensitiveKeyPattern() =>
        !string.IsNullOrWhiteSpace(Key) &&
        SensitiveKeyPatterns.Any(pattern => Key.Contains(pattern, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Validates the environment variable key and value.
    /// </summary>
    /// <returns>Collection of validation error messages.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Key))
            errors.Add("Environment variable key is required.");

        if (!IsValidKeyFormat(Key))
            errors.Add("Environment variable key must contain only alphanumeric characters and underscores.");

        if (string.IsNullOrEmpty(Value) && !IsSecret)
            errors.Add("Environment variable value cannot be empty for non-secret variables.");

        if (IsSecret && Value.Length > 0 && Value.Length < 8)
            errors.Add("Secret values should be at least 8 characters long.");

        if (string.IsNullOrWhiteSpace(ApplicationId))
            errors.Add("Application ID is required.");

        if (string.IsNullOrWhiteSpace(EnvironmentScope))
            errors.Add("Environment scope is required.");

        return errors;
    }

    /// <summary>
    /// Checks if the environment variable key follows naming conventions.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <returns>True if key format is valid.</returns>
    private static bool IsValidKeyFormat(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return key.All(c => char.IsLetterOrDigit(c) || c == '_') && !char.IsDigit(key[0]);
    }

    /// <summary>
    /// Gets the display value, masking secrets if requested. A variable is masked when it is
    /// explicitly flagged via <see cref="IsSecret"/> or when its <see cref="Key"/> matches a
    /// common secret naming pattern (SECRET, TOKEN, PASSWORD, KEY, CREDENTIAL).
    /// </summary>
    /// <param name="maskSecrets">Whether to mask secret values.</param>
    /// <returns>Display value for the variable.</returns>
    public string GetDisplayValue(bool maskSecrets = true)
    {
        if (!maskSecrets || (!IsSecret && !HasSensitiveKeyPattern()))
            return Value;

        return Value.Length > 4 ? $"***{Value.Substring(Value.Length - 4)}" : "***";
    }

    /// <summary>
    /// Creates a copy of the environment variable for auditing purposes.
    /// </summary>
    /// <returns>A new instance with the same properties.</returns>
    public EnvironmentVariable Clone()
    {
        return new EnvironmentVariable
        {
            Id = Id,
            ApplicationId = ApplicationId,
            Key = Key,
            Value = Value,
            IsSecret = IsSecret,
            Description = Description,
            EnvironmentScope = EnvironmentScope,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            CreatedBy = CreatedBy,
            UpdatedBy = UpdatedBy,
            IsActive = IsActive
        };
    }

    /// <summary>
    /// Marks the variable as updated with current timestamp and user.
    /// </summary>
    /// <param name="updatedBy">User or system performing the update.</param>
    public void MarkAsUpdated(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
