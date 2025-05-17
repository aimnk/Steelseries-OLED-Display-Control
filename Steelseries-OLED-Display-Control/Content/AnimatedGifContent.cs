using SkiaSharp;
using SteelseriesOledControl;
using SteelseriesOledControl.Core;

public class AnimatedGifContent : DisplayContent
{
    private readonly List<SKBitmap> _frames;
    private int _currentFrame = 0;

    public AnimatedGifContent(string gifPath)
    {
        _frames = LoadGifFrames(gifPath);
    }

    public override void Render(byte[] buffer, int width, int height)
    {
        if (_frames.Count == 0)
            return;

        var frame = _frames[_currentFrame];

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Gray8));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        float scaleX = (float)width / frame.Width;
        float scaleY = (float)height / frame.Height;
        float scale = Math.Min(scaleX, scaleY);

        int targetWidth = (int)(frame.Width * scale);
        int targetHeight = (int)(frame.Height * scale);

        int offsetX = (width - targetWidth) / 2;
        int offsetY = (height - targetHeight) / 2;

        var resized = frame.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.Medium);
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

        _currentFrame = (_currentFrame + 1) % _frames.Count;
    }

    private static List<SKBitmap> LoadGifFrames(string path)
    {
        var frames = new List<SKBitmap>();
        using var stream = File.OpenRead(path);
        var codec = SKCodec.Create(stream);
        var frameCount = codec.FrameCount;

        for (int i = 0; i < frameCount; i++)
        {
            var info = codec.Info;
            var bitmap = new SKBitmap(info.Width, info.Height);
            var options = new SKCodecOptions(i);
            codec.GetPixels(bitmap.Info, bitmap.GetPixels(), options);
            frames.Add(bitmap.Copy());
        }

        return frames;
    }
}
