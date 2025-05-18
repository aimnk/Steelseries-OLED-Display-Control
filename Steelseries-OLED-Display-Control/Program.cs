using System.Runtime.InteropServices;
using System.Text.Json;
using HidSharp;
using SteelseriesOledControl.Core;

class Program
{
    private static readonly string AssetsOledConfigJson =
        Path.Combine(AppContext.BaseDirectory, "Assets", "oled_config.json");
    
    static void Main(string[] args)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "Assets", "oled_config.json");
        var json = File.ReadAllText(configPath);
        var settings = JsonSerializer.Deserialize<DisplaySettings>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Console.WriteLine("[OLED] Запуск. Ожидание подключения устройства...");
        
        while (true)
        {
            try
            {
                var device = DeviceList.Local.GetHidDevices(0x1038, 0x12E0)
                    .Where(d => d.GetMaxFeatureReportLength() >= 512) // OLED обычно 1024+
                    .FirstOrDefault();
                
                if (device == null)
                {
                    Console.WriteLine("[OLED] Устройство не найдено. Повтор через 3 секунды...");
                    Thread.Sleep(3000);
                    continue;
                }

                var manager = new DeviceManager();
                if (!manager.Initialize(device))
                {
                    Console.WriteLine("[OLED] Не удалось инициализировать устройство.");
                    Thread.Sleep(3000);
                    continue;
                }

                Console.WriteLine("[OLED] Устройство подключено!");

                var controller = new DisplayController(manager.OledDevice, settings);

                foreach (var widgetConfig in settings.Widgets)
                {
                    WidgetRegistry.TryCreate(widgetConfig.Type, controller, settings);
                }

                controller.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLED] Ошибка или отключение устройства: {ex.Message}");
                // небольшая пауза перед повтором
                Thread.Sleep(1000);
            }

            Console.WriteLine("[OLED] Попытка переподключения устройства...");
        }
    }
}