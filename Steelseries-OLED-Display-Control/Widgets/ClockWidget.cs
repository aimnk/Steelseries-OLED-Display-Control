using SteelseriesOledControl.Core;

namespace SteelseriesOledControl.Widgets;

public class ClockWidget : IWidgetFactory
{
    public string Name => "Clock";

    public void Create(DisplayController controller, DisplaySettings settings)
    {
        controller.AddContent(new TimeContent());
    }
}