// BasicUsage.cs
// This example demonstrates how to use the CoolifyApiClient and ApplicationService
// to list all applications managed by Coolify.

using CoolifyCli.Infrastructure;
using CoolifyCli.Services;

public class BasicUsage
{
    public static async Task RunExample()
    {
        // 1. Initialize configuration (assuming COOLIFY_API_URL and COOLIFY_API_KEY are set)
        var config = CoolifyConfiguration.FromEnvironment();
        
        // 2. Setup dependencies
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds) };
        var apiClient = new CoolifyApiClient(httpClient, config.ApiUrl, config.ApiKey!);
        var logger = new ConsoleLogger(verbose: false, colorOutput: true);
        
        // 3. Use the service
        var appService = new ApplicationService(apiClient, logger);
        var result = await appService.GetAllApplicationsAsync();

        if (result.Success && result.Data != null)
        {
            Console.WriteLine($"Found {result.Data.Count} applications:");
            foreach (var app in result.Data)
            {
                Console.WriteLine($"- {app.Name} (ID: {app.Id}, Status: {app.Status})");
            }
        }
        else
        {
            Console.WriteLine($"Error: {result.Message}");
        }
    }
}
