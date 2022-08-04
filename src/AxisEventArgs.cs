namespace MaxRev.Input.Gamepad;

/// <summary>
/// Represents an axis event
/// </summary>
public class AxisEventArgs
{
    public char Axis { get; set; }
    public short Value { get; set; }
}
