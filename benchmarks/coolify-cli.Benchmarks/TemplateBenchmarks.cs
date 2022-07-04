using BenchmarkDotNet.Attributes;
using CoolifyCli.Services;

namespace CoolifyCli.Benchmarks;

[MemoryDiagnoser]
public class TemplateBenchmarks
{
    private TemplateVariableResolver _resolver = null!;
    private string _yaml = null!;

    [GlobalSetup]
    public void Setup()
    {
        _resolver = new TemplateVariableResolver(new NullLogger());
        _yaml = "env: ${ENV_NAME}\nimage: ${IMAGE_NAME}\nreplicas: ${REPLICAS}\nport: ${PORT}\n";
    }

    [Benchmark]
    public (string, List<string>) ExpandTemplate()
    {
        return _resolver.Expand(_yaml);
    }
}

public class NullLogger : ILogger
{
    public void Debug(string message) { }
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message) { }
    public void Error(Exception exception, string message = "") { }
    public void Fatal(string message) { }
}
