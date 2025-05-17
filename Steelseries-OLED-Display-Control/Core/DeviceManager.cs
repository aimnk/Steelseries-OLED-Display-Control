using HidSharp;

namespace SteelseriesOledControl.Core;

public class DeviceManager
{
    public DeviceWrapper? OledDevice { get; private set; }
    public HidDevice? InfoDevice { get; private set; }

        
    public bool Initialize(HidDevice baseDevice)
    {
        try
        {
            InfoDevice = baseDevice;
            OledDevice = new DeviceWrapper(baseDevice);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Initialization failed: {ex.Message}");
            return false;
        }
    }
}