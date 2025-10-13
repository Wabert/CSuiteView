# Query Persistence Implementation

## Changes Made - October 13, 2025

### ✅ **Complete Query Persistence System**

Implemented full save/load functionality for queries with all their criteria and display fields.

---

## New Data Models

### QueryDefinition
Represents a complete saved query:
```csharp
public class QueryDefinition
{
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public List<QueryCriteriaField> CriteriaFields { get; set; }
    public List<QueryDisplayField> DisplayFields { get; set; }
}
```

### QueryCriteriaField
Stores criteria field with all its values:
```csharp
public class QueryCriteriaField
{
    public string TableName { get; set; }
    public string FieldName { get; set; }
    public string DataType { get; set; }
    public bool HasListBox { get; set; }
    public List<string> SelectedValues { get; set; }  // For CheckedListBox
    public string TextValue { get; set; }              // For TextBox
    public int PanelHeight { get; set; }               // Preserves resize
}
```

### QueryDisplayField
Stores display field information:
```csharp
public class QueryDisplayField
{
    public string TableName { get; set; }
    public string FieldName { get; set; }
    public string DataType { get; set; }
}
```

### Updated DatabaseLibraryConfig
```csharp
public class DatabaseLibraryConfig
{
    public List<DatabaseConfig> Databases { get; set; }
    public List<QueryDefinition> Queries { get; set; }  // NEW
}
```

---

## Key Features Implemented

### 1. Query Creation & Persistence
**NewQueryButton_Click**:
- Creates new `QueryDefinition` object
- Checks for duplicate names
- Saves immediately to JSON
- Stores `QueryDefinition` in TreeNode.Tag
- Auto-selects new query

### 2. Loading Saved Queries
**LoadQueries**:
- Called on form load
- Reads all queries from JSON config
- Creates TreeNode for each query
- Stores `QueryDefinition` in node Tag
- Auto-expands if queries exist

### 3. Restoring Query Fields
**LoadQueryFields**:
- Retrieves table/field metadata from database config
- Recreates criteria panels with saved:
  - Selected checkbox values
  - Text input values
  - Panel heights (from resizing)
- Recreates display field panels
- Preserves exact UI state

### 4. Creating Panels from Saved Data
**CreateCriteriaFieldPanelFromSaved**:
- Mirrors CreateCriteriaFieldPanel but with restore logic
- For CheckedListBox:
  - Restores all checked items
  - Preserves panel height
  - Re-adds resize grip
- For TextBox:
  - Restores text value
- Maintains all styling and remove button

### 5. Saving Query Changes
**SaveQueryButton_Click**:
- Iterates through all criteria panels
- Extracts CheckedListBox selections or TextBox values
- Saves panel heights (for resized list boxes)
- Iterates through all display panels
- Updates `LastModifiedDate`
- Saves to JSON via `DatabaseLibraryManager.SaveConfig()`
- Shows success message

---

## Data Flow

### Creating New Query
```
User enters name → NewQueryButton_Click
  ↓
Creates QueryDefinition
  ↓
Adds to _config.Queries
  ↓
Saves to JSON
  ↓
Creates TreeNode (stores QueryDefinition in Tag)
  ↓
Adds to My_Queries
  ↓
Auto-selects → ShowQueryBuilderView
```

### Adding Fields to Query
```
User drags field → CriteriaDropZone_DragDrop / DisplayDropZone_DragDrop
  ↓
CreateCriteriaFieldPanel / CreateDisplayFieldPanel
  ↓
Panel added to UI
  ↓
User clicks Save → SaveQueryButton_Click
  ↓
Extracts all field data from panels
  ↓
Updates QueryDefinition
  ↓
Saves to JSON
```

### Reopening Saved Query
```
Form loads → LoadConfiguration
  ↓
LoadQueries
  ↓
Reads _config.Queries
  ↓
Creates TreeNodes under My_Queries
  ↓
User clicks query → ShowQueryBuilderView
  ↓
Retrieves QueryDefinition from node.Tag
  ↓
LoadQueryFields
  ↓
CreateCriteriaFieldPanelFromSaved (for each criteria field)
  ↓
CreateDisplayFieldPanel (for each display field)
  ↓
UI restored exactly as saved
```

