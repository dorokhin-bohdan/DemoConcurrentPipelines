using BenchmarkDotNet.Running;
using ConcurrentPipelines.Benchmarks.Channels;

_ = BenchmarkRunner.Run<ChannelBenchmark>();