#nullable enable

namespace CoolifyCli.Extensions;

using CoolifyCli.Models;
using CoolifyCli.Services;

/// <summary>
/// Extension methods for the infrastructure-as-code template engine.
/// </summary>
public static class IacServiceExtensions
{
    /// <summary>
    /// Creates a ready-to-use <see cref="IInfrastructureTemplateEngine"/> from the three
    /// concrete dependencies it requires.
    /// </summary>
    /// <param name="appService">Application lifecycle service.</param>
    /// <param name="dbService">Database management service.</param>
    /// <param name="logger">Structured diagnostic logger.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="appService"/>, <paramref name="dbService"/>, or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <returns>A configured <see cref="InfrastructureTemplateEngine"/> instance.</returns>
    public static IInfrastructureTemplateEngine CreateTemplateEngine(
        ApplicationService appService,
        DatabaseService dbService,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(appService);
        ArgumentNullException.ThrowIfNull(dbService);
        ArgumentNullException.ThrowIfNull(logger);

        return new InfrastructureTemplateEngine(appService, dbService, logger);
    }

    /// <summary>
    /// Reads the YAML file at <paramref name="filePath"/>, expands all
    /// <c>${VAR_NAME}</c> placeholders via <paramref name="resolver"/>, then delegates to
    /// <see cref="IInfrastructureTemplateEngine.LoadTemplateAsync"/> for deserialization.
    /// </summary>
    /// <remarks>
    /// Variable expansion is performed on the raw file text before YAML parsing so that
    /// placeholders can appear in any position — including keys, values, and anchors.
    /// </remarks>
    /// <param name="engine">The template engine to load into.</param>
    /// <param name="filePath">
    /// Absolute or working-directory-relative path to the <c>.yaml</c> file.
    /// </param>
    /// <param name="resolver">
    /// Resolver used to expand <c>${VAR}</c> tokens before parsing.
    /// </param>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="engine"/> or <paramref name="resolver"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="filePath"/> is <see langword="null"/>, empty, or consists only of whitespace.
    /// </exception>
    /// <returns>
    /// A successful <see cref="ApiResponse{T}"/> containing the parsed template, or an error
    /// response when the file is missing, variables are unresolved, or the YAML is malformed.
    /// </returns>
    public static async Task<ApiResponse<InfrastructureTemplate>> LoadWithVariablesAsync(
        this IInfrastructureTemplateEngine engine,
        string filePath,
        TemplateVariableResolver resolver,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var resolvedPath = Path.GetFullPath(filePath);
        if (!File.Exists(resolvedPath))
        {
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Template file not found: {resolvedPath}", 404);
        }

        var raw = await File.ReadAllTextAsync(resolvedPath, cancellationToken);
        var (expanded, unresolved) = resolver.Expand(raw);

        if (unresolved.Count > 0)
        {
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Unresolved template variable(s): " +
                $"{string.Join(", ", unresolved.Select(v => "${" + v + "}"))}", 400);
        }

        // Write the expanded content to a temp file so the existing LoadTemplateAsync
        // pipeline (which operates on file paths) can parse it without modification.
        var tempPath = Path.Combine(
            Path.GetTempPath(), $"coolify-iac-{Guid.NewGuid():N}.yaml");
        try
        {
            await File.WriteAllTextAsync(tempPath, expanded, cancellationToken);
            return await engine.LoadTemplateAsync(tempPath, cancellationToken);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}