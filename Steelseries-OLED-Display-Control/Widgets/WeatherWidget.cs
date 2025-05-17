using System.Globalization;
using SteelseriesOledControl.Core;
using SteelseriesOledControl.Helpers;


namespace SteelseriesOledControl.Widgets;

public class WeatherWidget : IWidgetFactory
{
    public string Name => "Weather";

    public void Create(DisplayController controller, DisplaySettings settings)
    {
        var content = new IconTextContent("Загрузка погоды...", 16, Utils.ContentAlignment.MiddleCenter);
        controller.AddContent(content);

        var widgetConf = settings.Widgets.FirstOrDefault(x => x.Type == Name);

        if (!float.TryParse(widgetConf.Params.GetValueOrDefault("lat"), CultureInfo.InvariantCulture, out var lat))
        {
            lat = 52.23f;
            Console.WriteLine($"[WIDGET LOG] {Name} Lat invalid");
        }
        
        if (!float.TryParse(widgetConf.Params.GetValueOrDefault("lon"),CultureInfo.InvariantCulture, out var lon))
        {
            lon = 21.01f;
            Console.WriteLine($"[WIDGET LOG] {Name} Lon invalid");
        }
        
        
        var weatherService = new WeatherService(lat, lon);

        _ = Task.Run(async () =>
        {
            while (true)
            {
                var weather = weatherService.GetCachedWeather();
                if (weather != null)
                {
                    var icon = weatherService.WeatherCodeToSymbol(weather.WeatherCode);
                    content.Text = $"{icon} {weather.City}\n{weather.Temperature:0}°C \n WIND {weather.WindSpeed} km/h";
                }

                await Task.Delay(3000);
            }
        });
    }
}