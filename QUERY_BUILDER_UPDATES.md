# Query Builder UI Updates

## Changes Made - October 13, 2025

### TreeView Structure Update
- **My_Queries is now a sibling to My_Library** (not a child)
  - TreeView structure:
    ```
    ├─ LifeProd_Library
    ├─ My_Library
    │  ├─ (scanned tables)
    └─ My_Queries
       └─ (user-created queries)
    ```

### New Query Creation Flow
1. Click on **My_Queries** → Shows "My_Queries" title at top with query name input
2. Enter query name in **white input box** (changed from black/dark gray)
3. Click **"New Query"** button → Creates a child node under My_Queries with that name
4. Query node is automatically selected and displays query builder

### Query Builder Interface
When a query is selected (clicked), the right panel shows:

#### Header Section
- **Title**: "My_Queries → [QueryName]" in bold at top left
- **Save Button**: Brass-colored button at top right
  - Saves the current criteria selections
  - Shows success message when clicked

#### Tab Control (fills remaining space)
Three tabs:
1. **Criteria Tab** (default)
   - White background (not black/dark)
   - Drag-and-drop zone for fields from My_Library tables
   - Fields appear as white panels with:
     - Field name label: `TableName.FieldName` (black text)
     - CheckedListBox for fields with unique values (white background)
     - TextBox for fields without unique values (white background)
   
2. **SQL Tab**
   - Empty for now (future: generated SQL query)
   
3. **Results Tab**
   - Empty for now (future: query execution results)

### Drag-and-Drop Behavior
- Expand a table in **My_Library** to see its fields
- Drag field nodes onto the **Criteria** tab
- Field panels appear with **white background** (no more black panels)
- Controls inside panels also have **white backgrounds** for clean appearance

### Key Features
- ✅ White input boxes and panels (improved readability)
- ✅ Title shows current context ("My_Queries" or "My_Queries → QueryName")
- ✅ Queries saved as child nodes under My_Queries
- ✅ Tab-based interface for organizing query building
- ✅ Save button for persisting criteria
- ✅ Clean white theme for query builder workspace

### Technical Details
**New Controls Added:**
- `_newQueryTitleLabel` - Shows "My_Queries" on new query panel
- `_queryBuilderTitleLabel` - Shows "My_Queries → QueryName" on builder
- `_saveQueryButton` - Brass-colored save button
- `_queryTabControl` - Tab control with 3 tabs
- `_criteriaTab`, `_sqlTab`, `_resultsTab` - Individual tab pages
- `_criteriaPanel` - White panel for drag-drop on Criteria tab
- `_currentQueryNode` - Tracks currently selected query

**Removed Controls:**
- `_tablesHeaderPanel` (replaced by tab structure)
- `_fieldsDropZone` (replaced by `_criteriaPanel`)

### Next Steps
Future enhancements:
1. **SQL Tab**: Generate SQL query from selected criteria
2. **Results Tab**: Execute query and display results in data grid
3. **Query Persistence**: Save/load queries from JSON
4. **Query Editing**: Edit existing query criteria
5. **Delete Queries**: Right-click menu to delete queries
