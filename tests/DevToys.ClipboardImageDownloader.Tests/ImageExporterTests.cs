using DevToys.ClipboardImageDownloader.Core;
using Xunit;

namespace DevToys.ClipboardImageDownloader.Tests;

public sealed class ImageExporterTests
{
    [Theory]
    [InlineData(4000, 2000, 1920, 1080, true, 1920, 960)]
    [InlineData(4000, 2000, 800, 800, true, 800, 400)]
    [InlineData(4000, 2000, 800, 800, false, 800, 800)]
    public void CalculateSize_ReturnsExpectedSize(int sw, int sh, int w, int h, bool keep, int ew, int eh)
    {
        var result = ImageExporter.CalculateSize(sw, sh, w, h, keep);
        Assert.Equal(ew, result.Width); Assert.Equal(eh, result.Height);
    }
}
