# Unique Values Analysis Feature

## Overview
Enhanced the Database Library Manager with the ability to analyze and store unique values for each field in your library tables.

## New Functionality

### Field-Level View
When you click on a table in "My_Library", the right panel now switches to show a detailed field view:

**Columns:**
1. **Field Name** (with checkbox) - Select fields to analyze
2. **Data Type** - Shows the SQL data type (e.g., varchar(50), int)
3. **Unique Values Scanned** - Date/time when unique values were last analyzed (or "Never")
4. **Unique Values** - Shows the actual unique values or a message

### Get Unique Values Button
- Appears at the top of the right panel when viewing a table's fields
- Click to analyze selected (checked) fields
- Queries the database to find all unique values in each field

### Smart Storage Logic
**For fields with ≤ 200 unique values:**
- All unique values are stored in the JSON configuration
- Displayed in the "Unique Values" column (comma-separated)
- Available for quick reference without re-querying

**For fields with > 200 unique values:**
- Values are NOT stored (to keep JSON file manageable)
- Column shows: "More than 200 unique values"
- Scan date is still recorded

### Data Persistence
All unique values data is automatically saved to:
```
%AppData%\Roaming\SuiteView\database_library.json
```

When you click on a table again, any previously scanned field data is automatically loaded and displayed.

## Updated Data Model

### FieldMetadata Class
Added three new properties to track unique values:

```csharp
public class FieldMetadata
{
    public string FieldName { get; set; }
    public string DataType { get; set; }
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    
    // New properties for unique values
    public DateTime? UniqueValuesScannedDate { get; set; }
    public List<string>? UniqueValues { get; set; }
    public bool HasMoreThan200UniqueValues { get; set; }
}
```

## New Service Methods

### DatabaseService.GetUniqueValuesAsync()
```csharp
public static async Task<(List<string> uniqueValues, bool hasMoreThan200)> GetUniqueValuesAsync(
    string odbcDsn, 
    string databaseName, 
    string tableName, 
    string fieldName)
```

**How it works:**
1. First runs `COUNT(DISTINCT [fieldName])` to get the count
2. If count > 200, returns `(empty list, hasMoreThan200 = true)`
3. If count ≤ 200, runs `SELECT DISTINCT [fieldName]` and returns all values
4. NULL values are represented as "(NULL)" in the list

## User Workflow

### Analyzing Field Values
1. **Navigate to table**: Click on a table under "My_Library" in the tree view
2. **View fields**: Right panel switches to show all fields in a table
3. **Select fields**: Check the boxes next to fields you want to analyze
4. **Analyze**: Click "Get Unique Values" button at the top
5. **View results**: 
   - "Unique Values Scanned" column updates with timestamp
   - "Unique Values" column shows either:
     - Comma-separated list of values (if ≤ 200)
     - "More than 200 unique values" (if > 200)
6. **Persist**: All data automatically saves to JSON

### Re-viewing Previously Scanned Fields
1. Click on any table you've previously analyzed
2. Fields ListView automatically populates with saved data
3. No database query needed - loads instantly from JSON

## UI Changes

### Layout Modes
The right panel now has two modes:

**Tables Mode** (default when clicking LifeProd_Library):
- Shows ListView with tables
- Columns: Table Name, Last Scanned, Fields
- Buttons: "Scan Selected", "Move to Library"

**Fields Mode** (when clicking a table in My_Library):
- Shows ListView with fields
- Columns: Field Name, Data Type, Unique Values Scanned, Unique Values
- Button: "Get Unique Values" (at top of panel)

### Visual Hierarchy
```
┌─────────────────────────────────────────────────┐
│ Database Library Manager                     [X]│
├──────────────┬──────────────────────────────────┤
│ TreeView     │ [Get Unique Values] Button       │
│              ├──────────────────────────────────┤
│LifeProd_Lib. │ Fields ListView                  │
│└─My_Library  │ ☑ CustomerId    int     Never    │
│  ├─Customers │ ☐ FirstName  varchar(50) Never   │
│  └─Orders    │ ☑ Status     varchar(20) Never   │
│              │                                   │
├──────────────┴──────────────────────────────────┤
│                              [Scan] [Move to Lib]│
└─────────────────────────────────────────────────┘
```

## JSON Structure Example

