#nullable enable
namespace CoolifyCli.Infrastructure;

/// <summary>
/// Application-wide constants and configuration values.
/// </summary>
public static class Constants
{
    /// <summary>The command-line application's display and executable name: <c>coolify-cli</c>.</summary>
    public const string ApplicationName = "coolify-cli";

    /// <summary>The current application version, represented as a semantic-version string.</summary>
    public const string ApplicationVersion = "2.0.2";

    /// <summary>The name of the application author.</summary>
    public const string Author = "Vladyslav Zaiets";

    /// <summary>The absolute URL of the application author's website.</summary>
    public const string AuthorUrl = "https://sarmkadan.com";

    /// <summary>Constants used to configure requests to the Coolify API.</summary>
    public static class Api
    {
        /// <summary>The default absolute base URL used to contact the Coolify API.</summary>
        public const string DefaultBaseUrl = "https://api.coolify.io";

        /// <summary>The API version identifier used by the client.</summary>
        public const string ApiVersion = "v1";

        /// <summary>The versioned relative path for application API operations.</summary>
        public const string ApplicationsEndpoint = "/api/v1/applications";

        /// <summary>The versioned relative path for database API operations.</summary>
        public const string DatabasesEndpoint = "/api/v1/databases";

        /// <summary>The default API request timeout, in seconds.</summary>
        public const int DefaultTimeoutSeconds = 30;

        /// <summary>The maximum number of retry attempts allowed for an API operation.</summary>
        public const int MaxRetries = 3;
    }

    /// <summary>Validation limits and defaults used when configuring deployments.</summary>
    public static class Deployment
    {
        /// <summary>The maximum permitted application name length, in characters.</summary>
        public const int MaxApplicationNameLength = 100;

        /// <summary>The lowest valid TCP or UDP port number.</summary>
        public const int MinPortNumber = 1;

        /// <summary>The highest valid TCP or UDP port number.</summary>
        public const int MaxPortNumber = 65535;

        /// <summary>The default source-control branch used for deployments.</summary>
        public const string DefaultBranch = "main";
    }

    /// <summary>Validation limits and defaults used for database configuration.</summary>
    public static class Database
    {
        /// <summary>The minimum permitted database password length, in characters.</summary>
        public const int MinPasswordLength = 8;

        /// <summary>The maximum permitted database password length, in characters.</summary>
        public const int MaxPasswordLength = 128;

        /// <summary>The default maximum number of concurrent database connections.</summary>
        public const int DefaultMaxConnections = 100;

        /// <summary>The default database connection timeout, in seconds.</summary>
        public const int DefaultConnectionTimeoutSeconds = 30;

        /// <summary>The default duration, in days, for retaining database backups.</summary>
        public const int DefaultBackupRetentionDays = 30;
    }

    /// <summary>Intervals and warning thresholds used by health monitoring.</summary>
    public static class Health
    {
        /// <summary>The default interval between health checks, in seconds.</summary>
        public const int DefaultCheckIntervalSeconds = 30;

        /// <summary>The minimum permitted interval between health checks, in seconds.</summary>
        public const int MinCheckIntervalSeconds = 5;

        /// <summary>The maximum permitted interval between health checks, in seconds.</summary>
        public const int MaxCheckIntervalSeconds = 300;

        /// <summary>The CPU usage percentage at which a health warning is raised.</summary>
        public const double CpuWarningThresholdPercent = 80.0;

        /// <summary>The memory usage, in megabytes, at which a health warning is raised.</summary>
        public const double MemoryWarningThresholdMb = 1024.0;
    }

    /// <summary>Names of environment variables recognized by the application.</summary>
    public static class Environment
    {
        /// <summary>The environment variable that supplies the Coolify API key.</summary>
        public const string ApiKeyVariableName             = "COOLIFY_API_KEY";

        /// <summary>The environment variable that overrides the Coolify API URL.</summary>
        public const string ApiUrlVariableName             = "COOLIFY_API_URL";

        /// <summary>The environment variable that controls verbose output.</summary>
        public const string VerboseVariableName            = "COOLIFY_VERBOSE";

        /// <summary>The environment variable that supplies the request timeout.</summary>
        public const string TimeoutVariableName            = "COOLIFY_TIMEOUT";

        /// <summary>The environment variable that identifies the default named environment.</summary>
        public const string DefaultEnvironmentVariableName = "COOLIFY_ENVIRONMENT";

        /// <summary>The environment variable that supplies the Coolify environment identifier.</summary>
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

    /// <summary>Filesystem locations used for application configuration and logs.</summary>
    public static class Paths
    {
        /// <summary>The directory where user-specific configuration files are stored.</summary>
        public static readonly string ConfigDirectory = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".coolify-cli");
        /// <summary>The path to the JSON configuration file.</summary>
        public static readonly string ConfigFile = Path.Combine(ConfigDirectory, "config.json");
        /// <summary>The path to the application log file.</summary>
        public static readonly string LogFile = Path.Combine(ConfigDirectory, "logs", "cli.log");
    }

    /// <summary>Process exit codes returned by the command-line application.</summary>
    public static class ExitCodes
    {
        /// <summary>Indicates successful completion; value <c>0</c>.</summary>
        public const int Success = 0;

        /// <summary>Indicates a general, otherwise unclassified error; value <c>1</c>.</summary>
        public const int GeneralError = 1;

        /// <summary>Indicates invalid command-line arguments; value <c>2</c>.</summary>
        public const int InvalidArguments = 2;

        /// <summary>Indicates an invalid or missing configuration; value <c>3</c>.</summary>
        public const int ConfigurationError = 3;

        /// <summary>Indicates an error returned while communicating with the API; value <c>4</c>.</summary>
        public const int ApiError = 4;

        /// <summary>Indicates that an operation timed out; value <c>5</c>.</summary>
        public const int Timeout = 5;

        /// <summary>Indicates that input or resource validation failed; value <c>6</c>.</summary>
        public const int ValidationError = 6;

        /// <summary>Indicates an unexpected, unhandled error; value <c>7</c>.</summary>
        public const int UnhandledError = 7;

        /// <summary>Indicates that authorization is required or was denied; value <c>8</c>.</summary>
        public const int UnauthorizedAccess = 8;

        /// <summary>Indicates a timeout-specific error result; value <c>9</c>.</summary>
        public const int TimeoutError = 9;
    }
}
