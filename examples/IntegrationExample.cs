// IntegrationExample.cs
// This example demonstrates how to register Coolify services in an ASP.NET Core
// Dependency Injection (DI) container.

using Microsoft.Extensions.DependencyInjection;
using CoolifyCli.Infrastructure;
using CoolifyCli.Services;

public static class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // 1. Register configuration
        services.AddSingleton(CoolifyConfiguration.FromEnvironment());

        // 2. Register HttpClient with timeout from config
        services.AddHttpClient<CoolifyApiClient>((sp, client) =>
        {
            var config = sp.GetRequiredService<CoolifyConfiguration>();
            client.BaseAddress = new Uri(config.ApiUrl);
            client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
        });

        // 3. Register Coolify Services
        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddScoped<ApplicationService>();
        services.AddScoped<DatabaseService>();
    }
}
