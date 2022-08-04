using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharpDX;
using SharpDX.DirectInput;

namespace MaxRev.Input.Gamepad;

/// <summary>
/// Controls gamepad events. Don't forget to dispose
/// </summary>
public class GamepadController : IDisposable, IGamepadEvents
{
    private readonly DirectInput _directInput;
    private Joystick? _joystick;
    private readonly ILogger<GamepadController> _logger;
    private readonly Dictionary<Buttons, ButtonState> _buttons;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Buttons[] _buttonsList = Enum.GetValues(typeof(Buttons)).Cast<Buttons>().ToArray();
    private readonly Dictionary<AxisMap, bool> _oldAxis = new();

    private enum AxisMap
    {
        XL, XH,
        YL, YH
    }
    private record ButtonState
    {
        public bool IsPressed { get; set; }
    }

    public GamepadController(ILogger<GamepadController>? logger = null)
    {
        if (logger == null)
        {
            logger = NullLoggerFactory.Instance.CreateLogger<GamepadController>();
        }
        _directInput = new DirectInput();
        _logger = logger;
        _buttons = _buttonsList.ToDictionary(x => x, y => new ButtonState());
        _oldAxis = Enum.GetValues(typeof(AxisMap)).Cast<AxisMap>().ToDictionary(x => x, _ => false);
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Factory.StartNew(() => RunAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
    }
    /// <summary>
    /// EventHandler to allow the notification of Button changes.
    /// </summary>
    public event EventHandler<ButtonEventArgs>? ButtonChanged;

    /// <summary>
    /// EventHandler to allow the notification of Axis changes.
    /// </summary>
    public event EventHandler<AxisEventArgs>? AxisChanged;

    /// <summary>
    /// Gamepad was disconnected
    /// </summary>
    public event Action<IGamepadEvents>? OnDisconnected;

    /// <summary>
    /// Gamepad is connected and ready for input events
    /// </summary>
    public event Action<IGamepadEvents>? OnReady;

    /// <summary>
    /// Controller will await for gamepad to be reconnected
    /// </summary>
    public bool AwaitForReconnection { get; set; } = true;

    private void InitializeJoystick()
    {
        var devices = _directInput.GetDevices()
            .Where(x => x.Type == DeviceType.Joystick && string.Equals(x.InstanceName.Trim(), "usb gamepad"))
            .FirstOrDefault();
        if (devices is null)
        {
            _logger.LogInformation("Joistick is not connected");
            return;
        }

        _joystick = new Joystick(_directInput, devices.InstanceGuid);
        if (_joystick.Properties.VendorId != 2064)
        {
            _logger.LogError("This joystick is not a `USB Gamepad`");
        }
    }

    /// <summary>
    /// This method should run in detached thread. Normally you should not call it directly. Just run constructor of <see cref="GamepadController"/>.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var lcs = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        var token = lcs.Token;
        while (!token.IsCancellationRequested)
        {
            // this will run constantly so we can hot plug the device
            InitializeJoystick();
            if (!AwaitForReconnection)
            {
                _logger.LogCritical("Joystick is not connected. Auto await is off");
                break;
            }
            if (_joystick is null || _joystick.IsDisposed)
            {
                _logger.LogWarning("Joystick is not connected. Waiting for joystick device...");
                await Task.Delay(1000);
                continue;
            }
            _logger.LogInformation("Joystick is ready");
            OnReady?.Invoke(this);

            try
            {
                _joystick.Acquire();

                while (!token.IsCancellationRequested)
                {
                    CheckState(_joystick.GetCurrentState());
                    Thread.Sleep(5);
                }
            }
            catch (SharpDXException ex)
                when (ex.ResultCode == 0x8007001E || //not connected
                ex.ResultCode == 0x80040209) //unplugged
            {
                _logger.LogCritical("Joystick is disconnected");
                OnDisconnected?.Invoke(this);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Joystick error. {0}", ex.Message);
            }
            finally
            {
                _joystick.Unacquire();
                _joystick.Dispose();
            }
        }
    }

    private void CheckState(in JoystickState state)
    {
        // process buttons state
        foreach (var button in _buttonsList)
        {
            var current = state.Buttons[(int)button];
            var last = _buttons[button];
            if (current != last.IsPressed)
            {
                ButtonChanged?.Invoke(this, new ButtonEventArgs { Button = button, Pressed = current });
            }
            _buttons[button].IsPressed = current;
        }

        // process X values
        if (state.X == ushort.MaxValue)
        {
            if (!_oldAxis[AxisMap.XH])
            {
                AxisChanged?.Invoke(this, new AxisEventArgs { Axis = 'X', Value = short.MaxValue });
            }

            _oldAxis[AxisMap.XH] = true;
        }
        else if (state.X == 0)
        {
            if (!_oldAxis[AxisMap.XL])
            {
                AxisChanged?.Invoke(this, new AxisEventArgs { Axis = 'X', Value = short.MinValue });
            }
            _oldAxis[AxisMap.XL] = true;
        }
        else
        {
            if (_oldAxis[AxisMap.XL] || _oldAxis[AxisMap.XH])
            {
                AxisChanged?.Invoke(this, new AxisEventArgs { Axis = 'X', Value = 0 });
            }

            _oldAxis[AxisMap.XL] = _oldAxis[AxisMap.XH] = false;
        }

        // process Y values
        if (state.Y == ushort.MaxValue)
        {
            if (!_oldAxis[AxisMap.YH])
            {
                AxisChanged?.Invoke(this, new AxisEventArgs { Axis = 'Y', Value = short.MaxValue });
            }
            _oldAxis[AxisMap.YH] = true;
        }
        else if (state.Y == 0)
        {
            if (!_oldAxis[AxisMap.YL])
            {
                AxisChanged?.Invoke(this, new AxisEventArgs { Axis = 'Y', Value = short.MinValue });
            }
            _oldAxis[AxisMap.YL] = true;
        }
        else
        {
            if (_oldAxis[AxisMap.YL] || _oldAxis[AxisMap.YH])
            {
                AxisChanged?.Invoke(this, new AxisEventArgs { Axis = 'Y', Value = 0 });
            }
            _oldAxis[AxisMap.YL] = _oldAxis[AxisMap.YH] = false;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        ((IDisposable)_directInput).Dispose();
        _joystick?.Dispose();
    }
}
