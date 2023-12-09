using ConcurrentPipelines.Common.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using ImageInfo = ConcurrentPipelines.Common.Models.ImageInfo;

namespace ConcurrentPipelines.Common.Helpers;

public static class ImageHelper
{
    public static async Task<ResizedImageInfo> Resize(ImageInfo imageInfo, int width, int height)
    {
        using var img = await Image.LoadAsync(new DecoderOptions { SkipMetadata = true }, imageInfo.ImageStream);

        Console.WriteLine($"\t[Resize][Thread #{Environment.CurrentManagedThreadId}][{width}x{height}] Resizing image #{imageInfo.Id} from {img.Width}x{img.Height}...");

        img.Mutate(x => x.Resize(width, height));

        var ms = new MemoryStream();
        await img.SaveAsPngAsync(ms);
        ms.Position = 0;

        return new ResizedImageInfo(imageInfo.Id, ms, width, height);
    }
}