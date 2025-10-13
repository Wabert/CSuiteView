# Query Builder - Visual Guide

## Step-by-Step Visual Walkthrough

### Step 1: Navigate to My_Queries

```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ Right Panel                                  │
│              │ (showing some other view)                    │
│LifeProd_Lib. │                                              │
│└►My_Library  │                                              │
│  ├─Customers │                                              │
│  │ ├─Field1  │                                              │
│  │ └─Field2  │                                              │
│  ├─Orders    │                                              │
│  └►My_Queries│ ← CLICK HERE TO START                       │
│              │                                              │
│              │                                              │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘
```

### Step 2: New Query Panel Appears

```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ NEW QUERY PANEL                              │
│              │                                              │
│LifeProd_Lib. │ [Customer Status Query   ] [New Query]     │
│└─My_Library  │  ↑                          ↑               │
│  ├─Customers │  Type query name here       Click to start  │
│  │ ├─Field1  │                                              │
│  │ └─Field2  │                                              │
│  ├─Orders    │                                              │
│  └►My_Queries│                                              │
│              │                                              │
│              │                                              │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘

Actions:
1. Type "Customer Status Query" (or any name)
2. Click the brass "New Query" button
```

### Step 3: Query Builder Workspace Appears

```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ TABLES HEADER (empty initially)              │
│              │ ┌──────────────────────────────────────────┐ │
│LifeProd_Lib. │ │ (No tables yet)                         │ │
│└─My_Library  │ └──────────────────────────────────────────┘ │
│  ├►Customers │ ──────────────────────────────────────────── │
│  │ ├─CustId  │ FIELDS DROP ZONE                             │
│  │ ├─Name    │ ┌──────────────────────────────────────────┐ │
│  │ └─Status  │ │                                          │ │
│  ├─Orders    │ │  Drop fields from tree here!             │ │
│  │ ├─OrderId │ │                                          │ │
│  │ └─Status  │ │                                          │ │
│  └►My_Queries│ └──────────────────────────────────────────┘ │
│              │                                              │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘

Ready for drag-and-drop!
```

### Step 4: Drag First Field (with Unique Values)

```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ TABLES HEADER                                │
│              │ ┌──────────────────────────────────────────┐ │
│LifeProd_Lib. │ │ Customers                               │ │
│└─My_Library  │ └──────────────────────────────────────────┘ │
│  ├►Customers │ ──────────────────────────────────────────── │
│  │ ├─CustId  │ FIELDS DROP ZONE                             │
│  │ ├─Name    │                                              │
│  │ └─Status  │ ┌────────────────────────────────────────┐  │
│  ├─Orders    │ │ Customers.Status                       │  │
│  │ ├─OrderId │ │ ┌──────────────────┐                  │  │
│  │ └─Status  │ │ │ ☐ (none)        │                  │  │
│  └►My_Queries│ │ │ ☐ Active        │                  │  │
│              │ │ │ ☐ Inactive      │                  │  │
│              │ │ │ ☐ Pending       │                  │  │
│              │ │ │ ☐ Suspended     │                  │  │
│              │ │ └──────────────────┘                  │  │
│              │ └────────────────────────────────────────┘  │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘

What happened:
1. "Customers" appears in tables header
2. Field panel created with label "Customers.Status"
3. CheckedListBox shows with unique values
4. "(none)" is first option
```

### Step 5: Drag Second Field (without Unique Values)

```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ TABLES HEADER                                │
│              │ ┌──────────────────────────────────────────┐ │
│LifeProd_Lib. │ │ Customers                               │ │
│└─My_Library  │ └──────────────────────────────────────────┘ │
│  ├►Customers │ ──────────────────────────────────────────── │
│  │ ├─CustId  │ FIELDS DROP ZONE (scrollable)                │
│  │ └─Name    │                                              │
│  ├─Orders    │ ┌────────────────────────────────────────┐  │
│  │ ├─OrderId │ │ Customers.Status                       │  │
│  │ └─Status  │ │ ☐ (none) ☐ Active ☐ Inactive         │  │
│  └►My_Queries│ └────────────────────────────────────────┘  │
│              │                                              │
│              │ ┌────────────────────────────────────────┐  │
│              │ │ Customers.Name                         │  │
│              │ │ [                                   ]  │  │
│              │ └────────────────────────────────────────┘  │
│              │                                              │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘

What happened:
1. "Customers" table already in header (not duplicated)
2. New field panel created with label "Customers.Name"
3. TextBox appears (Name has no unique values scanned)
4. User can type search criteria
```

