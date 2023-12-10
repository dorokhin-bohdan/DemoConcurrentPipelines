using BenchmarkDotNet.Attributes;
using System.Threading.Tasks.Dataflow;

namespace ConcurrentPipelines.Benchmarks.DataFlow;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class BlockBenchmark
{
    private BufferBlock<int> _bufferBlock = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _bufferBlock = new BufferBlock<int>();
    }


    [Benchmark]
    [Arguments(1)]
    [Arguments(1_000)]
    [Arguments(100_000)]
    [Arguments(1_000_000)]
    public async Task BufferBlock_WriteThenRead(int itemsCount)
    {
        foreach (var i in Enumerable.Range(1, itemsCount))
        {
            _bufferBlock.Post(i);
            await _bufferBlock.ReceiveAsync();
        }
    }

    [Benchmark]
    [Arguments(1)]
    [Arguments(1_000)]
    [Arguments(100_000)]
    [Arguments(1_000_000)]
    public async Task BufferBlock_ReadThenWrite(int itemsCount)
    {
        foreach (var i in Enumerable.Range(1, itemsCount))
        {
            var t = _bufferBlock.ReceiveAsync();
            _bufferBlock.Post(i);
            await t;
        }
    }
}