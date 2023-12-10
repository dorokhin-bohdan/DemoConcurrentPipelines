using System.Threading.Channels;

namespace ConcurrentPipelines.Channels.Extensions;

public static class ChannelReaderExtensions
{
    public static async Task RunInBackground<TItem>(this ChannelReader<TItem> channelReader, Func<TItem, Task> action, CancellationToken cancellationToken = default)
    {
        await foreach (var item in channelReader.ReadAllAsync(cancellationToken))
        {
            await action(item);
        }
    }

    public static async Task RunInBackground<TItem>(this ChannelReader<TItem> channelReader, Action<TItem> action, CancellationToken cancellationToken = default)
    {
        await foreach (var item in channelReader.ReadAllAsync(cancellationToken))
        {
            action(item);
        }
    }
}