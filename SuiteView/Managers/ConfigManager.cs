using System.Text.Json;
using SuiteView.Models;

namespace SuiteView.Managers;

/// <summary>
/// Manages application configuration persistence to JSON file
/// Handles reading and writing settings to %AppData%\SuiteView\settings.json
/// </summary>
public class ConfigManager : IDisposable
{
    private readonly string _configDirectory;
    private readonly string _configFilePath;
    private AppSettings _settings;
    private bool _disposed;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gets the current application settings
    /// </summary>
    public AppSettings Settings => _settings;

    public ConfigManager()
    {
        // Set up config directory in AppData
        _configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SuiteView"
        );
        _configFilePath = Path.Combine(_configDirectory, "settings.json");

        // Load or create default settings
        _settings = LoadSettings();
    }

    /// <summary>
    /// Loads settings from JSON file or creates default settings if file doesn't exist
    /// </summary>
    private AppSettings LoadSettings()
    {
        try
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            // Load settings if file exists
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                if (settings != null)
                {
                    // Validate opacity range
                    if (settings.Opacity < 70 || settings.Opacity > 100)
                    {
                        settings.Opacity = 100;
                    }

                    return settings;
                }
            }

            // Return default settings if file doesn't exist or deserialization failed
            return new AppSettings();
        }
        catch (Exception ex)
        {
            // Log error and return default settings
            Console.WriteLine($"Error loading settings: {ex.Message}");
            return new AppSettings();
        }
    }

    /// <summary>
    /// Saves the current settings to JSON file
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            // Ensure directory exists
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            // Serialize and save settings
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error saving settings: {ex.Message}");
            MessageBox.Show(
                $"Failed to save settings: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    /// <summary>
    /// Updates settings and saves to file
    /// </summary>
    public void UpdateSettings(Action<AppSettings> updateAction)
    {
        updateAction(_settings);
        SaveSettings();
    }

    /// <summary>
    /// Gets the configuration file path
    /// </summary>
    public string GetConfigFilePath() => _configFilePath;

    /// <summary>
    /// Gets the configuration directory path
    /// </summary>
    public string GetConfigDirectory() => _configDirectory;

    public void Dispose()
    {
        if (!_disposed)
        {
            SaveSettings();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
