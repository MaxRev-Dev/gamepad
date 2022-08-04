namespace MaxRev.Input.Gamepad;

public interface IDebounceEventProvider
{
    /// <summary>
    /// Occurs when an axis long press is detected after a debounce interval
    /// </summary>
    event EventHandler<(AxisEventArgs Target, int Count)>? AxisLongPress;
    /// <summary> 
    /// Occurs when an button long press is detected after a debounce interval
    /// </summary>
    event EventHandler<(ButtonEventArgs Target, int Count)>? ButtonLongPress;
}
