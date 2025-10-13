# Query Builder Feature - Documentation

## Overview
The Query Builder allows you to visually build database queries by dragging and dropping fields from your library tables. Fields with unique values display as multi-select listboxes, while other fields show as text input boxes.

## Feature Components

### 1. My_Queries Node
- Located in the TreeView under "My_Library"
- Click to access the query builder interface
- Starting point for creating new queries

### 2. New Query Panel
When you click "My_Queries", you see:
- **Query Name TextBox** - Enter a name for your query
- **New Query Button** (brass) - Click to start building

### 3. Query Builder Workspace
After clicking "New Query", you get a blank workspace with:
- **Tables Header Panel** - Shows which tables are used in your query
- **Fields Drop Zone** - Where you drop fields from the TreeView

## User Workflow

### Creating a Query

1. **Navigate to My_Queries**
   - Expand "My_Library" in the TreeView
   - Click on "My_Queries"

2. **Start New Query**
   - Enter a query name in the text box
   - Click "New Query" button
   - Query builder workspace appears

3. **Add Fields via Drag-and-Drop**
   - Expand a table in "My_Library" tree
   - Drag a field node from the tree
   - Drop it onto the query builder area
   - Field panel automatically creates

4. **Table Header Updates**
   - First time you drop a field from a table:
   - Table name appears in the header panel
   - Tracks all tables used in your query

5. **Field Panels Display**
   Each dropped field shows:
   - **Table.FieldName** label (e.g., "Customers.Status")
   - **Input control** based on field type:
     - **Multi-Select ListBox**: If field has unique values
     - **TextBox**: If field has no unique values or > 200 values

### Field Types

#### Fields with Unique Values (≤ 200)
**Display:** CheckedListBox with scroll bar
**Features:**
- Shows "(none)" as first option
- Lists all unique values
- Check multiple values
- Height: ~10 items visible
- Vertical scrollbar if more items

**Example:**
```
Status field (3 unique values):
┌─────────────────────┐
│ ☐ (none)           │
│ ☐ Active           │
│ ☐ Inactive         │
│ ☐ Pending          │
└─────────────────────┘
```

#### Fields without Unique Values
**Display:** Regular TextBox
**Use Cases:**
- Fields with > 200 unique values
- Fields not yet analyzed
- Numeric fields for custom input

**Example:**
```
CustomerId field:
┌─────────────────────┐
│                     │
└─────────────────────┘
```

## Layout Structure

### Query Builder View
```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ Tables Header Panel                          │
│              │ ┌──────────┬──────────┬──────────┐          │
│LifeProd_Lib. │ │Customers │  Orders  │ Products │          │
│└─My_Library  │ └──────────┴──────────┴──────────┘          │
│  ├─Customers │ ──────────────────────────────────────────── │
│  │ ├─Field1  │ Fields Drop Zone (scrollable)               │
│  │ └─Field2  │                                              │
│  ├─Orders    │ ┌────────────────────────────────────────┐  │
│  └►My_Queries│ │ Customers.Status                       │  │
│              │ │ ☐ (none)  ☐ Active  ☐ Inactive       │  │
│              │ └────────────────────────────────────────┘  │
│              │                                              │
│              │ ┌────────────────────────────────────────┐  │
│              │ │ Customers.FirstName                    │  │
│              │ │ [                                   ]  │  │
│              │ └────────────────────────────────────────┘  │
│              │                                              │
│              │ ┌────────────────────────────────────────┐  │
│              │ │ Orders.OrderStatus                     │  │
│              │ │ ☐ (none)  ☐ Shipped  ☐ Pending       │  │
│              │ │ ☐ Delivered                           │  │
│              │ └────────────────────────────────────────┘  │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
```

### New Query View
```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ Query Name: [________________] [New Query]  │
│              │                                              │
│LifeProd_Lib. │                                              │
│└─My_Library  │                                              │
│  ├─Customers │                                              │
│  ├─Orders    │                                              │
│  └►My_Queries│                                              │
│              │                                              │
│              │                                              │
│              │                                              │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
```

## Technical Implementation

### Drag-and-Drop Process

