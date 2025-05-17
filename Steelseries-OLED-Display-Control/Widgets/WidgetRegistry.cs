using SteelseriesOledControl.Core;
using SteelseriesOledControl.Widgets;

public static class WidgetRegistry
{
    private static readonly Dictionary<string, IWidgetFactory> _factories;

    static WidgetRegistry()
    {
        _factories = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IWidgetFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IWidgetFactory)Activator.CreateInstance(t))
            .ToDictionary(f => f.Name.ToLowerInvariant());
    }

    public static bool TryCreate(string name, DisplayController controller, DisplaySettings settings)
    {
        if (_factories.TryGetValue(name.ToLowerInvariant(), out var factory))
        {
            factory.Create(controller, settings);
            return true;
        }

        Console.WriteLine($"[WidgetRegistry] Unknown widget: {name}");
        return false;
    }
}