// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
#nullable enable

using CoolifyCli.Infrastructure;
using CoolifyCli.Services;

namespace CoolifyCli.Middleware;

/// <summary>
/// Middleware for API authentication. Validates API key presence and validity.
/// Ensures authenticated commands have valid credentials before execution.
/// Marks unauthenticated commands to skip authentication requirements.
/// </summary>
public class AuthenticationMiddleware : ICommandMiddleware
{
    private readonly CoolifyConfiguration _config;
    private readonly ILogger _logger;

    // Commands that don't require authentication
    private static readonly HashSet<string> UnauthenticatedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "version",
        "help",
        "--help",
        "-h",
        "version",
        "-v"
    };

    public AuthenticationMiddleware(CoolifyConfiguration config, ILogger logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates API key presence and basic validity. Allows unauthenticated commands to bypass check.
    /// </summary>
    public async Task ProcessAsync(CommandContext context, CommandMiddlewareDelegate next)
    {
        // Check if command requires authentication
        context.RequiresAuthentication = !UnauthenticatedCommands.Contains(context.CommandName);

        if (context.RequiresAuthentication)
        {
            if (!ValidateAuthentication())
            {
                throw new UnauthorizedAccessException("API key is missing or invalid. Set COOLIFY_API_KEY environment variable. If using a refresh token, ensure it's properly configured and not expired.");
            }

            context.AuthenticatedUser = ExtractUserIdentifier();
            context.IsAuthenticated = true;

            _logger.Debug($"Command authenticated for user: {context.AuthenticatedUser}");
        }

        await next(context);
    }

    /// <summary>
    /// Validates that API configuration contains a valid key.
    /// </summary>
    private bool ValidateAuthentication()
    {
        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            _logger.Warn("API key not configured");
            return false;
        }

        // Basic validation of key format (should be at least 20 characters for security)
        if (_config.ApiKey.Length < 20)
        {
            _logger.Warn("API key appears to be too short");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Extracts user identifier from API key (e.g., first part before colon).
    /// </summary>
    private string ExtractUserIdentifier()
    {
        // For API keys in format "user:token", extract the user part
        if (!string.IsNullOrEmpty(_config.ApiKey) && _config.ApiKey.Contains(":"))
        {
            var parts = _config.ApiKey.Split(":", 2);
            return parts[0];
        }

        // Otherwise, use a hash of the key for anonymity
        return $"user_{_config.ApiKey.GetHashCode():X8}";
    }
}