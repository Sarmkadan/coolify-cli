using BenchmarkDotNet.Attributes;
using CoolifyCli.Models;

namespace CoolifyCli.Benchmarks;

[MemoryDiagnoser]
public class DeploymentBenchmarks
{
    private ApplicationDeployment _current = null!;
    private ApplicationDeployment _proposed = null!;

    [GlobalSetup]
    public void Setup()
    {
        _current = new ApplicationDeployment
        {
            Id = 1,
            Name = "test-app",
            Repository = "github.com/test/repo",
            EnvironmentId = "env-1",
            EnvironmentVariables = new() { { "KEY1", "VAL1" }, { "KEY2", "VAL2" } },
            Ports = new() { "8080" }
        };

        _proposed = new ApplicationDeployment
        {
            Id = 1,
            Name = "test-app",
            Repository = "github.com/test/repo",
            EnvironmentId = "env-1",
            EnvironmentVariables = new() { { "KEY1", "VAL_NEW" }, { "KEY2", "VAL2" }, { "KEY3", "VAL3" } },
            Ports = new() { "8080", "9090" }
        };
    }

    [Benchmark]
    public DeploymentDiff ComputeDiff()
    {
        return DeploymentDiff.Compute(_current, _proposed);
    }
}
