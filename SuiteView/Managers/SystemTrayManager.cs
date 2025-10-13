namespace SuiteView.Managers;

/// <summary>
/// Manages the system tray icon and context menu
/// </summary>
public class SystemTrayManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private bool _disposed;

    public event EventHandler? ShowHideClicked;
    public event EventHandler? SettingsClicked;
    public event EventHandler? ExitClicked;

    /// <summary>
    /// Gets or sets whether the toolbar is currently visible
    /// </summary>
    public bool IsToolbarVisible { get; set; }

    public SystemTrayManager()
    {
        // Create context menu
        _contextMenu = new ContextMenuStrip();

        var showHideItem = new ToolStripMenuItem("Show SuiteView");
        showHideItem.Click += (s, e) => ShowHideClicked?.Invoke(this, EventArgs.Empty);

        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (s, e) => SettingsClicked?.Invoke(this, EventArgs.Empty);

        var separatorItem = new ToolStripSeparator();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitClicked?.Invoke(this, EventArgs.Empty);

        _contextMenu.Items.AddRange(new ToolStripItem[]
        {
            showHideItem,
            settingsItem,
            separatorItem,
            exitItem
        });

        // Create notify icon
        _notifyIcon = new NotifyIcon
        {
            Icon = CreateDefaultIcon(),
            Text = "SuiteView - Dockable Toolbar",
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        // Double-click to toggle visibility
        _notifyIcon.DoubleClick += (s, e) => ShowHideClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates the context menu text based on toolbar visibility
    /// </summary>
    public void UpdateContextMenuText()
    {
        if (_contextMenu.Items[0] is ToolStripMenuItem showHideItem)
        {
            showHideItem.Text = IsToolbarVisible ? "Hide SuiteView" : "Show SuiteView";
        }
    }

    /// <summary>
    /// Shows a balloon tip notification
    /// </summary>
    public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 3000)
    {
        _notifyIcon.ShowBalloonTip(timeout, title, text, icon);
    }

    /// <summary>
    /// Creates a default application icon (blue circle with gold "SV")
    /// </summary>
    private Icon CreateDefaultIcon()
    {
        // Create a 32x32 bitmap for the icon
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);

        // Enable anti-aliasing for smooth graphics
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Draw royal blue circle background
        using (var brush = new SolidBrush(ColorTranslator.FromHtml("#2C5AA0")))
        {
            graphics.FillEllipse(brush, 2, 2, 28, 28);
        }

        // Draw gold "SV" text in center
        using (var font = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Pixel))
        using (var brush = new SolidBrush(ColorTranslator.FromHtml("#D4AF37")))
        {
            var text = "SV";
            var size = graphics.MeasureString(text, font);
            var x = (32 - size.Width) / 2;
            var y = (32 - size.Height) / 2;
            graphics.DrawString(text, font, brush, x, y);
        }

        // Draw subtle gold border around the circle
        using (var pen = new Pen(ColorTranslator.FromHtml("#D4AF37"), 1.5f))
        {
            graphics.DrawEllipse(pen, 2, 2, 28, 28);
        }

        // Convert bitmap to icon
        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);

        return icon;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
