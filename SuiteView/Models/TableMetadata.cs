namespace SuiteView.Models;

/// <summary>
/// Represents metadata for a database table
/// </summary>
public class TableMetadata
{
    public string TableName { get; set; } = string.Empty;
    public DateTime? LastScanned { get; set; }
    public List<FieldMetadata> Fields { get; set; } = new();
    public bool IsInLibrary { get; set; }
}

/// <summary>
/// Represents metadata for a table field/column
/// </summary>
public class FieldMetadata
{
    public string FieldName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    
    // Unique values analysis
    public DateTime? UniqueValuesScannedDate { get; set; }
    public List<string>? UniqueValues { get; set; }
    public bool HasMoreThan200UniqueValues { get; set; }
    public int? UniqueValuesCount { get; set; }
}

/// <summary>
/// Represents a database connection configuration
/// </summary>
public class DatabaseConfig
{
    public string Name { get; set; } = string.Empty;
    public string OdbcDsn { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public List<TableMetadata> Tables { get; set; } = new();
}

/// <summary>
/// Root configuration for the Database Library Manager
/// </summary>
public class DatabaseLibraryConfig
{
    public List<DatabaseConfig> Databases { get; set; } = new();
}
