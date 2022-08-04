namespace MaxRev.Input.Windows;

public interface IGamepadEvents
{
    /// <summary>
    /// Occurs when the state of any button changes
    /// </summary>
    event EventHandler<ButtonEventArgs>? ButtonChanged;
    /// <summary>
    /// Occurs when the state of any axis changes
    /// </summary>
    event EventHandler<AxisEventArgs>? AxisChanged;
}
