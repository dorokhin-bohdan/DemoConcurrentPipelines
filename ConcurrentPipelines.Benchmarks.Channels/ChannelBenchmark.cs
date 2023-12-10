using BenchmarkDotNet.Attributes;
using System.Threading.Channels;

namespace ConcurrentPipelines.Benchmarks.Channels;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class ChannelBenchmark
{
    private Channel<int> _channel = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _channel = Channel.CreateUnbounded<int>();
    }

    [Benchmark]
    [Arguments(1)]
    [Arguments(1_000)]
    [Arguments(100_000)]
    [Arguments(1_000_000)]
    public async Task Channel_WriteThenRead(int itemsCount)
    {
        var writer = _channel.Writer;
        var reader = _channel.Reader;

        foreach (var i in Enumerable.Range(1, itemsCount))
        {
            writer.TryWrite(i);
            await reader.ReadAsync();
        }
    }

    [Benchmark]
    [Arguments(1)]
    [Arguments(1_000)]
    [Arguments(100_000)]
    [Arguments(1_000_000)]
    public async Task Channel_ReadThenWrite(int itemsCount)
    {
        var writer = _channel.Writer;
        var reader = _channel.Reader;

        foreach (var i in Enumerable.Range(1, itemsCount))
        {
            var vt = reader.ReadAsync();
            writer.TryWrite(i);
            await vt;
        }
    }
}