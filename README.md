# Clipboard Image Downloader for DevToys

Load an image directly from the system clipboard, preview it, resize it and save it in another format without uploading it anywhere.

Options: PNG/JPEG/WebP/BMP/GIF, quality 1–100, width and height up to 32768 px, aspect-ratio preservation, exact sizing, original-size and 50% presets.

## Build

```powershell
dotnet restore ClipboardImageDownloader.slnx
dotnet test ClipboardImageDownloader.slnx -c Release
dotnet pack src/DevToys.ClipboardImageDownloader/DevToys.ClipboardImageDownloader.csproj -c Release -o artifacts
```

Install the generated `.nupkg` from DevToys 2 Preview's **Manage extensions** page.

## Publishing

NuGet publishing uses GitHub OIDC Trusted Publishing. Configure the `NUGET_USER`
repository variable and register `.github/workflows/nuget-publish.yml` as the
trusted publisher on nuget.org. Push a `v*` tag or run the workflow manually.

All processing is local. No telemetry or network requests are used.