### Step 6: Add Field from Different Table

```
┌─────────────────────────────────────────────────────────────┐
│ Database Library Manager                                 [X]│
├──────────────┬──────────────────────────────────────────────┤
│ TreeView     │ TABLES HEADER                                │
│              │ ┌──────────────────────────────────────────┐ │
│LifeProd_Lib. │ │ Customers │ Orders                      │ │
│└─My_Library  │ └──────────────────────────────────────────┘ │
│  ├─Customers │ ──────────────────────────────────────────── │
│  ├►Orders    │ FIELDS DROP ZONE (scrollable)                │
│  │ ├─OrderId │                                              │
│  │ ├─Date    │ ┌────────────────────────────────────────┐  │
│  │ └─Status  │ │ Customers.Status                       │  │
│  └►My_Queries│ │ ☑ Active ☐ Inactive                   │  │
│              │ └────────────────────────────────────────┘  │
│              │                                              │
│              │ ┌────────────────────────────────────────┐  │
│              │ │ Customers.Name                         │  │
│              │ │ [John                               ]  │  │
│              │ └────────────────────────────────────────┘  │
│              │                                              │
│              │ ┌────────────────────────────────────────┐  │
│              │ │ Orders.Status                          │  │
│              │ │ ☑ Shipped ☑ Delivered                 │  │
│              │ └────────────────────────────────────────┘  │
├──────────────┴──────────────────────────────────────────────┤
│                              [Scan Selected] [Move to Library]│
└─────────────────────────────────────────────────────────────┘

What happened:
1. "Orders" added to tables header (now shows both tables)
2. Orders.Status field panel created
3. User has selected values in multiple fields
4. Query is building visually!

This query would find:
- Customers with Status = "Active"
- Name containing "John"
- Orders with Status = "Shipped" OR "Delivered"
```

## Field Panel Comparison

### Panel with CheckedListBox (Small Number of Values)
```
┌─────────────────────────────────────────────────────────────┐
│ Customers.Status                                            │
│                                                             │
│ ┌─────────────────────────┐                                │
│ │ ☐ (none)               │                                │
│ │ ☑ Active               │                                │
│ │ ☐ Inactive             │                                │
│ │ ☑ Pending              │                                │
│ │ ☐ Suspended            │                                │
│ └─────────────────────────┘                                │
│                                                             │
│ Height: 120px                                               │
└─────────────────────────────────────────────────────────────┘
```

### Panel with CheckedListBox (Many Values - Scrollable)
```
┌─────────────────────────────────────────────────────────────┐
│ Customers.Region                                            │
│                                                             │
│ ┌─────────────────────────┐▲                               │
│ │ ☐ (none)               ││                               │
│ │ ☑ Northeast            ││                               │
│ │ ☐ Southeast            ││                               │
│ │ ☐ Midwest              ││                               │
│ │ ☐ Southwest            ││                               │
│ │ ☐ West                 ││                               │
│ │ ☐ Northwest            ││                               │
│ │ ☐ Mountain             ││                               │
│ │ ☐ Pacific              ││                               │
│ │ ☐ Alaska               │█                               │
│ └─────────────────────────┘▼                               │
│                             ↑ Scroll bar appears           │
│ Height: 120px                                               │
└─────────────────────────────────────────────────────────────┘
```

