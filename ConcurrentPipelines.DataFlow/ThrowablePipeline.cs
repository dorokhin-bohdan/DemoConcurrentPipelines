using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Tasks.Dataflow;

namespace ConcurrentPipelines.DataFlow;

internal class ThrowablePipeline : IPipeline
{
    public async Task RunAsync()
    {
        var propagateCompletion = false;

        // Configure propagation
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = propagateCompletion };

        try
        {
            // Configure block options
            var executionOption = new ExecutionDataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded };

            // Buffer block for input data
            var bufferBlock = new BufferBlock<(int x, int y)>();

            // Divide block
            var divideBlock = new TransformBlock<(int x, int y), double>(tuple =>
            {
                var (x, y) = tuple;

                ConsoleHelper.PrintBlockMessage("DivideBlock", $"Dividing {x} by {y}...");
                var result = x / y;
                return result;

            }, executionOption);

            // Print block
            var printBlock = new ActionBlock<double>(result =>
            {
                ConsoleHelper.PrintBlockMessage("DivideBlock", $"Result ={result}");
            }, executionOption);

            /*
             * [bufferBlock] -> [divideBlock] -> [printBlock]
             */
            bufferBlock.LinkTo(divideBlock, linkOptions);
            divideBlock.LinkTo(printBlock, linkOptions);

            // Produce data
            var x = Random.Shared.Next(100);
            foreach (var y in Enumerable.Range(-5, 10))
            {
                await bufferBlock.SendAsync((x, y));
            }

            if (propagateCompletion)
            {
                // Complete star block
                bufferBlock.Complete();
                // Wait till last block process all data
                await printBlock.Completion;
                return;
            }

            // Complete buffer block
            bufferBlock.Complete();
            await bufferBlock.Completion;

            // Complete divide block
            divideBlock.Complete();
            await divideBlock.Completion;

            // Complete print block
            printBlock.Complete();
            await printBlock.Completion;
        }
        catch (DivideByZeroException)
        {
            ConsoleHelper.PrintBlockMessage("Error", "[DivideByZeroException] Unable to divide by zero...");
        }
        catch (AggregateException e)
        {
            ConsoleHelper.PrintBlockMessage("Error", $"[AggregateException] {e.Message}...");
        }
        catch (Exception e)
        {
            ConsoleHelper.PrintBlockMessage("Error", $"[Exception] {e.Message}...");
        }
    }
}