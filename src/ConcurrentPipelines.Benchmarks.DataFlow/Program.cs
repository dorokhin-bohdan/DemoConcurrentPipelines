using BenchmarkDotNet.Running;
using ConcurrentPipelines.Benchmarks.DataFlow;

_ = BenchmarkRunner.Run<BlockBenchmark>();