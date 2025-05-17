using SteelseriesOledControl.Core;
using SteelseriesOledControl.Helpers;

namespace SteelseriesOledControl.Widgets;

public class IconTextWidget: IWidgetFactory
{
    public string Name => "IconText";
    
    public void Create(DisplayController controller, DisplaySettings settings)
    {
        var widgetConf = settings.Widgets.FirstOrDefault(x => x.Type == Name);

        if (widgetConf == null)
        {
            Console.WriteLine("Widget configuration not found");
            return;
        }
        
        var text = widgetConf.Params.GetValueOrDefault("text");
        var iconPath = widgetConf.Params.GetValueOrDefault("icon");
        var fullPath = Path.Combine(AppContext.BaseDirectory, iconPath);
        
        if (!float.TryParse(widgetConf.Params.GetValueOrDefault("fontSize"), out var fontSize))
        {
            fontSize = 12f;
        }
        
        if (string.IsNullOrEmpty(fullPath))
        {
            controller.AddContent(new IconTextContent(text, fontSize, Utils.ContentAlignment.MiddleCenter));
            return;
        }

        var bmp = Utils.LoadBitmap(fullPath);
        controller.AddContent(new IconTextContent(text, fontSize, Utils.ContentAlignment.MiddleCenter, bmp));
    }
}