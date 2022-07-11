using BenchmarkDotNet.Attributes;
using CoolifyCli.Models;

namespace CoolifyCli.Benchmarks;

/// <summary>
/// Deployment benchmark class.
/// </summary>
[MemoryDiagnoser]
public class DeploymentBenchmarks
{
    /// <summary>
    /// The current application deployment.
    /// </summary>
    private ApplicationDeployment _current = null!;

    /// <summary>
    /// The proposed application deployment.
    /// </summary>
    private ApplicationDeployment _proposed = null!;

    /// <summary>
    /// Sets up the benchmark by initializing the current and proposed deployments.
    /// </summary>
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

    /// <summary>
    /// Computes the deployment difference between the current and proposed deployments.
    /// </summary>
    /// <returns>The deployment difference.</returns>
    [Benchmark]
    public DeploymentDiff ComputeDiff()
    {
        return DeploymentDiff.Compute(_current, _proposed);
    }
}
