﻿namespace MaxRev.Input.Gamepad;

/// <summary>
/// Represents a button event
/// </summary>
public class ButtonEventArgs
{
    public Buttons Button { get; set; }
    public bool Pressed { get; set; }
}
