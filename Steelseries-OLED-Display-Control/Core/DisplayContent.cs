namespace SteelseriesOledControl.Core;

public abstract class DisplayContent
{
    public abstract void Render(byte[] buffer, int width, int height);
}