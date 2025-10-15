using System;
using System.Drawing;
using SuiteView.Models;
using SuiteView.Managers;

namespace SuiteView.Forms.LayeredForms;

/// <summary>
/// Factory for creating layered forms wrapped in BorderedWindowForm
/// Provides consistent creation patterns for all layered form types
/// </summary>
public static class LayeredFormFactory
{
    /// <summary>
    /// Create the main SuiteView form
    /// </summary>
    public static BorderedWindowForm CreateMainForm(ConfigManager configManager)
    {
        var theme = Managers.ThemeManager.GetTheme(configManager.Settings.SelectedTheme);
        
        var contentForm = new MainFormNew(configManager);
        
        var borderedWindow = new BorderedWindowForm(
            theme: theme,
            initialSize: new Size(800, 600),
            minimumSize: new Size(100, 150));
        
        borderedWindow.SetContentForm(contentForm);
        
        // Pass the border form reference to the content form
        contentForm.SetParentBorderForm(borderedWindow);
        
        return borderedWindow;
    }

    /// <summary>
    /// Create a two-layer form with brass border
    /// </summary>
    public static BorderedWindowForm CreateTwoLayerForm(
        int formWidth = 1000,
        int formHeight = 600,
        int layer1HeaderHeight = 30,
        int layer1FooterHeight = 30,
        Size? minimumSize = null,
        ColorTheme? theme = null)
    {
        theme ??= Managers.ThemeManager.GetTheme("Royal Classic");
        
        var contentForm = new TwoLayerForm(
            formWidth: formWidth,
            formHeight: formHeight,
            layer1HeaderHeight: layer1HeaderHeight,
            layer1FooterHeight: layer1FooterHeight,
            theme: theme);
        
        var borderedWindow = new BorderedWindowForm(
            theme: theme,
            initialSize: new Size(formWidth, formHeight),
            minimumSize: minimumSize ?? new Size(100, 100));
        
        borderedWindow.SetContentForm(contentForm);
        return borderedWindow;
    }
    
    /// <summary>
    /// Create a three-layer form with resizable panels and brass border
    /// </summary>
    public static BorderedWindowForm CreateThreeLayerForm(
        int formWidth = 1000,
        int formHeight = 600,
        int layer1HeaderHeight = 30,
        int layer1FooterHeight = 30,
        int layer2HeaderHeight = 0,
        int layer2FooterHeight = 0,
        int layer2BorderWidth = 15,
        int panelCount = 3,
        Size? minimumSize = null,
        ColorTheme? theme = null)
    {
        theme ??= Managers.ThemeManager.GetTheme("Royal Classic");
        
        var contentForm = new ThreeLayerForm(
            formWidth: formWidth,
            formHeight: formHeight,
            layer1HeaderHeight: layer1HeaderHeight,
            layer1FooterHeight: layer1FooterHeight,
            layer2HeaderHeight: layer2HeaderHeight,
            layer2FooterHeight: layer2FooterHeight,
            layer2BorderWidth: layer2BorderWidth,
            panelCount: panelCount,
            theme: theme);
        
        var borderedWindow = new BorderedWindowForm(
            theme: theme,
            initialSize: new Size(formWidth, formHeight),
            minimumSize: minimumSize ?? new Size(300, 200));
        
        borderedWindow.SetContentForm(contentForm);
        return borderedWindow;
    }
}
