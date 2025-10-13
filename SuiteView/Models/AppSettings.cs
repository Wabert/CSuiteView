using System.Text.Json.Serialization;

namespace SuiteView.Models;

/// <summary>
/// Represents the application settings that are persisted to JSON file
/// </summary>
public class AppSettings
{
    /// <summary>
    /// The window position on screen
    /// </summary>
    [JsonPropertyName("windowPosition")]
    public WindowPosition WindowPosition { get; set; } = new();

    /// <summary>
    /// The window size
    /// </summary>
    [JsonPropertyName("windowSize")]
    public WindowSize WindowSize { get; set; } = new() { Width = 80, Height = 300 };

    /// <summary>
    /// The dock position of the toolbar
    /// </summary>
    [JsonPropertyName("dockPosition")]
    public DockPosition DockPosition { get; set; } = DockPosition.BottomRight;

    /// <summary>
    /// The index of the monitor where the toolbar is displayed
    /// </summary>
    [JsonPropertyName("monitorIndex")]
    public int MonitorIndex { get; set; } = 0;

    /// <summary>
    /// The selected color theme name
    /// </summary>
    [JsonPropertyName("selectedTheme")]
    public string SelectedTheme { get; set; } = "Emerald";

    /// <summary>
    /// Whether the application should launch on Windows startup
    /// </summary>
    [JsonPropertyName("launchOnStartup")]
    public bool LaunchOnStartup { get; set; } = false;

    /// <summary>
    /// The window opacity percentage (70-100)
    /// </summary>
    [JsonPropertyName("opacity")]
    public int Opacity { get; set; } = 100;

    /// <summary>
    /// Whether the window should always be on top
    /// </summary>
    [JsonPropertyName("alwaysOnTop")]
    public bool AlwaysOnTop { get; set; } = true;

    /// <summary>
    /// Whether the toolbar is visible
    /// </summary>
    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; } = false;
}

/// <summary>
/// Represents a window position
/// </summary>
public class WindowPosition
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

/// <summary>
/// Represents a window size
/// </summary>
public class WindowSize
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

/// <summary>
/// Represents the dock position of the toolbar
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DockPosition
{
    None,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
