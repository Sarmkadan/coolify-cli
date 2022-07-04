```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD EPYC-Rome Processor 2.45GHz, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), X64 RyuJIT x86-64-v3


```
| Method      | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------------ |---------:|----------:|----------:|-------:|-------:|----------:|
| ComputeDiff | 1.702 μs | 0.0330 μs | 0.0309 μs | 0.3548 | 0.0019 |   2.91 KB |
