using ConcurrentPipelines.Channels.Extensions;
using ConcurrentPipelines.Common;
using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using ConcurrentPipelines.Common.Models;
using System.Threading.Channels;

namespace ConcurrentPipelines.Channels;

internal class ComplexPipeline : IPipeline
{
    public async Task RunAsync()
    {
        /*
         *                      | [resizeFullHdChannel] |
         * [downloadChannel] -> |                       | -> [saveChannel]
         *                      |   [resize2KChannel]   |
         */
        var downloadChannel = Channel.CreateUnbounded<DownloadInfo>();
        var resizeFullHdChannel = Channel.CreateUnbounded<ImageInfo>();
        var resize2KChannel = Channel.CreateUnbounded<ImageInfo>();
        var saveChannel = Channel.CreateUnbounded<ResizedImageInfo>();

        // Downloading image from internet
        var downloadingTask = downloadChannel.Reader.RunInBackground(async downloadInfo =>
        {
            ConsoleHelper.PrintBlockMessage("DownloadBlock", $"Downloading image #{downloadInfo.Id}...");

            var httpClient = new HttpClient();
            await using var stream = httpClient.GetStreamAsync(downloadInfo.Url).GetAwaiter().GetResult();

            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            // Broadcast message
            var resizeFhd = resizeFullHdChannel.Writer.WriteAsync(new ImageInfo(downloadInfo.Id, await GetStreamCopyAsync(ms)));
            var resize2K = resize2KChannel.Writer.WriteAsync(new ImageInfo(downloadInfo.Id, await GetStreamCopyAsync(ms)));

            await resizeFhd;
            await resize2K;
        });

        // Resizing image to FHD
        var resizingFullHdTask = resizeFullHdChannel.Reader.RunInBackground(async info =>
        {
            ConsoleHelper.PrintBlockMessage("ResizeFullHdBlock", $"Resizing image #{info.Id} to FHD...");

            var resizedImage = await ImageHelper.Resize(info, 1920, 1080);
            await saveChannel.Writer.WriteAsync(resizedImage);
        });

        // Resizing image to 2K
        var resizing2KTask = resize2KChannel.Reader.RunInBackground(async info =>
        {
            ConsoleHelper.PrintBlockMessage("Resize2KBlock", $"Resizing image #{info.Id} to 2K...");

            var resizedImage = await ImageHelper.Resize(info, 2560, 1440);
            await saveChannel.Writer.WriteAsync(resizedImage);
        });

        // Saving on filesystem
        var saveTask = saveChannel.Reader.RunInBackground(async resizedInfo =>
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, $"{resizedInfo.Id}-data-flow-img-{resizedInfo.Width}x{resizedInfo.Height}.png");
            ConsoleHelper.PrintBlockMessage("SaveBlock", $"Saving image #{resizedInfo.Id} with {resizedInfo.Width}x{resizedInfo.Height} resolution into '{Path.GetRelativePath(Environment.CurrentDirectory, filePath)}'...");

            await using var fs = File.Open(filePath, FileMode.OpenOrCreate);
            await resizedInfo.ImageStream.CopyToAsync(fs);
        });

        // Produce messages
        var i = 1;
        foreach (var url in Constants.Urls)
        {
            await downloadChannel.Writer.WriteAsync(new DownloadInfo(i++, url));
        }

        await downloadChannel.CompleteChannel(downloadingTask);
        await resizeFullHdChannel.CompleteChannel(resizingFullHdTask);
        await resize2KChannel.CompleteChannel(resizing2KTask);
        await saveChannel.CompleteChannel(saveTask);
    }

    private static async Task<Stream> GetStreamCopyAsync(Stream streamToCopy)
    {
        if (streamToCopy.CanSeek)
            streamToCopy.Seek(0, SeekOrigin.Begin);

        var ms = new MemoryStream();
        await streamToCopy.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);

        return ms;
    }
}