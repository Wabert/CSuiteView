# SuiteView - Windows Forms Dockable Toolbar Application

A lightweight, extensible dockable toolbar designed to host custom widgets and tools for Windows 10/11.

## Features

### Core Functionality
- **Dockable Toolbar**: Drag the toolbar to any screen edge (top, bottom, left, right) for instant snapping
- **Multi-Monitor Support**: Works seamlessly across multiple monitors
- **System Tray Integration**: Minimizes to system tray with quick access menu
- **Always-on-Top**: Optional always-on-top behavior
- **Customizable Themes**: 5 professionally designed green-themed color schemes
- **Startup Integration**: Optional launch on Windows startup
- **Resizable**: Easily resize the toolbar using the bottom-right grip
- **Settings Persistence**: All settings saved to JSON file in %AppData%\SuiteView

### Available Themes
1. **Emerald** - Classic vibrant green with charcoal (Default)
2. **Matrix** - Digital green with black
3. **Forest** - Deep forest green with blue-gray
4. **Mint** - Soft mint green with navy
5. **Neon** - Electric green with dark purple-gray

## Technical Details

### Architecture
The application is structured with clean separation of concerns:

- **Models/** - Data models (AppSettings, ColorTheme, DockPosition)
- **Managers/** - Business logic managers:
  - `ConfigManager` - JSON configuration persistence
  - `ThemeManager` - Color theme management
  - `DockingManager` - Docking logic and multi-monitor support
  - `SystemTrayManager` - System tray integration
  - `StartupManager` - Windows registry startup integration
- **Forms/** - UI forms:
  - `MainForm` - Main dockable toolbar window
  - `SettingsForm` - Settings configuration dialog

### Key Features Implementation

#### Docking System
The docking system (`DockingManager.cs:32-134`) automatically detects when the toolbar is dragged within 50 pixels of any screen edge and snaps it into position. It supports:
- 8 dock positions (Top, Bottom, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight)
- Automatic orientation adjustment
- Multi-monitor awareness

#### Multi-Monitor Support
The application (`DockingManager.cs:188-217`) handles:
- Multiple connected monitors
- Monitor configuration changes
- Laptop scenarios (lid open/closed with external monitors)
- Graceful repositioning when saved monitor is disconnected

#### Configuration Persistence
Settings are stored in JSON format at `%AppData%\SuiteView\settings.json` ([ConfigManager.cs:16-117](ConfigManager.cs)) with the following structure:

```json
{
  "windowPosition": { "x": 0, "y": 0 },
  "windowSize": { "width": 80, "height": 300 },
  "dockPosition": "BottomRight",
  "monitorIndex": 0,
  "selectedTheme": "Emerald",
  "launchOnStartup": false,
  "opacity": 100,
  "alwaysOnTop": true,
  "isVisible": false
}
```

#### System Tray
The system tray icon ([SystemTrayManager.cs:11-101](SystemTrayManager.cs)) provides:
- Right-click context menu with Show/Hide, Settings, and Exit options
- Double-click to toggle toolbar visibility
- Startup notification balloon tip
- Dynamic icon generation

#### Startup Registry Integration
The application can automatically start with Windows by adding an entry to:
`HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`

This is managed by [StartupManager.cs:10-93](StartupManager.cs).

## Building the Project

### Requirements
- .NET 8.0 SDK or later
- Windows 10/11 (for running)
- Linux/WSL with `EnableWindowsTargeting` (for cross-compilation)

### Build Commands

**Debug Build:**
```bash
dotnet build
```

**Release Build:**
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

**Self-Contained Release (includes .NET runtime):**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

The compiled executable will be in:
- Debug: `bin/Debug/net8.0-windows/`
- Release: `bin/Release/net8.0-windows/win-x64/publish/`

## Usage

### First Launch
1. Run `SuiteView.exe`
2. The application starts minimized to the system tray
3. Double-click the tray icon to show the toolbar
4. Drag the toolbar to any screen edge to dock it

### Settings
Right-click the system tray icon and select "Settings" to configure:
- **Color Theme**: Choose from 5 preset themes
- **Launch on Startup**: Auto-start with Windows
- **Window Opacity**: Adjust transparency (70-100%)
- **Always on Top**: Keep toolbar above other windows

### Docking
1. Click and drag the title bar (green area with "SuiteView" text)
2. Drag near any screen edge (within 50 pixels)
3. Release to snap into position
4. The toolbar automatically saves its position

### Resizing
- Use the resize grip in the bottom-right corner
- Minimum size: 60×100 pixels
- Maximum size: 800×800 pixels

## Extensibility

The toolbar is designed to be easily extended with custom widgets. The content area is a `Panel` control in [MainForm.cs:84](MainForm.cs#L84):

```csharp
// Future widgets and tools will be added here
// Example: _contentPanel.Controls.Add(new CustomWidget());
```

To add custom widgets:
1. Create a custom `UserControl` for your widget
2. Add it to `_contentPanel` in the `InitializeComponent()` method
3. The widget will automatically be themed and positioned

## Project Structure

```
SuiteView/
├── Models/
│   ├── AppSettings.cs       # Application settings data model
│   └── ColorTheme.cs         # Color theme data model
├── Managers/
│   ├── ConfigManager.cs      # JSON configuration management
│   ├── ThemeManager.cs       # Theme management with 5 presets
│   ├── DockingManager.cs     # Docking logic and multi-monitor support
│   ├── SystemTrayManager.cs  # System tray integration
│   └── StartupManager.cs     # Windows startup registry management
├── Forms/
│   ├── MainForm.cs           # Main toolbar window
│   └── SettingsForm.cs       # Settings configuration dialog
├── Utils/                    # Reserved for future utilities
├── Program.cs                # Application entry point
└── SuiteView.csproj          # Project configuration
```

## Known Limitations

- **Windows Only**: This is a Windows Forms application and only runs on Windows
- **Icon**: Currently uses a programmatically generated icon (green circle)
- **No Widgets Yet**: The toolbar is ready to host widgets, but none are included by default

## Future Enhancements

Potential additions:
- Clock widget
- System monitor (CPU, RAM, Network)
- Quick launch buttons
- Notes/todo widget
- Weather widget
- Calendar widget
- Custom widget API

## License

This is a project created for demonstration purposes. Feel free to use and modify as needed.

## Support

For issues or questions, please refer to the source code comments which provide detailed explanations of the implementation.
