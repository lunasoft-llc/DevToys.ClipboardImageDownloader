using System.ComponentModel.Composition;
using DevToys.Api;
using DevToys.ClipboardImageDownloader.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static DevToys.Api.GUI;

namespace DevToys.ClipboardImageDownloader.Gui;

[Export(typeof(IGuiTool))]
[Name("Clipboard Image Downloader")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons", IconGlyph = '\uE155',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(ClipboardImageDownloaderResourceAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.ClipboardImageDownloader.Strings.ClipboardImageDownloader",
    ShortDisplayTitleResourceName = "ShortDisplayTitle", LongDisplayTitleResourceName = "LongDisplayTitle",
    DescriptionResourceName = "Description", AccessibleNameResourceName = "AccessibleName",
    SearchKeywordsResourceName = "SearchKeywords")]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Image)]
[NoCompactOverlaySupport]
internal sealed class ClipboardImageDownloaderGuiTool : IGuiTool, IDisposable
{
    private enum Rows { Toolbar, Workspace }
    private enum Columns { Main }
    private enum WorkspaceRows { Content }
    private enum WorkspaceColumns { Settings, Preview }

    private readonly IClipboard _clipboard;
    private readonly IUIImageViewer _preview = ImageViewer("clipboard-image-preview");
    private readonly IUISelectDropDownList _format = SelectDropDownList("image-format");
    private readonly IUINumberInput _quality = NumberInput("image-quality", 1, 100, 1);
    private readonly IUINumberInput _width = NumberInput("image-width", 1, 32768, 1);
    private readonly IUINumberInput _height = NumberInput("image-height", 1, 32768, 1);
    private readonly IUISelectDropDownList _fit = SelectDropDownList("image-fit");
    private readonly IUILabel _summary = Label().Text("No image loaded");
    private Image? _source;
    private Image? _output;

    [ImportingConstructor]
    public ClipboardImageDownloaderGuiTool(IClipboard clipboard)
    {
        _clipboard = clipboard;
        _format.WithItems("PNG", "JPEG", "WebP", "BMP", "GIF").Select(0);
        _quality.Value(90).OnValueChanged(UpdatePreview);
        _width.Value(1920).OnValueChanged(UpdatePreview);
        _height.Value(1080).OnValueChanged(UpdatePreview);
        _fit.WithItems("Keep aspect ratio", "Exact size").OnItemSelected(_ => UpdatePreview()).Select(0);
        _format.OnItemSelected(_ => UpdatePreview());
    }

    public UIToolView View => new(false,
        Grid().RowMediumSpacing()
            .Rows((Rows.Toolbar, Auto), (Rows.Workspace, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Columns((Columns.Main, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Cells(
                Cell(Rows.Toolbar, Columns.Main,
                    Stack().Horizontal().MediumSpacing().WithChildren(
                        Button("load-clipboard").Text("Paste image").AccentAppearance().OnClick(LoadClipboardAsync),
                        Label().Text("Copy any image, then paste it here.").AlignVertically(UIVerticalAlignment.Center))),
                Cell(Rows.Workspace, Columns.Main,
                    Grid().ColumnMediumSpacing()
                        .Rows((WorkspaceRows.Content, new UIGridLength(1, UIGridUnitType.Fraction)))
                        .Columns((WorkspaceColumns.Settings, 360),
                            (WorkspaceColumns.Preview, new UIGridLength(1, UIGridUnitType.Fraction)))
                        .Cells(
                            Cell(WorkspaceRows.Content, WorkspaceColumns.Settings,
                                Stack().Vertical().MediumSpacing().WithChildren(
                                    Label().Text("Export settings").Style(UILabelStyle.Subtitle),
                                    _format.Title("File format"),
                                    _quality.Title("Quality (JPEG / WebP)"),
                                    _fit.Title("Resize mode"),
                                    _width.Title("Width (px)"),
                                    _height.Title("Height (px)"),
                                    Stack().Horizontal().SmallSpacing().WithChildren(
                                        Button("original-size").Text("Original").OnClick(UseOriginalSizeAsync),
                                        Button("half-size").Text("50%").OnClick(HalfSizeAsync)),
                                    _summary)),
                            Cell(WorkspaceRows.Content, WorkspaceColumns.Preview,
                                _preview.Title("Preview — select Save as to download"))))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Image && parsedData is Image image)
            SetSource(image.CloneAs<Rgba32>());
    }

    public void Dispose()
    {
        _source?.Dispose();
        _output?.Dispose();
    }

    private async ValueTask LoadClipboardAsync()
    {
        Image? image = await _clipboard.GetClipboardImageAsync();
        if (image is null) { _summary.Text("Clipboard does not contain an image."); return; }
        SetSource(image);
    }

    private ValueTask UseOriginalSizeAsync()
    {
        if (_source is not null) { _width.Value(_source.Width); _height.Value(_source.Height); UpdatePreview(); }
        return ValueTask.CompletedTask;
    }

    private ValueTask HalfSizeAsync()
    {
        if (_source is not null) { _width.Value(Math.Max(1, _source.Width / 2)); _height.Value(Math.Max(1, _source.Height / 2)); UpdatePreview(); }
        return ValueTask.CompletedTask;
    }

    private void SetSource(Image image)
    {
        _source?.Dispose();
        _source = image;
        _width.Value(image.Width);
        _height.Value(image.Height);
        UpdatePreview();
    }

    private void UpdatePreview(double _ = 0)
    {
        if (_source is null) return;
        _output?.Dispose();
        _output = ImageExporter.Render(_source, (int)_width.Value, (int)_height.Value, _fit.SelectedItem?.Text != "Exact size");
        _preview.WithImage(_output, false);

        string format = _format.SelectedItem?.Text ?? "PNG";
        _preview.ManuallyHandleSaveAs('.' + format.ToLowerInvariant().Replace("jpeg", "jpg"), async stream =>
            await _output.SaveAsync(stream, ImageExporter.Encoder(format, (int)_quality.Value)));

        string quality = format is "JPEG" or "WebP" ? $" • Quality {(int)_quality.Value}%" : string.Empty;
        _summary.Text($"{_source.Width} × {_source.Height} → {_output.Width} × {_output.Height}\n{format}{quality}");
    }
}

internal static class DropDownItemExtensions
{
    public static IUISelectDropDownList WithItems(this IUISelectDropDownList list, params string[] values)
        => list.WithItems(values.Select(value => GUI.Item(value)).ToArray());
}
