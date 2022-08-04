<img align="left" height="85" src="media/icon.png" alt="Gamepad">

# SNES Gamepad Controller
A simple gamepad controller with debouncer using SharpDX (DirectX).

### Usage:
```C#
// See https://aka.ms/new-console-template for more information

using MaxRev.Input.Gamepad;

// Don't forget to dispose gamepad
using var gamepad = new GamepadController();
gamepad.AxisChanged += (s, e) =>
{
    Console.WriteLine($"Axis {e.Axis} - {e.Value}");
    // your own logic
};
gamepad.ButtonChanged += (s, e) =>
{
    Console.WriteLine($"Button {e.Button} - {e.Pressed}");
    // your own logic
};
```
### You can also configure and attach a debouncer to handle longpress: events
```C#
// Debouncer can be optinally subscribed for input events
using var inputDebouncer = new GamepadInputDebouncer(gamepad);
inputDebouncer.DebounceInterval = TimeSpan.FromMilliseconds(100);
inputDebouncer.LongPressInterval = TimeSpan.FromMilliseconds(250);
inputDebouncer.AxisLongPress += (s, e) =>
{
    Console.WriteLine($"Axis LongPress {e.Target.Axis} - x{e.Count}");
    // your own logic
};
inputDebouncer.ButtonLongPress += (s, e) =>
{
    Console.WriteLine($"Button LongPress {e.Target.Button} - x{e.Count}");
    // your own logic
};

```

This project is simmilar to https://github.com/nahueltaibo/gamepad, but works in Windows using DirectX API.

## Donations
Made with ❤️ in &#127482;&#127462;

Our country needs any support, so you can donate to Armed Forces of Ukraine or humanitarian needs.

See more at https://standforukraine.com/
