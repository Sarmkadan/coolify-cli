// AdvancedUsage.cs
// This example demonstrates deploying an application, handling configuration,
// and dealing with errors during the deployment process.

using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;

public class AdvancedUsage
{
    public static async Task DeployExample(int appId)
    {
        // 1. Load custom configuration
        var config = new CoolifyConfiguration
        {
            ApiUrl = "https://coolify.example.com",
            ApiKey = "your-api-key",
            RequestTimeoutSeconds = 60
        };

        // 2. Setup dependencies
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds) };
        var apiClient = new CoolifyApiClient(httpClient, config.ApiUrl, config.ApiKey);
        var logger = new ConsoleLogger(verbose: true, colorOutput: true);
        var appService = new ApplicationService(apiClient, logger);

        // 3. Deployment with Error Handling
        try
        {
            logger.Info($"Attempting to deploy app {appId}...");
            
            // Get app details first
            var appResult = await appService.GetApplicationAsync(appId);
            if (!appResult.Success || appResult.Data == null)
            {
                logger.Error($"Failed to fetch app: {appResult.Message}");
                return;
            }

            // Perform deployment
            var context = new DeploymentContext { Application = appResult.Data };
            var deployResult = await appService.DeployApplicationAsync(appId, context);

            if (deployResult.Success)
            {
                logger.Info($"Deployment successfully triggered. ID: {deployResult.Data?.DeploymentId}");
            }
            else
            {
                logger.Error($"Deployment failed: {deployResult.Message}");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.Error($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An unexpected error occurred during deployment.");
        }
    }
}
