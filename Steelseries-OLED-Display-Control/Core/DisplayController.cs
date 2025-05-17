
using SteelseriesOledControl.Helpers;

namespace SteelseriesOledControl.Core
{
    public class DisplayController(DeviceWrapper device, DisplaySettings settings)
    {
        private readonly Utils.BrightnessLevel _brightness = Enum.Parse<Utils.BrightnessLevel>(settings.BrightnessLevel);
        private readonly int _frameDelayMs = settings.FrameDelayMs;
        private readonly int _afkTimeout = settings.AfkTimeoutMinutes;
        private readonly int _wakeupThreshold = settings.WakeupThresholdMinutes;
        private CancellationTokenSource _cancelTokenSource = new();
        
        private readonly List<DisplayContent> _contents = new();
        private int _currentIndex = 0;
        private DateTime _lastActivity = DateTime.UtcNow;
        private bool _displayIsDimmed = false;
        private readonly object _displayLock = new();
        
        public void AddContent(DisplayContent content)
        {
            _contents.Add(content);
        }

        /// <summary>
        /// возвращает управление когда показывается дефолтный дисплей
        /// </summary>
        private void StartRecoveryLoop(CancellationToken ct)
        {
            new Thread(() =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        Thread.Sleep(500);
                        if (_contents.Count == 0)
                            continue;

                        var content = _contents[_currentIndex];
                        var buffer = new byte[128 * 64];

                        content.Render(buffer, 128, 64);
                        lock (_displayLock)
                        {
                            if (_displayIsDimmed) return;

                            device.SendFrame(buffer);
                        }
                    }
                }
                
                catch (Exception e)
                {
                    Console.WriteLine($"[RecoveryLoop] Прервано: {e.Message}");
                    _cancelTokenSource.Cancel();
                }
            }).Start();
        }
        
        public void Run()
        {
            _cancelTokenSource = new CancellationTokenSource();
            StartRecoveryLoop(_cancelTokenSource.Token);

            try
            {
                while (!_cancelTokenSource.IsCancellationRequested)
                {
                    if (!_displayIsDimmed && AfkDetector.GetIdleTime().TotalMinutes > _afkTimeout)
                    {
                        _displayIsDimmed = true;
                        lock (_displayLock)
                        {
                            var blank = new byte[128 * 64];
                            device.SendFrame(blank);
                        }

                    }
                    else if (_displayIsDimmed && AfkDetector.GetIdleTime().TotalMinutes < _wakeupThreshold)
                    {
                        device.SetBrightness(_brightness);
                        _displayIsDimmed = false;
                    }

                    if (_displayIsDimmed)
                    {
                        Thread.Sleep(_frameDelayMs);
                        continue;
                    }

                    var content = _contents[_currentIndex];

                    if (content is IDynamicContent dynamic && !dynamic.NeedsUpdate())
                    {
                        _currentIndex = (_currentIndex + 1) % _contents.Count;
                        Thread.Sleep(_frameDelayMs);
                        continue;
                    }

                    byte[] buffer = new byte[128 * 64];
                    content.Render(buffer, 128, 64);

                    lock (_displayLock)
                    {
                        if (!_displayIsDimmed)
                        {
                            device.SendFrame(buffer);
                        }
                    }

                    _currentIndex = (_currentIndex + 1) % _contents.Count;
                    Thread.Sleep(_frameDelayMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Run] Прервано: {ex.Message}");
            }
            
            _cancelTokenSource.Cancel();
        }
    }
} 
