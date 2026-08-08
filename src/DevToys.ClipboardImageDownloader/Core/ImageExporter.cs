using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.ClipboardImageDownloader.Core;

public static class ImageExporter
{
    public static Size CalculateSize(int sourceWidth, int sourceHeight, int width, int height, bool keepAspectRatio)
    {
        width = width <= 0 ? sourceWidth : width;
        height = height <= 0 ? sourceHeight : height;
        if (!keepAspectRatio) return new Size(width, height);

        double ratio = Math.Min(width / (double)sourceWidth, height / (double)sourceHeight);
        return new Size(Math.Max(1, (int)Math.Round(sourceWidth * ratio)), Math.Max(1, (int)Math.Round(sourceHeight * ratio)));
    }

    public static Image Render(Image source, int width, int height, bool keepAspectRatio)
    {
        Size target = CalculateSize(source.Width, source.Height, width, height, keepAspectRatio);
        Image result = source.CloneAs<Rgba32>();
        if (result.Width != target.Width || result.Height != target.Height)
            result.Mutate(x => x.Resize(target.Width, target.Height, KnownResamplers.Lanczos3));
        return result;
    }

    public static IImageEncoder Encoder(string format, int quality) => format.ToUpperInvariant() switch
    {
        "JPEG" or "JPG" => new JpegEncoder { Quality = Math.Clamp(quality, 1, 100) },
        "WEBP" => new WebpEncoder { Quality = Math.Clamp(quality, 1, 100) },
        "BMP" => new BmpEncoder(),
        "GIF" => new GifEncoder(),
        _ => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }
    };
}
