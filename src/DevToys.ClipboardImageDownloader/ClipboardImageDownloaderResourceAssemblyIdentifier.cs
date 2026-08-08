using System.ComponentModel.Composition;
using DevToys.Api;

namespace DevToys.ClipboardImageDownloader;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(ClipboardImageDownloaderResourceAssemblyIdentifier))]
internal sealed class ClipboardImageDownloaderResourceAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync() => ValueTask.FromResult(Array.Empty<FontDefinition>());
}