### Panel with TextBox (No Unique Values)
```
┌─────────────────────────────────────────────────────────────┐
│ Customers.Name                                              │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ John                                                    ││
│ └─────────────────────────────────────────────────────────┘│
│                                                             │
│ Height: 60px                                                │
└─────────────────────────────────────────────────────────────┘
```

### Panel with TextBox (>200 Unique Values)
```
┌─────────────────────────────────────────────────────────────┐
│ Customers.CustomerId                                        │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ 12345                                                   ││
│ └─────────────────────────────────────────────────────────┘│
│                                                             │
│ Height: 60px                                                │
│                                                             │
│ (Field has >200 unique values, so shows textbox)           │
└─────────────────────────────────────────────────────────────┘
```

## Interaction Examples

### Example 1: Select Multiple Status Values
```
Before:                          After:
┌─────────────────────┐         ┌─────────────────────┐
│ ☐ (none)           │         │ ☐ (none)           │
│ ☐ Active           │  Click  │ ☑ Active           │
│ ☐ Inactive         │   ───►  │ ☐ Inactive         │
│ ☐ Pending          │  Click  │ ☑ Pending          │
│ ☐ Suspended        │   ───►  │ ☐ Suspended        │
└─────────────────────┘         └─────────────────────┘

Result: Query will filter for Active OR Pending
```

### Example 2: Use "(none)" Option
```
┌─────────────────────┐
│ ☑ (none)           │  ← Select this for "no filter"
│ ☐ Active           │
│ ☐ Inactive         │
│ ☐ Pending          │
│ ☐ Suspended        │
└─────────────────────┘

Result: Don't filter by this field at all
```

### Example 3: Type in TextBox
```
Before:                    After:
┌─────────────────────┐   ┌─────────────────────┐
│                     │   │ %Smith%            │
└─────────────────────┘   └─────────────────────┘
         ↓                          ↓
    Type "Smith"              LIKE search pattern
```

## Color Scheme Reference

### Tables Header Panel
- **Background**: (50, 50, 50) - Dark gray
- **Table Labels**: Brass/Accent color
- **Height**: 40px

### Fields Drop Zone
- **Background**: (55, 55, 55) - Slightly lighter gray
- **Scrollable**: Yes
- **AllowDrop**: true

### Field Panels
- **Background**: (45, 45, 45) - Dark charcoal
- **Border**: FixedSingle, black
- **Margin**: 10px all sides

### Field Labels
- **Color**: White
- **Font**: Segoe UI, 10pt, Bold

### CheckedListBox
- **Background**: (60, 60, 60)
- **ForeColor**: White
- **Border**: FixedSingle

### TextBox
- **Background**: (60, 60, 60)
- **ForeColor**: White
- **Border**: FixedSingle

## Tips for Users

### Drag-and-Drop Tips
1. **Only field nodes can be dragged** (not table nodes)
2. **Cursor changes** to copy icon when dragging
3. **Drop anywhere** in the drop zone
4. **Panels appear** instantly when dropped

### Query Building Tips
1. **Start simple**: Add one or two fields first
2. **Check values**: Select multiple values for OR logic
3. **Use "(none)"**: To indicate no filter on that field
4. **Mix and match**: Some listboxes, some textboxes
5. **Multiple tables**: Drop fields from different tables

### Field Selection Tips
1. **Status fields**: Great for listboxes (few values)
2. **ID fields**: Use textboxes (many values)
3. **Name fields**: Use textboxes with wildcards
4. **Date fields**: Use textboxes for ranges

## Troubleshooting

### Can't Drag a Node?
- **Cause**: Trying to drag a table node
- **Solution**: Expand the table and drag a field node

### Dropped Field Doesn't Show Up?
- **Cause**: Dropped outside the drop zone
- **Solution**: Drop in the gray area below the tables header

### Table Doesn't Appear in Header?
- **Cause**: First field from that table hasn't been dropped
- **Solution**: Drop at least one field from the table

### Can't See All Fields?
- **Cause**: Many fields dropped, need to scroll
- **Solution**: Use scrollbar on right side of drop zone
