using SkiaSharp;
using SteelseriesOledControl.Core;

namespace SteelseriesOledControl;
public class ImageContent : DisplayContent
{
    private readonly SKBitmap _image;

    public ImageContent(SKBitmap image)
    {
        _image = image;
    }

    public override void Render(byte[] buffer, int width, int height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Gray8));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        float scaleX = (float)width / _image.Width;
        float scaleY = (float)height / _image.Height;
        float scale = Math.Min(scaleX, scaleY);

        int targetWidth = (int)(_image.Width * scale);
        int targetHeight = (int)(_image.Height * scale);

        int offsetX = (width - targetWidth) / 2;
        int offsetY = (height - targetHeight) / 2;

        var resized = _image.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.Medium);
        if (resized != null)
        {
            canvas.DrawBitmap(resized, new SKRect(offsetX, offsetY, offsetX + targetWidth, offsetY + targetHeight));
        }

        using var img = surface.Snapshot();
        using var pix = img.PeekPixels();

        var handle = System.Runtime.InteropServices.GCHandle.Alloc(buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
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
}
