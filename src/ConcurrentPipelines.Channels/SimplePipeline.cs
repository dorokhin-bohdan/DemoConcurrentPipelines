using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Channels;

namespace ConcurrentPipelines.Channels;

internal class SimplePipeline : IPipeline
{
    public async Task RunAsync()
    {
        /*
         * [multipleChannel] -> [printChannel]
         */
        var multipleChannel = Channel.CreateUnbounded<int>();
        var printChannel = Channel.CreateUnbounded<int>();

        var multipleTask = Task.Run(async delegate
        {
            await foreach (var item in multipleChannel.Reader.ReadAllAsync())
            {
                ConsoleHelper.PrintBlockMessage("MultipleChannel", $"Multiplying {item} by 2...");
                var doubledNum = item * 2;

                // Pass result to the next channel
                await printChannel.Writer.WriteAsync(doubledNum);
            }
        });

        var printTask = Task.Run(async delegate
        {
            await foreach (var item in printChannel.Reader.ReadAllAsync())
            {
                ConsoleHelper.PrintBlockMessage("PrintChannel", $"Received: {item}");
            }
        });

        // Produce data
        foreach (var num in Enumerable.Range(1, 10))
        {
            await multipleChannel.Writer.WriteAsync(num);
        }

        // Mark channel as completed (no more items received)
        multipleChannel.Writer.Complete();
        // Wait till all elements were consumed
        await multipleChannel.Reader.Completion;
        // Wait all all elements were processed
        await multipleTask;

        // Mark channel as completed (no more items received)
        printChannel.Writer.Complete();
        // Wait till all elements were consumed
        await printChannel.Reader.Completion;
        // Wait all all elements were processed
        await printTask;
    }
}