using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Tasks.Dataflow;

namespace ConcurrentPipelines.DataFlow;

internal class CancelablePipeline : IPipeline
{
    public async Task RunAsync()
    {
        var delay = TimeSpan.FromSeconds(2);
        var cts = new CancellationTokenSource();
        cts.Token.Register(() => ConsoleHelper.PrintBlockMessage("CancellationToken", "Cancellation requested..."));

        var executionOption = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = DataflowBlockOptions.Unbounded,
            CancellationToken = cts.Token,
        };
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        var startBlock = new TransformBlock<int, int>(i =>
        {
            ConsoleHelper.PrintBlockMessage("StartBlock", $"Starting operation #{i}");
            return i;
        }, executionOption);

        var heavyOperationBlock = new TransformBlock<int, int>(async i =>
        {
            ConsoleHelper.PrintBlockMessage("HeavyOperation", $"Running operation #{i}...");

            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(delay);

            return i;
        }, executionOption);

        var endBlock =
            new ActionBlock<int>(i => ConsoleHelper.PrintBlockMessage("EndBlock", $"Operation #{i} completed..."), executionOption);

        /*
         * [startChannel] -> [heavyOperationChannel] -> [endChannel]
         */
        startBlock.LinkTo(heavyOperationBlock, linkOptions);
        heavyOperationBlock.LinkTo(endBlock, linkOptions);

        // Request cancellation
        cts.CancelAfter(delay * 4);

        // Produce data
        foreach (var i in Enumerable.Range(1, 10))
        {
            // ReSharper disable once MethodSupportsCancellation
            await startBlock.SendAsync(i);
        }

        try
        {
            startBlock.Complete();
            await endBlock.Completion;
        }
        catch (Exception e)
        {
            ConsoleHelper.PrintBlockMessage("UnhandledException", $"[{e.GetType().Name}] {e.Message}");
        }
    }
}