### Closing and Reopening App
```
Close Database Library Manager
  ↓
Reopen Database Library Manager
  ↓
LoadConfiguration called
  ↓
LoadQueries called
  ↓
All queries appear under My_Queries
  ↓
Click any query
  ↓
All fields and values restored
```

---

## JSON Structure

### Example Saved Query
```json
{
  "Databases": [ /* existing database data */ ],
  "Queries": [
    {
      "Name": "test",
      "CreatedDate": "2025-10-13T10:30:00",
      "LastModifiedDate": "2025-10-13T10:35:00",
      "CriteriaFields": [
        {
          "TableName": "TAICession",
          "FieldName": "_CessSeq",
          "DataType": "numeric",
          "HasListBox": true,
          "SelectedValues": ["1", "2", "5"],
          "TextValue": "",
          "PanelHeight": 120
        },
        {
          "TableName": "TAICession",
          "FieldName": "_Co",
          "DataType": "char",
          "HasListBox": false,
          "SelectedValues": [],
          "TextValue": "ABC",
          "PanelHeight": 35
        }
      ],
      "DisplayFields": [
        {
          "TableName": "TAICession",
          "FieldName": "_Co",
          "DataType": "char"
        },
        {
          "TableName": "TAICession",
          "FieldName": "_Pol",
          "DataType": "char"
        }
      ]
    }
  ]
}
```

---

## Persistence Features

### ✅ What Gets Saved
1. **Query Name** - Unique identifier
2. **Creation Date** - When query was first created
3. **Last Modified Date** - Last save timestamp
4. **Criteria Fields**:
   - Table and field names
   - Data types
   - CheckedListBox: All selected values
   - TextBox: Entered text
   - Panel height (preserves resizing)
5. **Display Fields**:
   - Table and field names
   - Data types

### ✅ What Gets Restored
1. **All query nodes** under My_Queries
2. **All criteria fields** with:
   - Correct controls (CheckedListBox or TextBox)
   - Previously selected checkbox values
   - Previously entered text
   - Original panel heights
3. **All display fields** in correct order
4. **Exact UI state** as when saved

---

## User Workflow

### Creating and Saving a Query
1. Click **My_Queries** node
2. Enter query name, click **New Query**
3. Drag fields from **My_Library** tables
4. Drop on **Criteria** tab (for WHERE conditions)
5. Drop on **Display** tab (for SELECT columns)
6. Check values in list boxes or enter text
7. Resize list boxes if needed
8. Click **Save** button
9. ✅ Query saved to JSON

### Reopening a Saved Query
1. Open Database Library Manager
2. Queries automatically appear under **My_Queries**
3. Click any query name
4. ✅ All fields and values restored exactly
5. Make changes, click **Save** again
6. ✅ Updates persist

### Multiple Queries
1. Create "Sales Report" query
2. Add fields, save
3. Click **My_Queries**, create "Customer List" query
4. Add different fields, save
5. Switch between queries
6. ✅ Each maintains its own state

---

## Technical Implementation

### Query Storage Location
- **File**: `AppData\Roaming\SuiteView\database_library.json`
- **Format**: JSON with UTF-8 encoding
- **Access**: Via `DatabaseLibraryManager.LoadConfig()` and `SaveConfig()`

### Tag Usage
- **TreeNode.Tag**: Stores `QueryDefinition` object
- **Panel.Tag**: Stores anonymous object `{ Table, Field }`
- Used for extracting metadata during save

### State Management
- `_currentQueryNode`: Active TreeNode
- `_currentQuery`: Active QueryDefinition
- Both updated in `ShowQueryBuilderView()`

---

## Fixed Issues

### ✅ Fields Disappearing
**Before**: Clicking away and back cleared all fields
**After**: Fields persist - loaded from saved QueryDefinition

### ✅ Save Button Did Nothing
**Before**: Showed message but didn't save
**After**: Saves all field data to JSON

### ✅ Queries Not Appearing
**Before**: Queries lost on restart
**After**: All queries loaded from JSON on startup

### ✅ Multiple Queries Support
**Before**: No support for multiple queries
**After**: Each query maintains independent state

---

## Next Steps

Future enhancements:
1. **SQL Generation**: Build SELECT/WHERE from saved fields
2. **Query Execution**: Run queries against database
3. **Results Display**: Show data grid on Results tab
4. **Export Results**: Save query results to Excel
5. **Query Duplication**: Copy existing query as template
6. **Delete Queries**: Right-click to delete saved queries
