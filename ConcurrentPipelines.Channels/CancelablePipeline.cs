using ConcurrentPipelines.Channels.Extensions;
using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Channels;

namespace ConcurrentPipelines.Channels;

internal class CancelablePipeline : IPipeline
{
    public async Task RunAsync()
    {
        var delay = TimeSpan.FromSeconds(2);
        var cts = new CancellationTokenSource();
        cts.Token.Register(() => ConsoleHelper.PrintBlockMessage("CancellationToken", "Cancellation requested..."));

        /*
         * [startChannel] -> [heavyOperationChannel] -> [endChannel]
         */
        var startChannel = Channel.CreateUnbounded<int>(); // Want more? Check what happen when channels will be bounded
        var heavyOperationChannel = Channel.CreateUnbounded<int>();
        var endChannel = Channel.CreateUnbounded<int>();

        var startTask = startChannel.Reader.RunInBackground(async i =>
        {
            ConsoleHelper.PrintBlockMessage("StartBlock", $"Starting operation #{i}");

            // Pass data to the next channel
            // ReSharper disable once MethodSupportsCancellation
            await heavyOperationChannel.Writer.WriteAsync(i);
            //await heavyOperationChannel.Writer.WriteAsync(i, cts.Token);
        }, cts.Token);

        var heavyOperationTask = heavyOperationChannel.Reader.RunInBackground(async i =>
        {
            ConsoleHelper.PrintBlockMessage("HeavyOperation", $"Running operation #{i}...");

            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(delay);

            // Pass data to the next channel
            // ReSharper disable once MethodSupportsCancellation
            await endChannel.Writer.WriteAsync(i);
            //await endChannel.Writer.WriteAsync(i,cts.Token);
        }, cts.Token);

        var endTask = endChannel.Reader.RunInBackground(i =>
            ConsoleHelper.PrintBlockMessage("EndBlock", $"Operation #{i} completed..."), cts.Token);

        // Request cancellation
        cts.CancelAfter(delay * 4);

        // Produce data
        foreach (var i in Enumerable.Range(1, 10))
        {
            // ReSharper disable once MethodSupportsCancellation
            await startChannel.Writer.WriteAsync(i);
            //await startChannel.Writer.WriteAsync(i, cts.Token);
        }

        try
        {
            await startChannel.CompleteChannel(startTask);
            await heavyOperationChannel.CompleteChannel(heavyOperationTask);
            await endChannel.CompleteChannel(endTask);
        }
        catch (Exception e)
        {
            ConsoleHelper.PrintBlockMessage("UnhandledException", $"[{e.GetType().Name}] {e.Message}");
        }
    }
}