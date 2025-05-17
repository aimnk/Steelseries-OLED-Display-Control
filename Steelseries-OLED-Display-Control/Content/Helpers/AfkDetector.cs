using System;
using System.Runtime.InteropServices;

public static class AfkDetector
{
    public static TimeSpan GetIdleTime()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetIdleWindows();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetIdleMac();

        return TimeSpan.Zero;
    }

    // Windows: GetLastInputInfo
    private static TimeSpan GetIdleWindows()
    {
        LASTINPUTINFO info = new()
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
        };
        if (!GetLastInputInfo(ref info)) return TimeSpan.Zero;

        uint idleTime = unchecked((uint)Environment.TickCount - info.dwTime);
        return TimeSpan.FromMilliseconds(idleTime);
    }

    // macOS: CGEventSourceSecondsSinceLastEventType
    private static TimeSpan GetIdleMac()
    {
        double seconds = CGEventSourceSecondsSinceLastEventType(0, 0xffff); // all events
        return TimeSpan.FromSeconds(seconds);
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern double CGEventSourceSecondsSinceLastEventType(int sourceStateID, int eventType);
}