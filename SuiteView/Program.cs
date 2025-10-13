using SuiteView.Forms;
using SuiteView.Managers;

namespace SuiteView;

/// <summary>
/// SuiteView - Dockable Toolbar Application
/// Main application entry point
/// </summary>
static class Program
{
    private static ConfigManager? _configManager;
    private static SystemTrayManager? _systemTrayManager;
    private static ResizableBorderForm? _borderForm;
    private static MainForm? _mainForm;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // Enable visual styles for modern Windows look
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize configuration manager
            _configManager = new ConfigManager();

            // Initialize system tray manager
            _systemTrayManager = new SystemTrayManager();
            _systemTrayManager.ShowHideClicked += SystemTray_ShowHideClicked;
            _systemTrayManager.SettingsClicked += SystemTray_SettingsClicked;
            _systemTrayManager.ExitClicked += SystemTray_ExitClicked;

            // Create border form (parent) and main form (child content)
            _borderForm = new ResizableBorderForm(_configManager);
            _mainForm = new MainForm(_configManager);

            // Set up the parent-child relationship
            _mainForm.SetParentBorderForm(_borderForm);
            _borderForm.SetContentForm(_mainForm);

            // Set initial visibility based on settings (default: hidden/minimized to tray)
            if (_configManager.Settings.IsVisible)
            {
                _borderForm.Show();
                _systemTrayManager.IsToolbarVisible = true;
            }
            else
            {
                _borderForm.Hide();
                _systemTrayManager.IsToolbarVisible = false;
            }

            _systemTrayManager.UpdateContextMenuText();

            // Show startup notification
            _systemTrayManager.ShowBalloonTip(
                "SuiteView Started",
                "SuiteView is running in the system tray. Right-click the icon to show/hide the toolbar.",
                ToolTipIcon.Info
            );

            // Run the application with an application context to keep it alive
            Application.Run(new ApplicationContext());

            // Cleanup
            _configManager?.Dispose();
            _systemTrayManager?.Dispose();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fatal error starting SuiteView:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "SuiteView Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private static void SystemTray_ShowHideClicked(object? sender, EventArgs e)
    {
        if (_borderForm == null || _systemTrayManager == null || _configManager == null)
            return;

        if (_borderForm.Visible)
        {
            _borderForm.Hide();
            _systemTrayManager.IsToolbarVisible = false;
            _configManager.UpdateSettings(settings => settings.IsVisible = false);
        }
        else
        {
            _borderForm.Show();
            _borderForm.Activate();
            _systemTrayManager.IsToolbarVisible = true;
            _configManager.UpdateSettings(settings => settings.IsVisible = true);
        }

        _systemTrayManager.UpdateContextMenuText();
    }

    private static void SystemTray_SettingsClicked(object? sender, EventArgs e)
    {
        if (_mainForm == null || _borderForm == null || _configManager == null)
            return;

        // Show settings form
        using var settingsForm = new SettingsForm(_configManager, _mainForm, _borderForm);
        settingsForm.ShowDialog();
    }

    private static void SystemTray_ExitClicked(object? sender, EventArgs e)
    {
        // Perform cleanup and exit
        _configManager?.Dispose();
        _systemTrayManager?.Dispose();
        _borderForm?.Close();

        Application.Exit();
    }
}
