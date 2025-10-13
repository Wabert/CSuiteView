# SuiteView - Quick Start Guide

## Installation

### Prerequisites
- Windows 10 or Windows 11
- .NET 8.0 Runtime (if using standard build)
  - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Install Steps
1. Download `SuiteView.exe` from the release package
2. Place it in your preferred location (e.g., `C:\Program Files\SuiteView\`)
3. Double-click to run

## First Time Setup

### 1. Initial Launch
When you first run SuiteView:
- The application starts minimized to the system tray
- Look for a green circle icon in your system tray (bottom-right of taskbar)
- You'll see a balloon notification: "SuiteView Started"

### 2. Show the Toolbar
To display the toolbar for the first time:
- **Option A**: Double-click the green system tray icon
- **Option B**: Right-click the tray icon → Select "Show SuiteView"

### 3. Position Your Toolbar
The toolbar appears at the bottom-right corner by default. To reposition:
1. Click and hold the green title bar (where "SuiteView" is written)
2. Drag toward any screen edge:
   - **Top**: Toolbar will be horizontal across the top
   - **Bottom**: Toolbar will be horizontal across the bottom
   - **Left**: Toolbar will be vertical on the left side
   - **Right**: Toolbar will be vertical on the right side
   - **Corners**: Drag to corners for compact corner positioning
3. When you're within 50 pixels of an edge, the toolbar will snap into place
4. Release the mouse button to dock

### 4. Resize the Toolbar
To adjust the toolbar size:
1. Look for the small grip in the bottom-right corner (three dots)
2. Click and drag to resize
3. Minimum size: 60 × 100 pixels
4. Maximum size: 800 × 800 pixels

## Customization

### Access Settings
1. Right-click the system tray icon
2. Select "Settings"
3. The Settings window will open

### Choose a Theme
Pick from 5 professional green themes:
1. **Emerald** (Default) - Vibrant teal-green with charcoal
2. **Matrix** - Digital green with black (hacker aesthetic)
3. **Forest** - Natural green with blue-gray
4. **Mint** - Soft turquoise green with navy
5. **Neon** - Electric bright green with purple

**To apply:**
- Select theme from dropdown
- Preview updates immediately
- Click "Save" to keep the changes

### Adjust Transparency
Make the toolbar more or less transparent:
1. Move the "Window Opacity" slider
2. Range: 70% (very transparent) to 100% (solid)
3. Great for blending with desktop or staying subtle

### Auto-Start with Windows
To launch SuiteView automatically when you log in:
1. Check "Launch on Windows Startup"
2. Click "Save"
3. SuiteView will now start with Windows

### Always on Top
Keep the toolbar above all other windows:
1. Check "Always on Top" (enabled by default)
2. Uncheck to allow other windows to overlap the toolbar

## Daily Usage

### Show/Hide the Toolbar
**Three ways to toggle visibility:**
1. Double-click the system tray icon
2. Right-click tray icon → "Show SuiteView" or "Hide SuiteView"
3. Click the "×" button on the toolbar (minimizes to tray)

### Move to Another Monitor
If you have multiple monitors:
1. Drag the toolbar to the desired monitor
2. Position it near an edge to dock
3. SuiteView remembers which monitor you prefer

### Exit the Application
To fully close SuiteView:
1. Right-click the system tray icon
2. Select "Exit"
3. This completely closes the application (not just hide)

## Tips & Tricks

### Multi-Monitor Setup
- Dock on secondary monitor for quick access while working on primary
- SuiteView automatically handles monitor disconnects (e.g., closing laptop lid)

### Opacity for Transparency
- Set to 70-80% to see through the toolbar
- Useful for monitoring while keeping your desktop visible

### Corner Docking
- Drag to screen corners for minimal screen space usage
- Perfect for small always-visible controls

### Quick Access via Tray
- Keep toolbar hidden most of the time
- Double-click tray icon when you need it
- Instant access without cluttering your screen

## Configuration File Location

Your settings are automatically saved to:
```
%AppData%\SuiteView\settings.json
```

Full path example:
```
C:\Users\YourUsername\AppData\Roaming\SuiteView\settings.json
```

This file contains all your preferences and is automatically created on first run.

## Troubleshooting

### Issue: Application won't start
**Solution:**
- Verify .NET 8.0 Runtime is installed
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- Install the "Desktop Runtime" package

### Issue: Can't find the system tray icon
**Solution:**
- Click the "^" arrow in the system tray to expand hidden icons
- The green circle icon should be there
- Pin it: Drag the icon to the main tray area to keep it visible

### Issue: Toolbar appears on wrong monitor after restart
**Solution:**
- SuiteView saves monitor preference
- If monitor is disconnected, it defaults to primary monitor
- Simply drag it back to your preferred monitor

### Issue: Can't drag the toolbar
**Solution:**
- Click and hold the green title bar (top area with "SuiteView" text)
- Do not try to drag from the dark content area
- Make sure you're not clicking the "×" button

### Issue: Settings won't save
**Solution:**
- Check that you have write permissions to %AppData%
- Try running as administrator (right-click → Run as Administrator)
- Verify the SuiteView folder exists in: `%AppData%\SuiteView\`

### Issue: "Launch on Startup" doesn't work
**Solution:**
- Requires administrator privileges to modify registry
- Try running SuiteView as administrator once
- Check Windows Task Manager → Startup tab for "SuiteView" entry

## Keyboard Shortcuts

Currently, SuiteView uses mouse interactions only. Future versions may include:
- Hotkey to show/hide toolbar
- Hotkey to open settings
- Custom widget shortcuts

## Uninstallation

To remove SuiteView:
1. Exit the application (right-click tray icon → Exit)
2. Delete the SuiteView.exe file
3. (Optional) Delete settings folder: `%AppData%\SuiteView\`
4. (Optional) If "Launch on Startup" was enabled:
   - Open Registry Editor (Win+R → type "regedit")
   - Navigate to: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
   - Delete the "SuiteView" entry

## What's Next?

SuiteView is designed to be extensible. Future updates may include:
- Clock widget
- System monitor (CPU, RAM, Network)
- Quick launch buttons
- Notes/todo widget
- Weather widget
- And more!

## Need Help?

For issues or questions:
- Check this Quick Start Guide
- Review the README.md for technical details
- Check the source code comments for implementation details

## Version Information

- **Current Version**: 1.0.0
- **Framework**: .NET 8.0
- **Platform**: Windows 10/11
- **File Size**: 181 KB

---

**Enjoy using SuiteView!** 🟢
