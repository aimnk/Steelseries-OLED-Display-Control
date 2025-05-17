using SkiaSharp;

namespace SteelseriesOledControl.Helpers;

public class Utils
{
    public static SKBitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        return SKBitmap.Decode(stream);
    }
    
    public enum ContentAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
    
    public enum BrightnessLevel : byte
    {
        Min = 0x01,
        Dim = 0x03,
        Medium = 0x06,
        Bright = 0x08,
        Max = 0x0A
    }

}