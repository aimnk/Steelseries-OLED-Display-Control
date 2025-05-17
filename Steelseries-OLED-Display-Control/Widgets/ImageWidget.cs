using SteelseriesOledControl.Core;
using SteelseriesOledControl.Helpers;

namespace SteelseriesOledControl.Widgets;

public class LogoWidget : IWidgetFactory
{
    public string Name => "Logo";

    public void Create(DisplayController controller, DisplaySettings settings)
    {
        var iconPath = settings.Widgets.FirstOrDefault(x => x.Type == Name)?.Params.GetValueOrDefault("icon");

        if (string.IsNullOrEmpty(iconPath))
        {
            Console.WriteLine($"[WIDGET LOG] {Name} Icon Path invalid");
            return;
        }
        
        var fullPath = Path.Combine(AppContext.BaseDirectory, iconPath);
        
        var bmp = Utils.LoadBitmap(fullPath);
        controller.AddContent(new ImageContent(bmp));
    }
}