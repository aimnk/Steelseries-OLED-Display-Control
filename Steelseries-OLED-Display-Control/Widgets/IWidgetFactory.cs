using SteelseriesOledControl.Core;

namespace SteelseriesOledControl.Widgets;

public interface IWidgetFactory
{
    string Name { get; }
    void Create(DisplayController controller, DisplaySettings settings);
}