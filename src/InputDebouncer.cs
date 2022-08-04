using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace MaxRev.Input.Gamepad;

/// <summary>
/// Debouncer for gamepad to trigger longpress events and ignore quick clicks on input
/// </summary>
public class GamepadInputDebouncer : IDisposable, IDebounceEventProvider
{
    private readonly DebounceTimers<char> _longPressAxis = new();
    private readonly DebounceTimers<char> _axisDebouncers = new();
    private readonly DebounceTimers<Buttons> _longPressButtons = new();
    private readonly DebounceTimers<Buttons> _buttonsDebouncers = new();
    public GamepadInputDebouncer(IGamepadEvents eventProvider)
    {
        eventProvider.ButtonChanged += ButtonLongPressHandler;
        eventProvider.AxisChanged += AxisLongPressHandler;
        Disposing += (_) =>
        {
            eventProvider.ButtonChanged -= ButtonLongPressHandler;
            eventProvider.AxisChanged -= AxisLongPressHandler;
        };
    }

    public event EventHandler<(AxisEventArgs Target, int Count)>? AxisLongPress;
    public event EventHandler<(ButtonEventArgs Target, int Count)>? ButtonLongPress;
    public event Action<GamepadInputDebouncer> Disposing;

    /// <summary>
    /// Default long press interval is 100ms
    /// </summary>
    public TimeSpan LongPressInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Default debounce interval is 250ms
    /// </summary>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromMilliseconds(250);

    private class DebounceTimers<T> : IDisposable where T : notnull
    {
        private readonly ConcurrentDictionary<T, Timer?> _timers;

        public DebounceTimers()
        {
            _timers = new ConcurrentDictionary<T, Timer?>();
        }

        public Timer ReplaceTimer(T key, Timer timer)
        {
            if (_timers.TryGetValue(key, out var old))
            {
                old?.Stop();
                old?.Dispose();
            }

            return _timers[key] = timer;
        }

        public Timer? GetTimer(T key)
        {
            _timers.TryGetValue(key, out var timer);
            return timer;
        }

        public void DisposeTimer(T key)
        {
            if (_timers.TryGetValue(key, out var old))
            {
                old?.Stop();
                old?.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var i in _timers.Values)
            {
                i?.Stop();
                i?.Dispose();
            }
        }
    }
    private void AxisLongPressHandler(object? sender, AxisEventArgs e)
    {
        _longPressAxis.DisposeTimer(e.Axis);
        _axisDebouncers.DisposeTimer(e.Axis);

        if (e.Value != 0)
        {
            var debounceTimer = _axisDebouncers.ReplaceTimer(e.Axis, new Timer
            {
                Interval = DebounceInterval.TotalMilliseconds,
                AutoReset = false
            });
            debounceTimer.Elapsed += (v, c) =>
            {
                var counterInterval = _longPressAxis.ReplaceTimer(e.Axis, new Timer
                {
                    Interval = LongPressInterval.TotalMilliseconds
                });
                var count = 0;
                counterInterval.Elapsed += (sc, ec) =>
                    AxisLongPress?.Invoke(this, (e, ++count));
                counterInterval.Start();
            };
            debounceTimer.Start();
        }
    }

    private void ButtonLongPressHandler(object? sender, ButtonEventArgs e)
    {
        _longPressButtons.DisposeTimer(e.Button);
        _buttonsDebouncers.DisposeTimer(e.Button);

        if (e.Pressed)
        {
            var debounceTimer = _buttonsDebouncers.ReplaceTimer(e.Button, new Timer
            {
                Interval = DebounceInterval.TotalMilliseconds,
                AutoReset = false
            });
            debounceTimer.Elapsed += (v, c) =>
            {
                var counterInterval = _longPressButtons.ReplaceTimer(e.Button, new Timer
                {
                    Interval = LongPressInterval.TotalMilliseconds
                });
                var count = 0;
                counterInterval.Elapsed += (sc, ec) =>
                    ButtonLongPress?.Invoke(this, (e, ++count));
                counterInterval.Start();
            };
            debounceTimer.Start();
        }
    }

    public void Dispose()
    {
        Disposing?.Invoke(this);
        _axisDebouncers.Dispose();
        _buttonsDebouncers.Dispose();
        _longPressAxis.Dispose();
        _longPressButtons.Dispose();
    }
}
