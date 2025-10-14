using SuiteView.Models;

namespace SuiteView.Managers;

/// <summary>
/// Manages color themes for the application
/// Provides 5 professionally designed royal blue/walnut/brass color schemes
/// </summary>
public class ThemeManager
{
    private static readonly Dictionary<string, ColorTheme> _themes = new();

    /// <summary>
    /// Initialize all predefined themes
    /// </summary>
    static ThemeManager()
    {
        // Theme 1: Royal Classic - Royal blue with walnut and brass
        _themes["Royal Classic"] = new ColorTheme(
            name: "Royal Classic",
            primary: ColorTranslator.FromHtml("#2C5AA0"),      // Royal blue
            secondary: ColorTranslator.FromHtml("#3a71c5"),    // Rich walnut
            accent: ColorTranslator.FromHtml("#FFD700"),       // Lustrous brass/gold
            textOnPrimary: ColorTranslator.FromHtml("#FFFFFF"), // White text on blue
            textOnSecondary: ColorTranslator.FromHtml("#D4AF37"), // Brass text on walnut
            lightBlue: ColorTranslator.FromHtml("#60a0ff")     // Life blue for tree views (closer to primary)
        );

        // Theme 2: Navy Blue - Deep navy with walnut and bright brass
        _themes["Navy Blue"] = new ColorTheme(
            name: "Navy Blue",
            primary: ColorTranslator.FromHtml("#1E3A5F"),      // Deep navy blue
            secondary: ColorTranslator.FromHtml("#6B4423"),    // Rich walnut
            accent: ColorTranslator.FromHtml("#E8C547"),       // Bright brass
            textOnPrimary: ColorTranslator.FromHtml("#E8C547"), // Brass text on navy
            textOnSecondary: ColorTranslator.FromHtml("#E8C547"), // Brass text on walnut
            lightBlue: ColorTranslator.FromHtml("#2E5A7F")     // Darker blue for tree views (closer to primary)
        );

        // Theme 3: Sapphire - Bright sapphire blue with walnut and gold
        _themes["Sapphire"] = new ColorTheme(
            name: "Sapphire",
            primary: ColorTranslator.FromHtml("#0F52BA"),      // Sapphire blue
            secondary: ColorTranslator.FromHtml("#6B4423"),    // Rich walnut
            accent: ColorTranslator.FromHtml("#FFD700"),       // Pure gold
            textOnPrimary: ColorTranslator.FromHtml("#FFD700"), // Gold text on blue
            textOnSecondary: ColorTranslator.FromHtml("#FFD700"), // Gold text on walnut
            lightBlue: ColorTranslator.FromHtml("#2F6FCA")     // Darker blue for tree views (closer to primary)
        );

        // Theme 4: Midnight - Midnight blue with walnut and antique brass
        _themes["Midnight"] = new ColorTheme(
            name: "Midnight",
            primary: ColorTranslator.FromHtml("#003366"),      // Midnight blue
            secondary: ColorTranslator.FromHtml("#6B4423"),    // Rich walnut
            accent: ColorTranslator.FromHtml("#C9A961"),       // Antique brass
            textOnPrimary: ColorTranslator.FromHtml("#C9A961"), // Brass text on blue
            textOnSecondary: ColorTranslator.FromHtml("#C9A961"), // Brass text on walnut
            lightBlue: ColorTranslator.FromHtml("#1A4D7F")     // Darker blue for tree views (closer to primary)
        );

        // Theme 5: Azure - Azure blue with walnut and bronze
        _themes["Azure"] = new ColorTheme(
            name: "Azure",
            primary: ColorTranslator.FromHtml("#4169E1"),      // Azure royal blue
            secondary: ColorTranslator.FromHtml("#6B4423"),    // Rich walnut
            accent: ColorTranslator.FromHtml("#CD7F32"),       // Bronze
            textOnPrimary: ColorTranslator.FromHtml("#FFFFFF"), // White text on blue
            textOnSecondary: ColorTranslator.FromHtml("#CD7F32"), // Bronze text on walnut
            lightBlue: ColorTranslator.FromHtml("#5A86E8")     // Darker blue for tree views (closer to primary)
        );
    }

    /// <summary>
    /// Gets all available theme names
    /// </summary>
    public static IEnumerable<string> GetThemeNames()
    {
        return _themes.Keys;
    }

    /// <summary>
    /// Gets a theme by name, returns default Emerald theme if not found
    /// </summary>
    public static ColorTheme GetTheme(string themeName)
    {
        if (_themes.TryGetValue(themeName, out var theme))
        {
            return theme;
        }

        // Return default theme if not found
        return _themes["Royal Classic"];
    }

    /// <summary>
    /// Checks if a theme exists
    /// </summary>
    public static bool ThemeExists(string themeName)
    {
        return _themes.ContainsKey(themeName);
    }
}