1. **TreeView_ItemDrag Event**
   - Triggered when user drags a TreeView node
   - Only allows dragging if node contains `FieldMetadata`
   - Initiates drag-drop operation with `DragDropEffects.Copy`

2. **FieldsDropZone_DragEnter Event**
   - Validates dragged data is a TreeNode
   - Sets cursor effect to Copy if valid

3. **FieldsDropZone_DragDrop Event**
   - Extracts `FieldMetadata` from dragged node
   - Gets parent `TableMetadata`
   - Adds table to header if not already present
   - Creates field panel with appropriate controls

### Field Panel Creation

```csharp
CreateFieldPanel(TableMetadata table, FieldMetadata field)
```

**Steps:**
1. Create panel container (45,45,45 background)
2. Add label: `{table.TableName}.{field.FieldName}`
3. Check if field has unique values:
   - **If YES**: Create CheckedListBox with values
   - **If NO**: Create TextBox for manual entry
4. Add panel to drop zone

### Control Specifications

#### Field Panel
- **Size**: Width = Drop zone width - 40px
- **Height**: 60px (textbox) or 120px (listbox)
- **BackColor**: (45, 45, 45)
- **Border**: FixedSingle
- **Margin**: 10px all sides

#### Field Label
- **Text**: `{TableName}.{FieldName}`
- **Font**: Segoe UI, 10pt, Bold
- **ForeColor**: White
- **Location**: (10, 10)

#### CheckedListBox (for fields with values)
- **Location**: (250, 5)
- **Size**: (300, 100)
- **BackColor**: (60, 60, 60)
- **ForeColor**: White
- **BorderStyle**: FixedSingle
- **CheckOnClick**: true
- **First Item**: "(none)"
- **Other Items**: Field's unique values

#### TextBox (for fields without values)
- **Location**: (250, 10)
- **Size**: (300, 25)
- **BackColor**: (60, 60, 60)
- **ForeColor**: White
- **BorderStyle**: FixedSingle
- **Font**: Segoe UI, 9pt

### Data Tracking

```csharp
private List<string> _currentQueryTables = new();
```

- Tracks which tables are used in the current query
- Updates when fields are dropped
- Used to populate tables header panel

### Tables Header Panel

```csharp
FlowLayoutPanel _tablesHeaderPanel
```

**Properties:**
- **FlowDirection**: LeftToRight
- **WrapContents**: true
- **BackColor**: (50, 50, 50)
- **Height**: 40px
- **AutoSize**: false

**Child Labels:**
- Created for each unique table
- **Text**: Table name
- **Font**: Segoe UI, 10pt, Bold
- **ForeColor**: Brass/Accent
- **Padding**: (10, 10, 10, 10)

## Code Files Modified

### Forms/DatabaseLibraryManagerContentForm.cs

**New Controls Added:**
- `_newQueryPanel` - New query creation panel
- `_queryNameTextBox` - Query name input
- `_newQueryButton` - Creates new query
- `_queryBuilderPanel` - Main builder workspace
- `_tablesHeaderPanel` - Shows tables in query
- `_fieldsDropZone` - Accepts dropped fields

**New Data Members:**
- `_myQueriesNode` - TreeView node for queries
- `_currentQueryTables` - List of table names in query

**New Methods:**
- `ShowNewQueryView()` - Displays new query panel
- `ShowQueryBuilderView()` - Displays builder workspace
- `NewQueryButton_Click()` - Handles new query creation
- `TreeView_ItemDrag()` - Initiates drag operation
- `FieldsDropZone_DragEnter()` - Validates drop target
- `FieldsDropZone_DragDrop()` - Handles field drop
- `CreateFieldPanel()` - Creates field UI panel

**Modified Methods:**
- `LoadConfiguration()` - Adds My_Queries node
- `TreeView_AfterSelect()` - Handles My_Queries selection
- `ShowTablesView()` - Hides query builder panels
- `ShowFieldsView()` - Hides query builder panels
- `UpdateLayout()` - Positions query builder controls

## User Experience Features

### Visual Feedback
1. **Drag Cursor**: Changes to copy icon when dragging fields
2. **Drop Highlight**: Visual indication when hovering over drop zone
3. **Table Header**: Automatically updates as tables are added
4. **Panel Creation**: Instant visual feedback when field is dropped

