using BenchmarkDotNet.Running;

namespace CoolifyCli.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<TemplateBenchmarks>();
        BenchmarkRunner.Run<DeploymentBenchmarks>();
    }
}
