# Layered Forms Module

## Overview
The **Layered Forms** module provides a reusable pattern for creating multi-layered forms with consistent styling in CSuiteView. All forms feature brass borders, royal blue/walnut theming, draggability, and customizable dimensions.

## Architecture

### Class Hierarchy
```
LayeredFormBase (abstract)
├── TwoLayerForm
└── ThreeLayerForm

LayeredFormFactory (static)
SimpleSplitter (control)
```

### Layer Structure

#### TwoLayerForm
```
┌─────────────────────────────────────┐
│ BorderedWindowForm (5px brass)      │
│ ┌─────────────────────────────────┐ │
│ │ Layer 1: Form Container         │ │
│ │ (PRIMARY color #2C5AA0)         │ │
│ │ ┌────────────────────────────┐  │ │
│ │ │ Header (30px default)      │  │ │
│ │ ├────────────────────────────┤  │ │
│ │ │                            │  │ │
│ │ │   Content Panel            │  │ │
│ │ │   (LightBlue #60a0ff)      │  │ │
│ │ │                            │  │ │
│ │ ├────────────────────────────┤  │ │
│ │ │ Footer (30px default)      │  │ │
│ │ └────────────────────────────┘  │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

#### ThreeLayerForm
```
┌──────────────────────────────────────────┐
│ BorderedWindowForm (5px brass)           │
│ ┌──────────────────────────────────────┐ │
│ │ Layer 1: Form Container (30px)       │ │
│ │ (PRIMARY #2C5AA0)                    │ │
│ │ ┌──────────────────────────────────┐ │ │
│ │ │ Layer 2: Interior Panel (15px)   │ │ │
│ │ │ (SECONDARY #3a71c5)              │ │ │
│ │ │ ┌──────────────────────────────┐ │ │ │
│ │ │ │ Layer 3: Panel Container     │ │ │ │
│ │ │ │ (LightBlue #60a0ff)          │ │ │ │
│ │ │ │ ┌─────┬─┬─────┬─┬─────┐      │ │ │ │
│ │ │ │ │Panel│S│Panel│S│Panel│      │ │ │ │
│ │ │ │ │  1  │ │  2  │ │  3  │      │ │ │ │
│ │ │ │ └─────┴─┴─────┴─┴─────┘      │ │ │ │
│ │ │ └──────────────────────────────┘ │ │ │
│ │ └──────────────────────────────────┘ │ │
│ └──────────────────────────────────────┘ │
└──────────────────────────────────────────┘
```

## Usage

### Creating a Two-Layer Form

```csharp
using SuiteView.Forms.LayeredForms;

// Simple creation
var form = LayeredFormFactory.CreateTwoLayerForm();
form.Show();

// Customized
var form = LayeredFormFactory.CreateTwoLayerForm(
    formWidth: 1200,
    formHeight: 800,
    layer1HeaderHeight: 40,
    layer1FooterHeight: 40,
    minimumSize: new Size(600, 400)
);
form.Show();

// Add content to the form
var contentForm = (TwoLayerForm)form.GetContentForm();
var myControl = new MyCustomControl();
contentForm.SetContent(myControl);
```

### Creating a Three-Layer Form

```csharp
// Three panels (default)
var form = LayeredFormFactory.CreateThreeLayerForm();
form.Show();

// Four panels with custom dimensions
var form = LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1600,
    formHeight: 900,
    layer1HeaderHeight: 30,
    layer1FooterHeight: 30,
    layer2HeaderHeight: 0,
    layer2FooterHeight: 0,
    layer2BorderWidth: 15,
    panelCount: 4,
    minimumSize: new Size(800, 500)
);
form.Show();

// Access individual panels
var contentForm = (ThreeLayerForm)form.GetContentForm();
Panel firstPanel = contentForm.GetPanel(0);
Panel secondPanel = contentForm.GetPanel(1);

// Add controls to panels
firstPanel.Controls.Add(new TreeView { Dock = DockStyle.Fill });
secondPanel.Controls.Add(new DataGridView { Dock = DockStyle.Fill });
```

## Parameters

### TwoLayerForm Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| formWidth | int | 1000 | Total form width in pixels |
| formHeight | int | 600 | Total form height in pixels |
| layer1HeaderHeight | int | 30 | Height of top border (Layer 1) |
| layer1FooterHeight | int | 30 | Height of bottom border (Layer 1) |
| theme | ColorTheme? | null | Color theme (defaults to "Royal Classic") |

### ThreeLayerForm Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| formWidth | int | 1000 | Total form width in pixels |
| formHeight | int | 600 | Total form height in pixels |
| layer1HeaderHeight | int | 30 | Height of top border (Layer 1) |
| layer1FooterHeight | int | 30 | Height of bottom border (Layer 1) |
| layer2HeaderHeight | int | 0 | Height of top border (Layer 2) |
| layer2FooterHeight | int | 0 | Height of bottom border (Layer 2) |
| layer2BorderWidth | int | 15 | Width of left/right borders (Layer 2) |
| panelCount | int | 3 | Number of resizable panels (1-4) |
| theme | ColorTheme? | null | Color theme (defaults to "Royal Classic") |

## Features

### Common Features (All Layered Forms)

1. **Brass Border**: 5px brass (#FFD700) outer border via BorderedWindowForm
2. **Close Button**: Brass round X button in top-right of Layer 1 header
3. **Form Dragging**: Click and drag anywhere (except buttons/splitters) to move window
4. **Themed Colors**: Uses ColorTheme for consistent royal blue/walnut/brass styling
5. **Double Buffering**: Flicker-free rendering during resize
6. **Resizable**: Flexible minimum sizes (default 300x200)

### ThreeLayerForm Specific Features

1. **Resizable Panels**: Drag splitters to resize panels proportionally
2. **1-4 Panels**: Support for 1, 2, 3, or 4 vertical panels
3. **Proportional Sizing**: Panels maintain proportions during form resize
4. **Panel Access**: GetPanel(index) and GetAllPanels() methods

## Color Scheme

| Element | Color | Hex | Usage |
|---------|-------|-----|-------|
| PRIMARY | Royal Blue | #2C5AA0 | Layer 1 (Form Container) |
| SECONDARY | Medium Blue | #3a71c5 | Layer 2 (Interior Panel), Splitters |
| LightBlue | Light Blue | #60a0ff | Content areas, panels |
| Accent | Brass/Gold | #FFD700 | Outer border, close button |

## Extension Points

### Creating Custom Layered Forms

Extend `LayeredFormBase` to create new layered form types:

```csharp
public class MyCustomLayeredForm : LayeredFormBase
{
    public MyCustomLayeredForm(
        int formWidth = 1000,
        int formHeight = 600,
        int layer1HeaderHeight = 30,
        int layer1FooterHeight = 30,
        ColorTheme? theme = null)
        : base(formWidth, formHeight, layer1HeaderHeight, layer1FooterHeight, theme)
    {
        // Initialize your custom structure
        InitializeCustomLayers();
        InitializeCloseButton();
    }
    
    private void InitializeCustomLayers()
    {
        // Your custom layer implementation
    }
    
    protected override bool ShouldPreventDragging(Point clientPos)
    {
        // Override if you have controls that should prevent dragging
        return false;
    }
}
```

### Adding to LayeredFormFactory

Add factory methods for new form types:

```csharp
public static BorderedWindowForm CreateMyCustomForm(/* parameters */)
{
    var contentForm = new MyCustomLayeredForm(/* ... */);
    var borderedWindow = new BorderedWindowForm(/* ... */);
    borderedWindow.SetContentForm(contentForm);
    return borderedWindow;
}
```

## Best Practices

1. **Use Factory Methods**: Always create forms via `LayeredFormFactory` for consistency
2. **Default Parameters**: Use sensible defaults (30px headers/footers, 15px borders)
3. **Minimum Sizes**: Set appropriate minimums to prevent unusable forms
4. **Panel Content**: Add controls via Dock=Fill for proper resizing behavior
5. **Theme Consistency**: Use the same theme across all forms in your application

## Migration from IndependentThreePanelForm

The old `IndependentThreePanelForm` is replaced by `ThreeLayerForm`:

```csharp
// OLD
var form = new IndependentThreePanelForm(3, true, true, theme: theme);
var bordered = new BorderedWindowForm(theme, new Size(1200, 700), new Size(300, 200));
bordered.SetContentForm(form);
bordered.Show();

// NEW
var form = LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    panelCount: 3,
    minimumSize: new Size(300, 200)
);
form.Show();
```

## Future Extensions

Potential future layered form types:

- **FourLayerForm**: Additional nested layer for complex UIs
- **HorizontalSplitForm**: Horizontal panel splitting instead of vertical
- **GridLayeredForm**: Grid-based panel layout (2x2, 3x3, etc.)
- **TabLayeredForm**: Tabbed interface with layered structure
- **DockableLayeredForm**: Panels can be docked/undocked

## Files

- `LayeredFormBase.cs` - Abstract base class
- `TwoLayerForm.cs` - Two-layer implementation
- `ThreeLayerForm.cs` - Three-layer implementation with panels
- `SimpleSplitter.cs` - Draggable splitter control
- `LayeredFormFactory.cs` - Factory for creating forms
- `README.md` - This documentation
