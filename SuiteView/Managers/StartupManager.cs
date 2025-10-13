using Microsoft.Win32;

namespace SuiteView.Managers;

/// <summary>
/// Manages Windows startup registry entry for launching the application on system startup
/// </summary>
public static class StartupManager
{
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SuiteView";

    /// <summary>
    /// Adds the application to Windows startup
    /// </summary>
    public static bool AddToStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);

            if (key == null)
            {
                return false;
            }

            var exePath = Application.ExecutablePath;
            key.SetValue(AppName, $"\"{exePath}\"");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding to startup: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Removes the application from Windows startup
    /// </summary>
    public static bool RemoveFromStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);

            if (key == null)
            {
                return false;
            }

            // Check if the value exists before trying to delete
            if (key.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing from startup: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if the application is set to run on startup
    /// </summary>
    public static bool IsInStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);

            if (key == null)
            {
                return false;
            }

            var value = key.GetValue(AppName) as string;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            // Check if the path matches the current executable
            var exePath = Application.ExecutablePath;
            return value.Contains(exePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking startup status: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Updates the startup status based on the desired state
    /// </summary>
    public static bool UpdateStartupStatus(bool shouldStartup)
    {
        if (shouldStartup)
        {
            return AddToStartup();
        }
        else
        {
            return RemoveFromStartup();
        }
    }
}
