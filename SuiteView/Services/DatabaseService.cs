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
            
            // Check if this is DB2 (NEON_DSN) - if so, skip schema approach and go straight to WITH clause
            if (odbcDsn.Equals("NEON_DSN", StringComparison.OrdinalIgnoreCase))
            {
                // DB2: Go straight to WITH clause query
                string query = "WITH DUMBY AS (SELECT 1 FROM DB2TAB.LH_COV_PHA) SELECT NAME FROM SYSIBM.SYSTABLES WHERE CREATOR = 'DB2TAB'";
                
                using var command = new OdbcCommand(query, connection);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    string? tableName = reader["NAME"]?.ToString();
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tableName = tableName.Trim();
                        tables.Add($"DB2TAB.{tableName}");
                    }
                }
            }
            else
            {
                // SQL Server or other: Use schema approach
                try
                {
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
                }
                catch
                {
                    // Schema approach failed for SQL Server
                    throw;
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
            
            try
            {
                // Approach 1: Get column schema information using ODBC schema
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
                
                if (fields.Count == 0)
                {
                    throw new Exception("Schema approach returned no columns");
                }
            }
            catch (Exception)
            {
                // Approach 2: Try WITH clause for DB2 compatibility using DB2TAB.LH_COV_PHA (known working table)
                string withQuery = $"WITH DUMMY AS (SELECT 1 FROM DB2TAB.LH_COV_PHA) SELECT * FROM {tableName} FETCH FIRST 1 ROW ONLY";
                
                try
                {
                    using var command = new OdbcCommand(withQuery, connection);
                    using var reader = command.ExecuteReader(System.Data.CommandBehavior.SchemaOnly);
                    var querySchemaTable = reader.GetSchemaTable();
                    
                    if (querySchemaTable != null)
                    {
                        foreach (DataRow row in querySchemaTable.Rows)
                        {
                            var field = new FieldMetadata
                            {
                                FieldName = row["ColumnName"]?.ToString() ?? string.Empty,
                                DataType = row["DataTypeName"]?.ToString() ?? string.Empty,
                                MaxLength = row["ColumnSize"] != DBNull.Value ? Convert.ToInt32(row["ColumnSize"]) : null,
                                IsNullable = row["AllowDBNull"] != DBNull.Value && Convert.ToBoolean(row["AllowDBNull"])
                            };
                            
                            fields.Add(field);
                        }
                    }
                }
                catch
                {
                    // Approach 3: Try without WITH clause
                    string simpleQuery = $"SELECT * FROM {tableName} FETCH FIRST 1 ROW ONLY";
                    
                    try
                    {
                        using var command = new OdbcCommand(simpleQuery, connection);
                        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SchemaOnly);
                        var querySchemaTable = reader.GetSchemaTable();
                        
                        if (querySchemaTable != null)
                        {
                            foreach (DataRow row in querySchemaTable.Rows)
                            {
                                var field = new FieldMetadata
                                {
                                    FieldName = row["ColumnName"]?.ToString() ?? string.Empty,
                                    DataType = row["DataTypeName"]?.ToString() ?? string.Empty,
                                    MaxLength = row["ColumnSize"] != DBNull.Value ? Convert.ToInt32(row["ColumnSize"]) : null,
                                    IsNullable = row["AllowDBNull"] != DBNull.Value && Convert.ToBoolean(row["AllowDBNull"])
                                };
                                
                                fields.Add(field);
                            }
                        }
                    }
                    catch (Exception simpleEx)
                    {
                        // All approaches failed - throw detailed error
                        throw new Exception(
                            $"Failed to retrieve fields for table: {tableName}\n" +
                            $"ODBC DSN: {odbcDsn}\n" +
                            $"Database: {databaseName}\n" +
                            $"Final error: {simpleEx.Message}",
                            simpleEx);
                    }
                }
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

    /// <summary>
    /// Executes a SQL query and returns the results as a DataTable
    /// </summary>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="odbcDsn">ODBC datasource name</param>
    /// <param name="databaseName">Database name</param>
    /// <param name="debugLog">If true, writes SQL and connection details to Debug output</param>
    public static async Task<DataTable> ExecuteQueryAsync(string sql, string odbcDsn, string databaseName, bool debugLog = false)
    {
        if (debugLog)
        {
            System.Diagnostics.Debug.WriteLine("=== ExecuteQueryAsync Debug ===");
            System.Diagnostics.Debug.WriteLine($"ODBC DSN: {odbcDsn}");
            System.Diagnostics.Debug.WriteLine($"Database: {databaseName}");
            System.Diagnostics.Debug.WriteLine($"Connection String: DSN={odbcDsn};Database={databaseName};");
            System.Diagnostics.Debug.WriteLine($"SQL Query:");
            System.Diagnostics.Debug.WriteLine(sql);
            System.Diagnostics.Debug.WriteLine("================================");
        }
        
        var dataTable = new DataTable();
        
        await Task.Run(() =>
        {
            string connectionString = $"DSN={odbcDsn};Database={databaseName};";
            
            using var connection = new OdbcConnection(connectionString);
            connection.Open();
            
            using var command = new OdbcCommand(sql, connection);
            command.CommandTimeout = 120; // 2 minutes timeout for large queries
            
            using var adapter = new OdbcDataAdapter(command);
            adapter.Fill(dataTable);
        });
        
        if (debugLog)
        {
            System.Diagnostics.Debug.WriteLine($"=== Query Result: {dataTable.Rows.Count} rows returned ===");
        }
        
        return dataTable;
    }
}
