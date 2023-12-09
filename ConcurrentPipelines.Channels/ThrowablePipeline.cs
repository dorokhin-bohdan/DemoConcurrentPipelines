using ConcurrentPipelines.Channels.Extensions;
using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using System.Threading.Channels;

namespace ConcurrentPipelines.Channels;

internal class ThrowablePipeline : IPipeline
{
    public async Task RunAsync()
    {
        // Divide channel
        var divideChannel = Channel.CreateUnbounded<(int x, int y)>();
        // Print channel
        var printChannel = Channel.CreateUnbounded<double>();

        var divideTask = divideChannel.Reader.RunInBackground(tuple =>
        {
            var (x, y) = tuple;

            try
            {
                ConsoleHelper.PrintBlockMessage("DivideBlock", $"Dividing {x} by {y}...");
                var result = x / y;

                // Pass result to the next channel
                printChannel.Writer.TryWrite(result);
            }
            catch (Exception e)
            {
                divideChannel.Writer.TryComplete(e); // Important to pass exception to the channel
                printChannel.Writer.TryComplete(e); // Propagate next channel
            }

            return Task.CompletedTask;
        });

        var printTask = printChannel.Reader.RunInBackground(num =>
        {
            ConsoleHelper.PrintBlockMessage("DivideBlock", $"Result ={num}");

            return Task.CompletedTask;
        });

        // Produce data
        var x = Random.Shared.Next(100);
        foreach (var y in Enumerable.Range(-5, 10))
        {
            divideChannel.Writer.TryWrite((x, y));
        }

        try
        {
            await divideChannel.CompleteChannel(divideTask);
            await printChannel.CompleteChannel(printTask);
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