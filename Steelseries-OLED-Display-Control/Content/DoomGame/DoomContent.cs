using SkiaSharp;
using SteelseriesOledControl;
using SteelseriesOledControl.Core;

/// <summary>
/// Doom 0.5
/// </summary>
public class DoomContent : DisplayContent, IDynamicContent
{
    private float _playerX = 3.5f;
    private float _playerY = 3.5f;
    private float _angle = 0;
    private readonly float _fov = (float)(Math.PI / 3);
    private readonly int[,] _map = new int[8, 8]
    {
        { 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 0, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 1, 0, 0, 1 },
        { 1, 0, 1, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    private DateTime _lastUpdate = DateTime.MinValue;

    public override void Render(byte[] buffer, int width, int height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Gray8));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        int step = 2;

        for (int x = 0; x < width; x += step)
        {
            float rayAngle = (_angle - _fov / 2f) + (_fov * x / width);

            float distanceToWall = 0;
            bool hitWall = false;

            float eyeX = (float)Math.Cos(rayAngle);
            float eyeY = (float)Math.Sin(rayAngle);

            while (!hitWall && distanceToWall < 16f)
            {
                distanceToWall += 0.2f;
                int testX = (int)(_playerX + eyeX * distanceToWall);
                int testY = (int)(_playerY + eyeY * distanceToWall);

                if (testX < 0 || testX >= 8 || testY < 0 || testY >= 8)
                {
                    hitWall = true;
                    distanceToWall = 16f;
                }
                else if (_map[testY, testX] == 1)
                {
                    hitWall = true;
                }
            }

            int lineHeight = (int)(height / distanceToWall);
            int drawStart = Math.Max(0, (height - lineHeight) / 2);
            int drawEnd = Math.Min(height, (height + lineHeight) / 2);

            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = false };            
            canvas.DrawRect(x, drawStart, step, drawEnd - drawStart, paint);
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

    public bool NeedsUpdate()
    {
        if ((DateTime.UtcNow - _lastUpdate).TotalMilliseconds > 50)
        {
            _lastUpdate = DateTime.UtcNow;
            _angle += 0.03f;
            return true;
        }
        return false;
    }
} 