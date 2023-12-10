using System.Threading.Channels;

namespace ConcurrentPipelines.Channels.Extensions;

public static class ChannelHelper
{
    public static async Task CompleteChannel<TItem>(this Channel<TItem> channel, Task backgroundTask)
    {
        // Mark channel as completed (No more items receive)
        channel.Writer.TryComplete();
        // Wait till last block complete processing all items
        await channel.Reader.Completion;
        // Wait till task complete
        await backgroundTask;
    }
}