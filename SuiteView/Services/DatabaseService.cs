using System.Data;
using System.Data.Odbc;
using SuiteView.Models;

namespace SuiteView.Services;

/// <summary>
/// Service for interacting with databases via ODBC
/// </summary>
public class DatabaseService
{
    /// <summary>
    /// Gets all table names from a database using ODBC DSN
    /// </summary>
    public static async Task<List<string>> GetTableNamesAsync(string odbcDsn, string databaseName)
    {
        var tables = new List<string>();
        
        await Task.Run(() =>
        {
            string connectionString = $"DSN={odbcDsn};Database={databaseName};";
            
            using var connection = new OdbcConnection(connectionString);
            connection.Open();
            
            // Get schema information for tables
            DataTable schemaTable = connection.GetSchema("Tables");
            
            foreach (DataRow row in schemaTable.Rows)
            {
                string? tableType = row["TABLE_TYPE"]?.ToString();
                if (tableType == "TABLE" || tableType == "BASE TABLE")
                {
                    string? tableName = row["TABLE_NAME"]?.ToString();
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
            }
        });
        
        return tables;
    }
    
    /// <summary>
    /// Gets field metadata for a specific table
    /// </summary>
    public static async Task<List<FieldMetadata>> GetTableFieldsAsync(string odbcDsn, string databaseName, string tableName)
    {
        var fields = new List<FieldMetadata>();
        
        await Task.Run(() =>
        {
            string connectionString = $"DSN={odbcDsn};Database={databaseName};";
            
            using var connection = new OdbcConnection(connectionString);
            connection.Open();
            
            // Get column schema information
            DataTable schemaTable = connection.GetSchema("Columns", new[] { null, null, tableName, null });
            
            foreach (DataRow row in schemaTable.Rows)
            {
                var field = new FieldMetadata
                {
                    FieldName = row["COLUMN_NAME"]?.ToString() ?? string.Empty,
                    DataType = row["DATA_TYPE"]?.ToString() ?? string.Empty,
                    MaxLength = row["COLUMN_SIZE"] != DBNull.Value ? Convert.ToInt32(row["COLUMN_SIZE"]) : null,
                    IsNullable = row["IS_NULLABLE"]?.ToString()?.Equals("YES", StringComparison.OrdinalIgnoreCase) ?? false
                };
                
                fields.Add(field);
            }
        });
        
        return fields;
    }
    
    /// <summary>
    /// Tests if an ODBC connection is valid
    /// </summary>
    public static async Task<bool> TestConnectionAsync(string odbcDsn, string databaseName)
    {
        try
        {
            await Task.Run(() =>
            {
                string connectionString = $"DSN={odbcDsn};Database={databaseName};";
                using var connection = new OdbcConnection(connectionString);
                connection.Open();
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets unique values for a specific field in a table
    /// Returns a tuple of (uniqueValues, hasMoreThan200, totalCount)
    /// </summary>
    public static async Task<(List<string> uniqueValues, bool hasMoreThan200, int totalCount)> GetUniqueValuesAsync(
        string odbcDsn, 
        string databaseName, 
        string tableName, 
        string fieldName)
    {
        var uniqueValues = new List<string>();
        bool hasMoreThan200 = false;
        int totalCount = 0;
        
        await Task.Run(() =>
        {
            string connectionString = $"DSN={odbcDsn};Database={databaseName};";
            
            using var connection = new OdbcConnection(connectionString);
            connection.Open();
            
            // First, count the unique values
            string countQuery = $"SELECT COUNT(DISTINCT [{fieldName}]) FROM [{tableName}]";
            using var countCommand = new OdbcCommand(countQuery, connection);
            totalCount = Convert.ToInt32(countCommand.ExecuteScalar());
            
            if (totalCount > 200)
            {
                hasMoreThan200 = true;
            }
            else
            {
                // Get the actual unique values
                string query = $"SELECT DISTINCT [{fieldName}] FROM [{tableName}] ORDER BY [{fieldName}]";
                using var command = new OdbcCommand(query, connection);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    var value = reader[0];
                    if (value == DBNull.Value || value == null)
                    {
                        uniqueValues.Add("(NULL)");
                    }
                    else
                    {
                        uniqueValues.Add(value.ToString() ?? "(NULL)");
                    }
                }
            }
        });
        
        return (uniqueValues, hasMoreThan200, totalCount);
    }
}
