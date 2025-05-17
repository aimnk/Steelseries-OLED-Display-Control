using System.Runtime.InteropServices;

namespace SteelseriesOledControl.Core
{
    public static class HidNativeInterop
    {
        [DllImport("libsteelseries.dylib", CallingConvention = CallingConvention.Cdecl)]
        public static extern int disable_steelseries_ui();
    }
}