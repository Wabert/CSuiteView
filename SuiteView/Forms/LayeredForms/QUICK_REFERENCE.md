# Layered Forms Quick Reference

## Form Types

### TwoLayerForm
**Use Case:** Simple forms with content area (similar to MainForm structure)

```
Structure:
└── BorderedWindowForm (5px brass border)
    └── TwoLayerForm
        ├── Layer 1 Header (30px royal blue)
        ├── Content Area (light blue)
        └── Layer 1 Footer (30px royal blue)

Example:
var form = LayeredFormFactory.CreateTwoLayerForm(
    formWidth: 800,
    formHeight: 600
);
form.Show();
```

### ThreeLayerForm
**Use Case:** Multi-panel forms with resizable sections

```
Structure:
└── BorderedWindowForm (5px brass border)
    └── ThreeLayerForm
        ├── Layer 1 Header (30px royal blue)
        ├── Layer 2 (medium blue)
        │   ├── Layer 2 Header (optional)
        │   ├── Layer 3 Panel Container
        │   │   ├── Panel 1
        │   │   ├── Splitter
        │   │   ├── Panel 2
        │   │   ├── Splitter
        │   │   └── Panel N...
        │   └── Layer 2 Footer (optional)
        └── Layer 1 Footer (30px royal blue)

Example:
var form = LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    layer2HeaderHeight: 30,    // Add header in Layer 2
    layer2FooterHeight: 30,    // Add footer in Layer 2
    panelCount: 3              // Three resizable panels
);
form.Show();
```

## Common Menu Configurations

### No Layer 2 Headers/Footers (Clean Panel View)
```csharp
LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    panelCount: 3,
    theme: _currentTheme);
// Layer 2 headers/footers default to 0 (not shown)
```

### With Layer 2 Headers/Footers (Decorated Panel View)
```csharp
LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    layer2HeaderHeight: 30,
    layer2FooterHeight: 30,
    panelCount: 3,
    theme: _currentTheme);
// Shows decorative borders at top/bottom of Layer 2
```

### Header Only
```csharp
LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    layer2HeaderHeight: 30,
    // layer2FooterHeight defaults to 0
    panelCount: 3,
    theme: _currentTheme);
```

### Footer Only
```csharp
LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    layer2FooterHeight: 30,
    // layer2HeaderHeight defaults to 0
    panelCount: 3,
    theme: _currentTheme);
```

## Parameter Defaults

| Parameter | Default | Description |
|-----------|---------|-------------|
| formWidth | 1000 | Total form width |
| formHeight | 600 | Total form height |
| layer1HeaderHeight | 30 | Top border (royal blue) |
| layer1FooterHeight | 30 | Bottom border (royal blue) |
| layer2HeaderHeight | 0 | Optional top decoration (medium blue) |
| layer2FooterHeight | 0 | Optional bottom decoration (medium blue) |
| layer2BorderWidth | 15 | Left/right borders (medium blue) |
| panelCount | 3 | Number of panels (1-4) |
| minimumSize | 300x200 | Minimum window size |
| theme | Royal Classic | Color scheme |

## Color Legend

| Layer | Color Name | Hex | RGB |
|-------|------------|-----|-----|
| Brass Border | Gold | #FFD700 | (255, 215, 0) |
| Layer 1 | Royal Blue (PRIMARY) | #2C5AA0 | (44, 90, 160) |
| Layer 2 | Medium Blue (SECONDARY) | #3A71C5 | (58, 113, 197) |
| Layer 3 / Panels | Light Blue | #60A0FF | (96, 160, 255) |
| Splitters | Medium Blue (SECONDARY) | #3A71C5 | (58, 113, 197) |

## Feature Checklist

✅ Brass border (5px outer frame)  
✅ Close button (brass circle with blue X)  
✅ Form dragging (click-and-drag anywhere)  
✅ Themed colors (royal blue/walnut/brass)  
✅ Resizable panels (ThreeLayerForm only)  
✅ Double buffering (no flicker)  
✅ Flexible sizing (300x200 minimum)  

## Quick Tips

1. **Always use the factory** - Don't instantiate forms directly
2. **Default parameters are sensible** - Only specify what you need to change
3. **Panel count is flexible** - 1-4 panels supported
4. **Layer naming is hierarchical** - Layer 1 (outer) → Layer 2 (middle) → Layer 3 (inner panels)
5. **Headers/footers are optional** - Set to 0 to hide them

## Migration from IndependentThreePanelForm

```csharp
// OLD
var contentForm = new IndependentThreePanelForm(3, true, true, theme: theme);
var window = new BorderedWindowForm(theme, new Size(1200, 700), new Size(300, 200));
window.SetContentForm(contentForm);
window.Show();

// NEW
var window = LayeredFormFactory.CreateThreeLayerForm(
    formWidth: 1200,
    formHeight: 700,
    layer2HeaderHeight: 30,
    layer2FooterHeight: 30,
    panelCount: 3,
    minimumSize: new Size(300, 200),
    theme: theme);
window.Show();
```

## Extension Example

```csharp
// Custom four-layer form
public class FourLayerForm : LayeredFormBase
{
    public FourLayerForm(...) : base(...)
    {
        // Your custom implementation
    }
}

// Add to factory
public static BorderedWindowForm CreateFourLayerForm(...)
{
    var contentForm = new FourLayerForm(...);
    var borderedWindow = new BorderedWindowForm(...);
    borderedWindow.SetContentForm(contentForm);
    return borderedWindow;
}
```
