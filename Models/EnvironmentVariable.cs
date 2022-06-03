#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Models;

/// <summary>
/// Represents an environment variable for an application or service.
/// Supports value encryption, scoping to environments, and change tracking.
/// </summary>
public class EnvironmentVariable
{
    public int Id { get; set; }
    public string ApplicationId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsSecret { get; set; } = false;
    public string? Description { get; set; }
    public string EnvironmentScope { get; set; } = "production";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;

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
    /// Gets the display value, masking secrets if requested.
    /// </summary>
    /// <param name="maskSecrets">Whether to mask secret values.</param>
    /// <returns>Display value for the variable.</returns>
    public string GetDisplayValue(bool maskSecrets = true)
    {
        if (!IsSecret || !maskSecrets)
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
