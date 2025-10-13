namespace SuiteView.Models;

/// <summary>
/// Represents a file system item (file, folder, or link) for efficient display
/// </summary>
public class FileSystemItem
{
    public string FullPath { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime DateModified { get; set; }
    public string Size { get; set; } = string.Empty;
}
