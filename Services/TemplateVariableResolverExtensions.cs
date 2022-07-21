#nullable enable

namespace CoolifyCli.Services;

/// <summary>
/// Provides useful extension methods for <see cref="TemplateVariableResolver"/> to simplify
/// common template variable resolution scenarios.
/// </summary>
public static class TemplateVariableResolverExtensions
{
    /// <summary>
    /// Attempts to expand template variables in the provided YAML content, returning only the
    /// expanded result. If any variables cannot be resolved, throws an exception with details
    /// of the unresolved variables.
    /// </summary>
    /// <param name="resolver">The template variable resolver.</param>
    /// <param name="rawYaml">The raw YAML content containing template variables.</param>
    /// <returns>The expanded YAML content.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any template variables cannot be resolved.
    /// </exception>
    public static string ExpandOrThrow(this TemplateVariableResolver resolver, string rawYaml)
    {
        var (expanded, unresolved) = resolver.Expand(rawYaml);

        if (unresolved.Count > 0)
        {
            throw new InvalidOperationException(
                $"Failed to resolve template variables: {string.Join(", ", unresolved.Select(v => $"${{" + v + "}}"))}. " +
                "Ensure all required variables are provided via overrides or environment.");
        }

        return expanded;
    }

    /// <summary>
    /// Attempts to expand template variables in the provided YAML content, returning a tuple
    /// indicating whether the expansion was successful and the expanded result.
    /// </summary>
    /// <param name="resolver">The template variable resolver.</param>
    /// <param name="rawYaml">The raw YAML content containing template variables.</param>
    /// <returns>
    /// A tuple of (success: bool, expandedYaml: string).
    /// When success is true, expandedYaml contains the fully resolved content.
    /// When success is false, expandedYaml contains the original content with unresolved variables.
    /// </returns>
    public static (bool Success, string ExpandedYaml) TryExpand(this TemplateVariableResolver resolver, string rawYaml)
    {
        var (expanded, unresolved) = resolver.Expand(rawYaml);

        return (unresolved.Count == 0, expanded);
    }

    /// <summary>
    /// Loads multiple .env files into the resolver, accumulating all variables.
    /// </summary>
    /// <param name="resolver">The template variable resolver.</param>
    /// <param name="dotEnvPaths">Collection of paths to .env files to load.</param>
    /// <returns>The total number of variables loaded across all files.</returns>
    public static int LoadDotEnvFiles(this TemplateVariableResolver resolver, IEnumerable<string> dotEnvPaths)
    {
        if (dotEnvPaths == null)
            return 0;

        int totalLoaded = 0;
        foreach (var path in dotEnvPaths)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                totalLoaded += resolver.LoadDotEnvFile(path);
            }
        }

        return totalLoaded;
    }

    /// <summary>
    /// Determines whether all template variables in the provided YAML content can be resolved
    /// using the current resolver's overrides and environment variables.
    /// </summary>
    /// <param name="resolver">The template variable resolver.</param>
    /// <param name="rawYaml">The raw YAML content to check.</param>
    /// <returns>
    /// True if all template variables can be resolved; otherwise, false.
    /// </returns>
    public static bool CanResolveAll(this TemplateVariableResolver resolver, string rawYaml)
    {
        var placeholders = TemplateVariableResolver.CollectPlaceholders(rawYaml);

        if (placeholders.Count == 0)
            return true;

        // Use Expand to check which variables are unresolved
        var (_, unresolved) = resolver.Expand(rawYaml);
        return unresolved.Count == 0;
    }
}