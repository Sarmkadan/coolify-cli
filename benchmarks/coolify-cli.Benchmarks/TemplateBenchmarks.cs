using BenchmarkDotNet.Attributes;
using CoolifyCli.Services;

namespace CoolifyCli.Benchmarks;

/// <summary>
/// Benchmark class for template expansion performance.
/// </summary>
[MemoryDiagnoser]
public class TemplateBenchmarks
{
    /// <summary>
    /// The template variable resolver instance used for benchmarking.
    /// </summary>
    private TemplateVariableResolver _resolver = null!;

    /// <summary>
    /// The YAML template string used for benchmarking.
    /// </summary>
    private string _yaml = null!;

    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _resolver = new TemplateVariableResolver(new NullLogger());
        _yaml = "env: ${ENV_NAME}\nimage: ${IMAGE_NAME}\nreplicas: ${REPLICAS}\nport: ${PORT}\n";
    }

    /// <summary>
    /// Expands the template with variable values and returns the result.
    /// </summary>
    /// <returns>A tuple containing the expanded template string and a list of variable names.</returns>
    [Benchmark]
    public (string, List<string>) ExpandTemplate()
    {
        return _resolver.Expand(_yaml);
    }
}

/// <summary>
/// A logger that does nothing.
/// </summary>
public class NullLogger : ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Debug(string message) { }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message) { }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Warn(string message) { }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Error(string message) { }

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public void Error(Exception exception, string message = "") { }

    /// <summary>
    /// Logs a fatal message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Fatal(string message) { }
}
