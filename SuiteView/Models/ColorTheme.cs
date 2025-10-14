namespace SuiteView.Models;

/// <summary>
/// Represents a color theme with 2-3 coordinated colors
/// </summary>
public class ColorTheme
{
    /// <summary>
    /// Theme name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Primary color (vibrant green)
    /// </summary>
    public Color Primary { get; set; }

    /// <summary>
    /// Secondary color (dark slate/charcoal)
    /// </summary>
    public Color Secondary { get; set; }

    /// <summary>
    /// Accent color (white or light cyan for text/borders)
    /// </summary>
    public Color Accent { get; set; }

    /// <summary>
    /// Text color on primary background
    /// </summary>
    public Color TextOnPrimary { get; set; }

    /// <summary>
    /// Text color on secondary background
    /// </summary>
    public Color TextOnSecondary { get; set; }

    /// <summary>
    /// Light blue color for tree view backgrounds
    /// </summary>
    public Color LightBlue { get; set; }

    public ColorTheme()
    {
    }

    public ColorTheme(string name, Color primary, Color secondary, Color accent, Color textOnPrimary, Color textOnSecondary, Color lightBlue)
    {
        Name = name;
        Primary = primary;
        Secondary = secondary;
        Accent = accent;
        TextOnPrimary = textOnPrimary;
        TextOnSecondary = textOnSecondary;
        LightBlue = lightBlue;
    }
}
