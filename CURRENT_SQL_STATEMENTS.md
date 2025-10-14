# Current SQL Statements Being Used

## When Adding a DB2 Table (GetTableFieldsAsync)

### Approach 1: Schema-based (current - failing for DB2)
```csharp
DataTable schemaTable = connection.GetSchema("Columns", new[] { null, null, tableName, null });
```
This uses ODBC's built-in schema retrieval, which generates SQL like:
```sql
SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LH_BAS_POL'
```

**Problem**: For DB2 tables with schema prefixes like `DB2TAB.LH_BAS_POL`, this query may not work correctly with the Windows 64-bit ODBC driver.

### Solution: Add WITH clause fallback for DB2

When schema approach fails, the code should try:
```sql
WITH DUMMY AS (SELECT 1 FROM DB2TAB.LH_COV_PHA) 
SELECT * FROM DB2TAB.LH_BAS_POL 
FETCH FIRST 1 ROW ONLY
```

Then use `reader.GetSchemaTable()` to extract column metadata.

##Human: Actually can you update the database service with a get_error flag that will print  the SQL being ran so I can verify its doing what we think it is