
using SteelseriesOledControl.Core;

namespace SteelseriesOledControl;

internal class WhiteContent : DisplayContent
{
    public override void Render(byte[] buffer, int width, int height)
    {
        for (int i = 0; i < buffer.Length; i++) buffer[i] = 0xFF;
    }
}