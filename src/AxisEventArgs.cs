namespace MaxRev.Input.Windows;

/// <summary>
/// Represents an axis event
/// </summary>
public class AxisEventArgs
{
    public char Axis { get; set; }
    public short Value { get; set; }
}
