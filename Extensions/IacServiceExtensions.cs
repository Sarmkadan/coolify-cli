#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Extensions;

using Microsoft.Extensions.DependencyInjection;
using CoolifyCli.Models;
using CoolifyCli.Services;

/// <summary>
/// Extension methods that integrate the infrastructure-as-code template engine into both
/// container-managed and manually-constructed service graphs.
/// </summary>
public static class IacServiceExtensions
{
    /// <summary>
    /// Registers the infrastructure-as-code template engine and its supporting services with
    /// the DI container.
    /// <para>
    /// Prerequisites: <see cref="CoolifyApiClient"/>, <see cref="ApplicationService"/>,
    /// <see cref="DatabaseService"/>, and <see cref="ILogger"/> must already be registered
    /// before calling this method.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same collection for fluent chaining.</returns>
    public static IServiceCollection AddInfrastructureTemplateEngine(
        this IServiceCollection services)
    {
        services.AddTransient<TemplateVariableResolver>();
        services.AddTransient<IInfrastructureTemplateEngine, InfrastructureTemplateEngine>();
        return services;
    }

    /// <summary>
    /// Creates a ready-to-use <see cref="IInfrastructureTemplateEngine"/> from the three
    /// concrete dependencies it requires.  Intended for use in top-level programs and
    /// integration tests that build their object graph manually rather than through a DI
    /// container.
    /// </summary>
    /// <param name="appService">Application lifecycle service.</param>
    /// <param name="dbService">Database management service.</param>
    /// <param name="logger">Structured diagnostic logger.</param>
    /// <returns>A configured <see cref="InfrastructureTemplateEngine"/> instance.</returns>
    public static IInfrastructureTemplateEngine CreateTemplateEngine(
        ApplicationService appService,
        DatabaseService dbService,
        ILogger logger) =>
        new InfrastructureTemplateEngine(appService, dbService, logger);

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
    /// <param name="filePath">Absolute or working-directory-relative path to the <c>.yaml</c> file.</param>
    /// <param name="resolver">Resolver used to expand <c>${VAR}</c> tokens before parsing.</param>
    /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
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

        var resolvedPath = Path.GetFullPath(filePath);
        if (!File.Exists(resolvedPath))
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Template file not found: {resolvedPath}", 404);

        var raw = await File.ReadAllTextAsync(resolvedPath, cancellationToken);
        var (expanded, unresolved) = resolver.Expand(raw);

        if (unresolved.Count > 0)
            return ApiResponse<InfrastructureTemplate>.ErrorResponse(
                $"Unresolved template variable(s): " +
                $"{string.Join(", ", unresolved.Select(v => "${" + v + "}"))}", 400);

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
                File.Delete(tempPath);
        }
    }
}
