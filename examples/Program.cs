// See https://aka.ms/new-console-template for more information

using MaxRev.Input.Gamepad;

// Don't forget to dispose gamepad
using var gamepad = new GamepadController();
gamepad.AwaitForReconnection = true;
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
gamepad.OnDisconnected += (s) =>
{
    // Oh, creap! Gamepad was disconnected
};
gamepad.OnReady += (s) =>
{
    // Gamepad was reconnected and ready to receive input!
};



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

Console.ReadKey();