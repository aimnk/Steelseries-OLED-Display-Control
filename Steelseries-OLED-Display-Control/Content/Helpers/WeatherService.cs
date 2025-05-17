using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

public class WeatherData
{
    public string City { get; set; } = "";
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int WeatherCode { get; set; }
}

public class WeatherService
{
    private readonly HttpClient _http = new();
    private WeatherData? _cached;
    private DateTime _lastUpdate = DateTime.MinValue;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(10);
    private readonly string _city;
    private readonly double _lat;
    private readonly double _lon;
    

    public WeatherService(double lat, double lon)
    {
        _lat = lat;
        _lon = lon;
        _ = Task.Run(UpdateLoop);
    }
    
    public WeatherData? GetCachedWeather()
    {
        return _cached;
    }

    public string WeatherCodeToSymbol(int code) => code switch
    {
        0 => "SUN",
        1 or 2 => "PARTLY",
        3 => "CLOUD",
        45 or 48 => "FOG",
        51 or 53 or 55 => "DRIZZLE",
        61 or 63 or 65 => "RAIN",
        66 or 67 => "MIX",
        71 or 73 or 75 => "SNOW",
        95 or 96 or 99 => "STORM",
        _ => "???"
    };


    private async Task UpdateLoop()
    {
        while (true)
        {
            try
            {
                string url = $"https://api.open-meteo.com/v1/forecast?latitude={_lat.ToString(CultureInfo.InvariantCulture)}&longitude={_lon.ToString(CultureInfo.InvariantCulture)}&current_weather=true";

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    continue;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    Console.WriteLine("[WeatherService] WARNING: response is array, taking first element");
                    root = root[0];
                }

                if (!root.TryGetProperty("current_weather", out var weather))
                {
                    Console.WriteLine("[WeatherService] No 'current_weather' in response");
                    continue;
                }


                _cached = new WeatherData
                {
                    City = _city,
                    Temperature = weather.GetProperty("temperature").GetDouble(),
                    WindSpeed = weather.GetProperty("windspeed").GetDouble(),
                    WeatherCode = weather.GetProperty("weathercode").GetInt32()
                };
                _lastUpdate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeatherService] Failed: {ex.Message}");
            }

            await Task.Delay(_updateInterval);
        }
    }
}
