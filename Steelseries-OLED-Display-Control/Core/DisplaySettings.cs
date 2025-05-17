using System.Numerics;
using System.Text.Json.Serialization;

namespace SteelseriesOledControl.Core;

public class DisplaySettings
{
    public string BrightnessLevel { get; set; } = "Min";
    public int FrameDelayMs { get; set; } = 1000;
    public int AfkTimeoutMinutes { get; set; } = 5;
    public int WakeupThresholdMinutes { get; set; } = 1;
    public List<WidgetConfig> Widgets { get; set; } = new();
}

[Serializable]
public class WidgetConfig
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("params")]
    public Dictionary<string, string> Params { get; set; } = new();
}

[Serializable]
public class LatLon
{
    public float Lat { get; set; }
    public float Lon { get; set; }

    public Vector2 ToVector2() => new(Lat, Lon);
}
