# Database Library Manager - Complete Workflow

## Navigation Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     Main SuiteView                          │
│                                                             │
│  ┌───────────────┐                                         │
│  │  DB Library   │  ← Click this button                    │
│  └───────────────┘                                         │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│            Database Library Manager Window                  │
├────────────────┬────────────────────────────────────────────┤
│  TreeView      │  Right Panel (Tables View)                 │
│                │                                            │
│  LifeProd_Lib  │  [No content yet]                         │
│  My_Library    │                                            │
└────────────────┴────────────────────────────────────────────┘
                    ↓ Click LifeProd_Library
┌─────────────────────────────────────────────────────────────┐
│            Database Library Manager Window                  │
├────────────────┬────────────────────────────────────────────┤
│  TreeView      │  Right Panel (Tables View)                 │
│                │  ┌────────────────────────────────┐        │
│► LifeProd_Lib  │  │ ☐ Customers  | 2024-10-13 | 5 │        │
│  My_Library    │  │ ☐ Orders     | Never      | 0 │        │
│                │  │ ☐ Products   | 2024-10-12 | 8 │        │
│                │  └────────────────────────────────┘        │
├────────────────┴────────────────────────────────────────────┤
│                            [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
                    ↓ Check boxes, click "Scan Selected"
┌─────────────────────────────────────────────────────────────┐
│            Database Library Manager Window                  │
├────────────────┬────────────────────────────────────────────┤
│  TreeView      │  Right Panel (Tables View)                 │
│                │  ┌────────────────────────────────┐        │
│► LifeProd_Lib  │  │☑ Customers | 2024-10-13 14:30| 5│      │
│  My_Library    │  │ ☐ Orders   | Never          | 0│       │
│                │  │☑ Products  | 2024-10-13 14:30| 8│      │
│                │  └────────────────────────────────┘        │
│                │  Tables now scanned with field metadata!   │
├────────────────┴────────────────────────────────────────────┤
│                            [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
                    ↓ Check boxes, click "Move to Library"
┌─────────────────────────────────────────────────────────────┐
│            Database Library Manager Window                  │
├────────────────┬────────────────────────────────────────────┤
│  TreeView      │  Right Panel (Tables View)                 │
│                │  ┌────────────────────────────────┐        │
│  LifeProd_Lib  │  │ ☐ Orders     | Never      | 0 │        │
│► My_Library    │  └────────────────────────────────┘        │
│  ├─Customers   │  Tables moved to My_Library!               │
│  └─Products    │                                            │
│                │                                            │
├────────────────┴────────────────────────────────────────────┤
│                            [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
                    ↓ Click on "Customers" table
┌─────────────────────────────────────────────────────────────┐
│            Database Library Manager Window                  │
├────────────────┬────────────────────────────────────────────┤
│  TreeView      │  Right Panel (Fields View)                 │
│                │  [Get Unique Values]  ← NEW BUTTON!        │
│  LifeProd_Lib  │  ┌──────────────────────────────────────┐  │
│  My_Library    │  │Field      DataType  Scanned  Values  │  │
│  ├►Customers   │  │─────────────────────────────────────│  │
│  │ ├─CustomerId│  │☐CustomerId  int     Never            │  │
│  │ ├─FirstName │  │☐FirstName   varchar Never            │  │
│  │ ├─LastName  │  │☐LastName    varchar Never            │  │
│  │ ├─Email     │  │☐Email       varchar Never            │  │
│  │ └─Status    │  │☐Status      varchar Never            │  │
│  └─Products    │  └──────────────────────────────────────┘  │
├────────────────┴────────────────────────────────────────────┤
│                            [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
                    ↓ Check "Status", click "Get Unique Values"
┌─────────────────────────────────────────────────────────────┐
│            Database Library Manager Window                  │
├────────────────┬────────────────────────────────────────────┤
│  TreeView      │  Right Panel (Fields View)                 │
│                │  [Get Unique Values]                       │
│  LifeProd_Lib  │  ┌──────────────────────────────────────┐  │
│  My_Library    │  │Field      DataType  Scanned  Values  │  │
│  ├►Customers   │  │─────────────────────────────────────│  │
│  │ ├─CustomerId│  │☐CustomerId  int     Never            │  │
│  │ ├─FirstName │  │☐FirstName   varchar Never            │  │
│  │ ├─LastName  │  │☐LastName    varchar Never            │  │
│  │ ├─Email     │  │☐Email       varchar Never            │  │
│  │ └─Status    │  │☑Status   varchar(20) 2024-10-13 15:00│  │
│  └─Products    │  │          Active, Inactive, Pending   │  │
│                │  └──────────────────────────────────────┘  │
│                │  Unique values analyzed and saved!         │
├────────────────┴────────────────────────────────────────────┤
│                            [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
```

## Key Concepts

### Two-Panel Layout
**Left Panel (TreeView):**
- **LifeProd_Library** = Database source (read-only)
- **My_Library** = Your curated collection
  - Contains scanned tables
  - Expandable to show fields

**Right Panel (Dynamic):**
- **Tables View** = Shows when clicking on LifeProd_Library or My_Library root nodes
- **Fields View** = Shows when clicking on a specific table in My_Library

### Data Flow

```
Database (SQL Server via ODBC)
         ↓
    [Scan Tables]
         ↓
  Table Metadata (schema info)
         ↓
   [Move to Library]
         ↓
   My_Library (TreeView)
         ↓
   [Click on Table]
         ↓
   Fields View (ListView)
         ↓
  [Select Fields & Get Unique Values]
         ↓
  Query Database for DISTINCT values
         ↓
  Store in FieldMetadata
         ↓
  Save to JSON (database_library.json)
         ↓
  Display in "Unique Values" column
```

### Storage Strategy

**≤ 200 Unique Values:**
```json
{
  "FieldName": "Status",
  "UniqueValuesScannedDate": "2025-10-13T15:00:00",
  "UniqueValues": ["Active", "Inactive", "Pending"],
  "HasMoreThan200UniqueValues": false
}
```
✅ All values stored
✅ Displayed in UI
✅ No re-query needed

**> 200 Unique Values:**
```json
{
  "FieldName": "CustomerId", 
  "UniqueValuesScannedDate": "2025-10-13T15:00:00",
  "UniqueValues": null,
  "HasMoreThan200UniqueValues": true
}
```
✅ Scan date recorded
✅ Message shown: "More than 200 unique values"
❌ Values not stored (too large)

## Button Visibility Logic

```
Current View               | Scan Selected | Move to Library | Get Unique Values
---------------------------|---------------|-----------------|------------------
LifeProd_Library (root)    | ✓ Visible     | ✓ Visible      | ✗ Hidden
My_Library (root)          | ✓ Visible     | ✓ Visible      | ✗ Hidden  
Table in My_Library        | ✗ Hidden      | ✗ Hidden       | ✓ Visible
```

## Complete Feature Summary

### Phase 1: Initial Setup (Previously Implemented)
1. Click "DB Library" button in SuiteView
2. Click "LifeProd_Library" → Loads tables from UL_Rates database
3. Check tables → Click "Scan Selected" → Gets field schema
4. Click "Move to Library" → Tables appear in My_Library

### Phase 2: Unique Values Analysis (Just Implemented!)
5. Click on a table in My_Library → Shows fields view
6. Check fields → Click "Get Unique Values" → Analyzes data
7. View unique values in the UI
8. Data persists to JSON
9. Reloading shows saved values instantly

## Example Use Case

### Scenario: Building a Customer Report
**Goal:** Need to know what Status values exist in Customers table

**Traditional Approach:**
1. Open SQL Server Management Studio
2. Write query: `SELECT DISTINCT Status FROM Customers`
3. Execute query
4. Copy results to notepad
5. Repeat for other fields

**With Database Library Manager:**
1. Click "Customers" in My_Library
2. Check "Status" field
3. Click "Get Unique Values"
4. See results instantly: "Active, Inactive, Pending"
5. Next time you need it, just click "Customers" - already there!

### Benefits
✅ No SQL knowledge required
✅ Results saved for future reference
✅ Fast lookup (no re-query)
✅ Visual interface
✅ Shareable (JSON can be committed to git)
