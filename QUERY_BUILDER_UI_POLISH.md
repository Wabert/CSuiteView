# Query Builder - UI Polish & Bug Fixes

## Changes Made - October 13, 2025

### 🐛 Fixed: Duplicate Fields Bug
**Problem**: When clicking away and coming back to add another field, duplicate fields appeared (2x, 3x, etc.).

**Root Cause**: The `TreeView_ItemDrag` event handler was being attached multiple times without removing the previous handler.

**Solution**: 
```csharp
// Remove existing handler before adding to prevent duplicates
_treeView.ItemDrag -= TreeView_ItemDrag;
_treeView.ItemDrag += TreeView_ItemDrag;
```

This ensures only one event handler is active at a time.

---

### 🎨 Softer, More Modern UI Design

#### Rounded Corners
Created a new `RoundedPanel` class with smooth, rounded borders:
- **8px border radius** for soft appearance
- Anti-aliased rendering for smooth edges
- Custom paint implementation for professional look

#### Subtle Remove Button
**Before**: 
- Harsh red × with black square border
- Large 25×25px button
- Jarring appearance

**After**:
- Soft gray × (Color: 150, 150, 150)
- Smaller 22×22px button
- Light gray background (240, 240, 240)
- Hover: Light pink (255, 200, 200)
- Click: Darker pink (255, 150, 150)
- No border, seamless integration

#### Softer Field Panels
- **Background**: Very light gray (250, 250, 250) instead of pure white
- **Border**: Light gray (220, 220, 220) instead of harsh black
- **Text**: Dark gray (60, 60, 60) instead of pure black
- Overall softer, more pleasing aesthetic

---

### 📏 Resizable List Boxes

Added drag-to-resize functionality for fields with unique values:

#### Features
- **Resize grip** in bottom-right corner: `⋮⋮`
- Cursor changes to `SizeNS` (vertical resize)
- Click and drag to increase/decrease height
- Minimum height: 80px (prevents collapsing)
- Smooth resizing experience

#### Implementation
```csharp
var resizeGrip = new Label
{
    Text = "⋮⋮",
    Cursor = Cursors.SizeNS,
    Location = bottom-right corner
};
```

**Mouse Events**:
- **MouseDown**: Capture starting position and height
- **MouseMove**: Calculate delta and resize panel + listbox
- **MouseUp**: Stop resizing

#### Anchoring
List boxes use proper anchoring:
```csharp
Anchor = AnchorStyles.Top | AnchorStyles.Left | 
         AnchorStyles.Right | AnchorStyles.Bottom
```
This ensures the list box resizes with the panel.

---

### Visual Comparison

#### Old Design
```
┌─────────────────────────────────────┐
│ [×] TableName.FieldName  [TextBox] │  ← Harsh black borders
└─────────────────────────────────────┘  ← Square corners
```

#### New Design
```
╭─────────────────────────────────────╮
│ ⊗ TableName.FieldName  [TextBox]   │  ← Rounded corners
╰─────────────────────────────────────╯  ← Soft gray borders
 ↑ Subtle remove button
```

For list boxes:
```
╭─────────────────────────────────────╮
│ ⊗ TableName.FieldName               │
│                    ┌──────────────┐ │
│                    │ ☐ (none)     │ │
│                    │ ☐ Value1     │ │
│                    │ ☐ Value2     │ │
│                    │ ☐ Value3     │ │
│                    └──────────⋮⋮──┘ │  ← Resize grip
╰─────────────────────────────────────╯
```

---

### Technical Details

#### RoundedPanel Class
**File**: `Forms/RoundedPanel.cs`

**Key Features**:
- Inherits from `Panel`
- Custom `OnPaint` override for rounded borders
- `GetRoundedRectPath()` creates smooth corner arcs
- Properties:
  - `BorderColor` - Customizable border color
  - `BorderRadius` - Adjustable corner radius
  - `DoubleBuffered` - Smooth rendering
  - `ResizeRedraw` - Auto-redraw on size changes

**Graphics Path**:
```csharp
// Creates 4 arcs for corners, connects with straight lines
path.AddArc(topLeft);
path.AddArc(topRight);
path.AddArc(bottomRight);
path.AddArc(bottomLeft);
path.CloseFigure();
```

#### Color Palette
- **Panel Background**: `FromArgb(250, 250, 250)` - Off-white
- **Panel Border**: `FromArgb(220, 220, 220)` - Light gray
- **Remove Button**: `FromArgb(240, 240, 240)` - Very light gray
- **Remove Button Text**: `FromArgb(150, 150, 150)` - Medium gray
- **Field Label**: `FromArgb(60, 60, 60)` - Dark gray
- **Hover Background**: `FromArgb(255, 200, 200)` - Light pink
- **Click Background**: `FromArgb(255, 150, 150)` - Medium pink

---

### User Experience Improvements

1. **No More Duplicates**: Fields add cleanly without multiplication bug
2. **Softer Aesthetics**: Easy on the eyes, professional appearance
3. **Better Remove Action**: Subtle button doesn't distract from content
4. **Flexible List Boxes**: Resize to see more/fewer values as needed
5. **Consistent Design**: Both Criteria and Display tabs use same styling

---

### Usage

#### Resizing List Boxes
1. Look for `⋮⋮` symbol in bottom-right of list box
2. Cursor changes to vertical resize arrows
3. Click and drag down to expand, up to shrink
4. Release to set new size

#### Removing Fields
1. Hover over the ⊗ button (turns light pink)
2. Click to remove field (no confirmation needed)
3. Field disappears immediately

---

### Next Steps

Future enhancements:
1. **Persist Resize Heights**: Save/load list box heights with query
2. **Double-Click to Remove**: Alternative to button click
3. **Keyboard Shortcuts**: Delete key to remove selected field
4. **Undo/Redo**: Restore accidentally removed fields
5. **Field Reordering**: Drag fields to reorder them
