# Performance Benchmarks

This directory contains performance benchmarks for `coolify-cli` using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Running Benchmarks

To run the benchmarks, execute the following command from the project root:

```bash
dotnet run -c Release --project benchmarks/coolify-cli.Benchmarks/coolify-cli.Benchmarks.csproj
```

## Benchmarks Included

- **TemplateBenchmarks**: Measures throughput and memory allocation for template variable expansion.
- **DeploymentBenchmarks**: Measures throughput and memory allocation for deployment diff computation.

## Recent Results

| Method | Mean | Error | StdDev | Gen0 | Gen1 | Allocated |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TemplateBenchmarks.ExpandTemplate** | 1.957 us | 0.0385 us | 0.0778 us | 0.3319 | - | 2.73 KB |
| **DeploymentBenchmarks.ComputeDiff** | 1.702 us | 0.0330 us | 0.0309 us | 0.3548 | 0.0019 | 2.91 KB |
