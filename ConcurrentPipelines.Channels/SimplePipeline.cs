using ConcurrentPipelines.Channels.Extensions;
using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Channels;

namespace ConcurrentPipelines.Channels;

internal class SimplePipeline : IPipeline
{
    public async Task RunAsync()
    {
        var multipleChannel = Channel.CreateUnbounded<int>();
        var printChannel = Channel.CreateUnbounded<int>();

        /*
         * [multipleChannel] -> [printChannel]
         */
        var multipleTask = multipleChannel.Reader.RunInBackground(async num =>
        {
            ConsoleHelper.PrintBlockMessage("MultipleChannel", $"Multiplying {num} by 2...");
            var doubledNum = num * 2;

            // Pass result to the next channel
            await printChannel.Writer.WriteAsync(doubledNum);
        });

        var printTask = printChannel.Reader.RunInBackground(num =>
        {
            ConsoleHelper.PrintBlockMessage("PrintChannel", $"Received: {num}");
            return Task.CompletedTask;
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