using SkiaSharp;
using System.Globalization;
using SteelseriesOledControl.Core;

namespace SteelseriesOledControl;

internal class TimeContent : DisplayContent
{
    public override void Render(byte[] buffer, int width, int height)
    {
        var now = DateTime.Now;
        string currentTime = now.ToString("HH:mm");
        string currentDate = now.ToString("ddd dd MMM", new CultureInfo("ru-RU")); // Пример: пн 20 мая

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Black);

        using var paintTime = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 28,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Consolas", SKFontStyle.Normal)
        };

        using var paintDate = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 14,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Consolas", SKFontStyle.Normal)
        };

        // Время — по центру, чуть выше
        float timeWidth = paintTime.MeasureText(currentTime);
        paintTime.GetFontMetrics(out var timeMetrics);
        float timeHeight = timeMetrics.Descent - timeMetrics.Ascent;
        float timeX = (width - timeWidth) / 2;
        float timeY = (height / 2) - 4;

        // Дата — по центру, ниже
        float dateWidth = paintDate.MeasureText(currentDate);
        float dateX = (width - dateWidth) / 2;
        float dateY = timeY + timeHeight;

        canvas.DrawText(currentTime, timeX, timeY, paintTime);
        canvas.DrawText(currentDate, dateX, dateY, paintDate);
        canvas.Flush();

        for (int yb = 0; yb < height; yb++)
        {
            for (int xb = 0; xb < width; xb++)
            {
                var color = bitmap.GetPixel(xb, yb);
                int i = yb * width + xb;
                buffer[i] = color.Red > 128 ? (byte)1 : (byte)0;
            }
        }
    }
}