```json
{
  "Databases": [
    {
      "Name": "LifeProd_Library",
      "OdbcDsn": "UL_Rates",
      "DatabaseName": "UL_Rates",
      "Tables": [
        {
          "TableName": "Customers",
          "LastScanned": "2025-10-13T14:30:00",
          "IsInLibrary": true,
          "Fields": [
            {
              "FieldName": "Status",
              "DataType": "varchar",
              "MaxLength": 20,
              "IsNullable": false,
              "UniqueValuesScannedDate": "2025-10-13T15:45:00",
              "UniqueValues": ["Active", "Inactive", "Pending"],
              "HasMoreThan200UniqueValues": false
            },
            {
              "FieldName": "CustomerId",
              "DataType": "int",
              "MaxLength": null,
              "IsNullable": false,
              "UniqueValuesScannedDate": "2025-10-13T15:45:00",
              "UniqueValues": null,
              "HasMoreThan200UniqueValues": true
            }
          ]
        }
      ]
    }
  ]
}
```

## Performance Considerations

### Database Queries
- **Count query** runs first: `SELECT COUNT(DISTINCT [field])`
  - Fast even on large tables (SQL Server optimized)
- **Values query** only runs if count ≤ 200: `SELECT DISTINCT [field] ORDER BY [field]`
  - Limited result set ensures good performance

### Memory Usage
- Only stores up to 200 values per field
- Prevents JSON file from becoming too large
- Typical storage: ~50 bytes per value = ~10KB per field maximum

### UI Responsiveness
- All database operations are async
- Cursor changes to wait cursor during queries
- Button disabled during processing to prevent multiple clicks

## Code Files Modified

### Models/TableMetadata.cs
- Added `UniqueValuesScannedDate`, `UniqueValues`, and `HasMoreThan200UniqueValues` to `FieldMetadata`

### Services/DatabaseService.cs
- Added `GetUniqueValuesAsync()` method

### Forms/DatabaseLibraryManagerContentForm.cs
- Added `_fieldsListView` control
- Added `_getUniqueValuesButton` control
- Added `ShowTablesView()` and `ShowFieldsView()` methods
- Modified `TreeView_AfterSelect()` to detect table clicks
- Added `GetUniqueValuesButton_Click()` handler
- Updated `UpdateLayout()` to position fields view
- Updated `ApplyTheme()` to style fields view

## Testing Checklist

- [ ] Click on table in My_Library - fields view appears
- [ ] Check one or more fields
- [ ] Click "Get Unique Values"
- [ ] Verify scan date updates
- [ ] Verify unique values appear (if ≤ 200)
- [ ] Verify "More than 200" message (if > 200)
- [ ] Close and reopen app
- [ ] Click same table - verify data persists
- [ ] Test with various field types (varchar, int, datetime, etc.)
- [ ] Test with NULL values in fields
- [ ] Test with exactly 200, 201, and 199 unique values

## Future Enhancement Ideas

1. **Value Filtering**: Add search/filter in unique values column
2. **Export Values**: Export unique values to CSV/text file
3. **Value Statistics**: Show count, min, max, average for numeric fields
4. **Configurable Threshold**: Allow user to change the 200-value limit
5. **Value History**: Track changes in unique values over time
6. **Data Profiling**: Add more statistics (NULL count, blank count, etc.)
7. **Bulk Analysis**: "Analyze All Fields" button
8. **Progress Bar**: Show progress when analyzing multiple fields
9. **Cancel Operation**: Allow canceling long-running queries
10. **Value Grouping**: Group similar values (e.g., date ranges)

## Tips for Users

### Best Fields to Analyze
- Status/State fields (usually < 10 values)
- Category/Type fields (usually < 50 values)
- Priority fields (High/Medium/Low)
- Boolean/Yes/No fields
- Country/State codes

### Fields to Avoid Analyzing
- Primary key fields (CustomerId, OrderId, etc.)
- Unique identifier fields (GUIDs)
- Free-text fields (Comments, Notes, Descriptions)
- Timestamp fields (too many unique values)

### Use Cases
- **Data Validation**: Understand what values exist in a field
- **Query Building**: See available filter options
- **Data Quality**: Identify unexpected values
- **Documentation**: Quick reference for valid field values
- **Integration**: Know what values to expect when integrating systems