### Workflow Enhancements
1. **Auto-Table Tracking**: No need to manually select tables
2. **Smart Input Controls**: Automatically chooses listbox vs textbox
3. **Scrollable Workspace**: Handle queries with many fields
4. **Multi-Select Support**: Choose multiple filter values easily

### User-Friendly Design
1. **"(none)" Option**: Clear way to indicate no filter
2. **Visible Table Names**: Always know which table each field belongs to
3. **Consistent Styling**: Matches SuiteView theme
4. **Intuitive Drag-Drop**: Natural interaction pattern

## Future Enhancements (Not Yet Implemented)

### Phase 2 - Query Execution
- Generate SQL from selected fields and values
- Execute query against database
- Display results in grid

### Phase 3 - Query Persistence
- Save queries to JSON configuration
- Load saved queries from My_Queries tree
- Edit existing queries

### Phase 4 - Advanced Features
- Join conditions between tables
- Sorting options
- Grouping/aggregation
- Export query results to Excel

### Phase 5 - Query Sharing
- Export query definitions
- Import query definitions
- Share queries with team

## Testing Checklist

### Basic Functionality
- [ ] My_Queries node appears under My_Library
- [ ] Click My_Queries shows new query panel
- [ ] Enter query name and click New Query
- [ ] Query builder workspace appears
- [ ] Tables header panel visible at top
- [ ] Fields drop zone visible below header

### Drag-and-Drop
- [ ] Can drag field nodes from tree
- [ ] Cursor changes to copy icon
- [ ] Can drop fields onto drop zone
- [ ] Field panels create correctly
- [ ] Cannot drag table nodes (only fields)

### Table Header
- [ ] First dropped field adds table to header
- [ ] Subsequent fields from same table don't duplicate
- [ ] Multiple tables show in header
- [ ] Table names display in brass color

### Field Panels - With Unique Values
- [ ] CheckedListBox appears
- [ ] "(none)" is first item
- [ ] All unique values listed
- [ ] Can check/uncheck items
- [ ] Scrollbar appears if > 10 items
- [ ] Panel height adjusts (120px)

### Field Panels - Without Unique Values
- [ ] TextBox appears
- [ ] Can type in textbox
- [ ] Panel height standard (60px)
- [ ] Accepts input correctly

### Multiple Fields
- [ ] Can drop multiple fields
- [ ] Panels stack vertically
- [ ] Drop zone scrolls if needed
- [ ] Each panel maintains state

### Visual Design
- [ ] Consistent dark theme
- [ ] Brass accent colors
- [ ] Readable text
- [ ] Proper spacing/margins
- [ ] Panels look professional

## Example Use Cases

### Use Case 1: Customer Status Query
**Goal:** Find all active customers

1. Click "My_Queries"
2. Enter "Active Customers" as query name
3. Click "New Query"
4. Drag "Status" field from Customers table
5. Check "Active" in the listbox
6. (Ready for execution in future phase)

### Use Case 2: Order Search by Date Range
**Goal:** Find orders in a date range

1. Create new query "Date Range Orders"
2. Drag "OrderDate" field from Orders table
3. Enter date range in textbox (e.g., "2024-01-01 to 2024-12-31")
4. Drag "Status" field
5. Select "Shipped" and "Delivered"

### Use Case 3: Multi-Table Query
**Goal:** Find premium customers with pending orders

1. Create new query "Premium Pending"
2. Drag "CustomerType" from Customers
3. Select "Premium"
4. Drag "OrderStatus" from Orders
5. Select "Pending"
6. **Tables Header shows:** "Customers | Orders"

## Architecture Notes

### Design Patterns Used
- **Drag-and-Drop Pattern**: Natural UI interaction
- **Factory Pattern**: CreateFieldPanel() creates appropriate controls
- **Observer Pattern**: Event handlers respond to user actions

### Performance Considerations
- Controls created on-demand (not pre-loaded)
- Only unique values fields get listboxes
- Drop zone uses auto-scroll for large queries
- Minimal re-layout during field addition

### Extensibility
- Easy to add new field types
- Can extend with custom filters
- Query definition can be serialized
- SQL generation logic separate from UI
