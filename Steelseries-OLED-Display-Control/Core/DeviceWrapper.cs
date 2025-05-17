using HidSharp;
using SteelseriesOledControl.Helpers;

namespace SteelseriesOledControl.Core;

public class DeviceWrapper(HidDevice device, int width = 128, int height = 64)
{
    public void SendFrame(byte[] bitmap)
    {
        var reports = CreateDrawReports(bitmap);
        Exception? lastEx = null;

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                using var stream = device.Open();
                foreach (var report in reports)
                {
                    stream.SetFeature(report);
                    Thread.Sleep(2);
                }
                return;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                Thread.Sleep(1000); // Подождать перед новой попыткой
            }
        }

        throw new Exception("Failed to send frame after retries", lastEx);
    }
    
    public void SetBrightness(Utils.BrightnessLevel level)
    {
        byte[] report = new byte[64];
        report[0] = 0x06;
        report[1] = 0x85;
        report[2] = (byte) level;
        using var stream = device.Open();
        stream.Write(report);
    }

    private byte[][] CreateDrawReports(byte[] bitmap)
    {
        int stride = width;
        var reports = new List<byte[]>();

        for (int x = 0; x < width; x += 64)
        {
            int w = Math.Min(64, width - x);
            int h = height;

            byte[] report = new byte[1024];
            report[0] = 0x06;
            report[1] = 0x93;
            report[2] = (byte)x;
            report[3] = 0;
            report[4] = (byte)w;
            report[5] = (byte)h;

            int stride_h = ((h + 7) / 8) * 8;

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    int ri = dx * stride_h + dy;
                    int pi = dy * stride + x + dx;
                    if (pi < bitmap.Length && bitmap[pi] != 0)
                    {
                        report[(ri / 8) + 6] |= (byte)(1 << (ri % 8));
                    }
                }
            }
            reports.Add(report);
        }
        return reports.ToArray();
    }
}