using System.Runtime.InteropServices;
using SkiaSharp;
using SteelseriesOledControl.Core;
using SteelseriesOledControl.Helpers;

namespace SteelseriesOledControl;
public class IconTextContent : DisplayContent, IDynamicContent
{
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                _dirty = true;
            }
        }
    }

    public SKBitmap Icon { get; set; }
    public Utils.ContentAlignment Alignment { get; set; } = Utils.ContentAlignment.TopLeft;

    private readonly SKPaint _paint;
    private bool _dirty = true;
    private string _text;

    public IconTextContent(string text, float sizeText = 12f, Utils.ContentAlignment alignment = Utils.ContentAlignment.TopLeft, SKBitmap icon = null)
    {
        Text = text;
        Icon = icon;
        Alignment = alignment;

        _paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = sizeText,
            IsAntialias = false,
            Typeface = SKTypeface.Default
        };
    }

    public override void Render(byte[] buffer, int width, int height)
    {
        _dirty = false;

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Gray8));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        int iconSize = 16;
        int padding = 2;

        var lines = Text.Replace("\r", "").Split('\n');
        float lineHeight = _paint.TextSize + 2;
        float totalTextHeight = lines.Length * lineHeight;
        float iconBlockHeight = Math.Max(iconSize, totalTextHeight);

        float startY = Alignment switch
        {
            Utils.ContentAlignment.TopLeft or Utils.ContentAlignment.TopCenter or Utils.ContentAlignment.TopRight => padding,
            Utils.ContentAlignment.MiddleLeft or Utils.ContentAlignment.MiddleCenter or Utils.ContentAlignment.MiddleRight => (height - iconBlockHeight) / 2,
            Utils.ContentAlignment.BottomLeft or Utils.ContentAlignment.BottomCenter or Utils.ContentAlignment.BottomRight => height - iconBlockHeight - padding,
            _ => padding
        };

        float iconOffsetX = 0;
        if (Icon != null)
        {
            float iconX = padding;
            float iconY = startY + (iconBlockHeight - iconSize) / 2;
            canvas.DrawBitmap(Icon, new SKRect(iconX, iconY, iconX + iconSize, iconY + iconSize));
            iconOffsetX = iconSize + padding;
        }

        float y = startY;
        foreach (var line in lines)
        {
            float textWidth = _paint.MeasureText(line);

            float x = Alignment switch
            {
                Utils.ContentAlignment.TopLeft or Utils.ContentAlignment.MiddleLeft or Utils.ContentAlignment.BottomLeft => padding + iconOffsetX,
                Utils.ContentAlignment.TopCenter or Utils.ContentAlignment.MiddleCenter or Utils.ContentAlignment.BottomCenter => (width - textWidth + iconOffsetX) / 2,
                Utils.ContentAlignment.TopRight or Utils.ContentAlignment.MiddleRight or Utils.ContentAlignment.BottomRight => width - textWidth - padding,
                _ => padding + iconOffsetX
            };

            canvas.DrawText(line, x, y + _paint.TextSize, _paint);
            y += lineHeight;
        }

        canvas.Flush();

        using var img = surface.Snapshot();
        using var pix = img.PeekPixels();

        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            pix.ReadPixels(new SKImageInfo(width, height, SKColorType.Gray8), ptr, width);
        }
        finally
        {
            handle.Free();
        }
    }

    public bool NeedsUpdate() => _dirty;
}
