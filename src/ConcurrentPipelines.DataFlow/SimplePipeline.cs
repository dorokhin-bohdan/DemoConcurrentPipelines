using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Tasks.Dataflow;

namespace ConcurrentPipelines.DataFlow;

internal class SimplePipeline : IPipeline
{
    public async Task RunAsync()
    {
        var multipleBlock = new TransformBlock<int, int>(i =>
        {
            ConsoleHelper.PrintBlockMessage("MultipleBlock", $"Multiplying {i} by 2...");
            return i * 2;
        });
        var printBlock = new ActionBlock<int>(i => ConsoleHelper.PrintBlockMessage("PrintBlock", $"Received: {i}"));

        /*
         * [multipleBlock] -> [printBlock]
         */
        multipleBlock.LinkTo(printBlock);

        foreach (var num in Enumerable.Range(1, 10))
        {
            await multipleBlock.SendAsync(num);
        }

        multipleBlock.Complete();
        await multipleBlock.Completion;

        printBlock.Complete();
        await printBlock.Completion;
    }
}