using ConcurrentPipelines.Common;
using ConcurrentPipelines.Common.Extensions;
using ConcurrentPipelines.Common.Helpers;
using ConcurrentPipelines.Common.Interfaces;
using ConcurrentPipelines.Common.Models;
using Spectre.Console;
using System.Threading.Tasks.Dataflow;
using ImageInfo = ConcurrentPipelines.Common.Models.ImageInfo;

namespace ConcurrentPipelines.DataFlow;

internal class ComplexPipeline : IPipeline
{
    public async Task RunAsync()
    {
        // Configure concurrency
        var executionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };

        // Complete all block if someone was completed or failed
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        // Downloading image from internet
        var downloadBlock = new TransformBlock<DownloadInfo, ImageInfo>(async downloadInfo =>
        {
            ConsoleHelper.PrintBlockMessage("DownloadBlock", $"Downloading image #{downloadInfo.Id}...");

            var httpClient = new HttpClient();
            await using var stream = httpClient.GetStreamAsync(downloadInfo.Url).GetAwaiter().GetResult();
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            return new ImageInfo(downloadInfo.Id, ms);
        }, executionOptions);

        // Broadcast message to all linked blocks
        var broadcastBlock = new BroadcastBlock<ImageInfo>(info =>
        {
            ConsoleHelper.PrintBlockMessage("BroadcastBlock", $"Broadcasting image #{info.Id}...");

            var ms = new MemoryStream();
            info.ImageStream.CopyTo(ms);
            ms.Position = 0;
            info.ImageStream.Position = 0;

            return info with { ImageStream = ms };
        });

        // Resizing image to FHD block
        var resizeFullHdBlock = new TransformBlock<ImageInfo, ResizedImageInfo>(info =>
        {
            ConsoleHelper.PrintBlockMessage("ResizeFullHdBlock", $"Resizing image #{info.Id} to FHD...");

            return ImageHelper.Resize(info, 1920, 1080);
        }, executionOptions);

        // Resizing image to 2K block
        var resize2KBlock = new TransformBlock<ImageInfo, ResizedImageInfo>(info =>
        {
            ConsoleHelper.PrintBlockMessage("Resize2KBlock", $"Resizing image #{info.Id} to 2K...");

            return ImageHelper.Resize(info, 2560, 1440);
        }, executionOptions);

        // Saving on filesystem block
        var saveBlock = new ActionBlock<ResizedImageInfo>(async resizedInfo =>
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, $"{resizedInfo.Id}-data-flow-img-{resizedInfo.Width}x{resizedInfo.Height}.png");
            ConsoleHelper.PrintBlockMessage("SaveBlock", $"Saving image #{resizedInfo.Id} with {resizedInfo.Width}x{resizedInfo.Height} resolution into '{Path.GetRelativePath(Environment.CurrentDirectory, filePath)}'...");

            await using var fs = File.Open(filePath, FileMode.OpenOrCreate);
            await resizedInfo.ImageStream.CopyToAsync(fs);
        });

        /*
         *                                       | [resizeFullHdBlock] |
         * [downloadBlock] -> [broadcastBlock] ->|                     | -> [saveBlock]
         *                                       |   [resize2KBlock]   |
         */
        downloadBlock.LinkTo(broadcastBlock, linkOptions);
        broadcastBlock.LinkTo(resizeFullHdBlock, linkOptions);
        broadcastBlock.LinkTo(resize2KBlock, linkOptions);
        resizeFullHdBlock.LinkTo(saveBlock, linkOptions);
        resize2KBlock.LinkTo(saveBlock, linkOptions);

        // Produce messages
        var i = 1;
        foreach (var url in Constants.Urls)
        {
            await downloadBlock.SendAsync(new DownloadInfo(i++, url));
        }

        // Mark & propagate block as completed (No more items receive)
        downloadBlock.Complete();

        // Wait till last block complete processing all items
        await saveBlock.Completion;
    }
}