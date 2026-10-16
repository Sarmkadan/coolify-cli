#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoolifyCli.Infrastructure;

/// <summary>
/// Registers <see cref="CoolifyOptions"/> with the dependency injection container.
/// </summary>
public static class CoolifyOptionsExtensions
{
    /// <summary>
    /// Binds <see cref="CoolifyOptions"/> from the <c>Coolify</c> section of <paramref name="configuration"/>,
    /// registers <see cref="CoolifyOptionsValidator"/>, and validates on host start so that an invalid
    /// configuration fails before any command makes an HTTP call.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The layered configuration from <see cref="CoolifyOptionsConfiguration.Build"/>.</param>
    /// <returns>The options builder, for further customization.</returns>
    public static OptionsBuilder<CoolifyOptions> AddCoolifyOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IValidateOptions<CoolifyOptions>, CoolifyOptionsValidator>();

        return services.AddOptions<CoolifyOptions>()
            .Bind(configuration.GetSection(CoolifyOptions.SectionName))
            .ValidateOnStart();
    }
}
