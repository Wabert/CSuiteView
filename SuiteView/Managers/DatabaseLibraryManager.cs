using System.Text.Json;
using SuiteView.Models;

namespace SuiteView.Managers;

/// <summary>
/// Manages database library configuration persistence
/// </summary>
public class DatabaseLibraryManager
{
    private static readonly string ConfigFileName = "database_library.json";
    private static string ConfigFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SuiteView",
        ConfigFileName
    );
    
    /// <summary>
    /// Loads the database library configuration from disk
    /// </summary>
    public static DatabaseLibraryConfig LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                string json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<DatabaseLibraryConfig>(json);
                return config ?? new DatabaseLibraryConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading database library config: {ex.Message}");
        }
        
        return new DatabaseLibraryConfig();
    }
    
    /// <summary>
    /// Saves the database library configuration to disk
    /// </summary>
    public static void SaveConfig(DatabaseLibraryConfig config)
    {
        try
        {
            // Ensure directory exists
            string? directory = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving database library config: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets or creates a database configuration by name
    /// </summary>
    public static DatabaseConfig GetOrCreateDatabase(DatabaseLibraryConfig config, string name, string odbcDsn, string databaseName)
    {
        var db = config.Databases.FirstOrDefault(d => d.Name == name);
        if (db == null)
        {
            db = new DatabaseConfig
            {
                Name = name,
                OdbcDsn = odbcDsn,
                DatabaseName = databaseName
            };
            config.Databases.Add(db);
        }
        
        return db;
    }
}
