// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Infrastructure;

/// <summary>
/// Application-wide constants and configuration values.
/// </summary>
public static class Constants
{
    public const string ApplicationName = "coolify-cli";
    public const string ApplicationVersion = "1.0.0";
    public const string Author = "Vladyslav Zaiets";
    public const string AuthorUrl = "https://sarmkadan.com";

    public static class Api
    {
        public const string DefaultBaseUrl = "https://api.coolify.io";
        public const string ApiVersion = "v1";
        public const int DefaultTimeoutSeconds = 30;
        public const int MaxRetries = 3;
    }

    public static class Deployment
    {
        public const int MaxApplicationNameLength = 100;
        public const int MinPortNumber = 1;
        public const int MaxPortNumber = 65535;
        public const string DefaultBranch = "main";
    }

    public static class Database
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int DefaultMaxConnections = 100;
        public const int DefaultConnectionTimeoutSeconds = 30;
        public const int DefaultBackupRetentionDays = 30;
    }

    public static class Health
    {
        public const int DefaultCheckIntervalSeconds = 30;
        public const int MinCheckIntervalSeconds = 5;
        public const int MaxCheckIntervalSeconds = 300;
        public const double CpuWarningThresholdPercent = 80.0;
        public const double MemoryWarningThresholdMb = 1024.0;
    }

    public static class Environment
    {
        public const string ApiKeyVariableName             = "COOLIFY_API_KEY";
        public const string ApiUrlVariableName             = "COOLIFY_API_URL";
        public const string VerboseVariableName            = "COOLIFY_VERBOSE";
        public const string TimeoutVariableName            = "COOLIFY_TIMEOUT";
        public const string DefaultEnvironmentVariableName = "COOLIFY_ENVIRONMENT";
        public const string EnvironmentIdVariableName      = "COOLIFY_ENVIRONMENT_ID";
    }

    /// <summary>Constants for the infrastructure-as-code template subsystem.</summary>
    public static class Iac
    {
        /// <summary>The canonical apiVersion value written into new templates.</summary>
        public const string DefaultApiVersion = "v2";

        /// <summary>The only accepted <c>kind</c> discriminator value.</summary>
        public const string SupportedKind = "CoolifyInfrastructure";

        /// <summary>Default filename produced by <c>iac init</c> and used as an option default.</summary>
        public const string DefaultTemplateFileName = "coolify.yaml";

        /// <summary>
        /// Regex pattern matching <c>${VAR_NAME}</c> placeholder tokens in raw YAML text.
        /// Capture group 1 contains the variable name.
        /// </summary>
        public const string TemplateVariablePattern = @"\$\{([A-Za-z_][A-Za-z0-9_]*)\}";

        /// <summary>Maximum number of resource operations to run in parallel during apply.</summary>
        public const int DefaultMaxConcurrentOperations = 3;

        /// <summary>Maximum directory levels ascended when searching for a template file.</summary>
        public const int MaxTemplateSearchDepth = 5;

        /// <summary>Default wall-clock budget for a complete apply phase.</summary>
        public static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromMinutes(5);
    }

    public static class Paths
    {
        public static readonly string ConfigDirectory = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".coolify-cli");
        public static readonly string ConfigFile = Path.Combine(ConfigDirectory, "config.json");
        public static readonly string LogFile = Path.Combine(ConfigDirectory, "logs", "cli.log");
    }

    public static class ExitCodes
    {
        public const int Success = 0;
        public const int GeneralError = 1;
        public const int InvalidArguments = 2;
        public const int ConfigurationError = 3;
        public const int ApiError = 4;
        public const int Timeout = 5;
        public const int ValidationError = 6;
        public const int UnhandledError = 7;
        public const int UnauthorizedAccess = 8;
        public const int TimeoutError = 9;
    }
}
