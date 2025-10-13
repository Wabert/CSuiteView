using SuiteView.Managers;
using SuiteView.Models;
using System.Runtime.InteropServices;

namespace SuiteView.Forms;

/// <summary>
/// Parent form that provides resizable border functionality
/// Contains the content form as a child
/// </summary>
public class ResizableBorderForm : Form
{
    private readonly ConfigManager _configManager;
    private MainForm? _contentForm;
    private const int BorderThickness = 5;
    private const int ResizeBorderWidth = 10;

    // Windows API for rounded corners
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;

    // Windows message constants for resizing
    private const int WM_NCHITTEST = 0x84;
    private const int HTCLIENT = 1;
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    public ResizableBorderForm(ConfigManager configManager)
    {
        _configManager = configManager;

        InitializeComponent();
        RestorePosition();
    }

    private void InitializeComponent()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.MinimumSize = new Size(100, 150);
        this.MaximumSize = new Size(1200, 1200);
        this.BackColor = ColorTranslator.FromHtml("#FFD700"); // Brass border
        this.TopMost = _configManager.Settings.AlwaysOnTop;
        this.Opacity = _configManager.Settings.Opacity / 100.0;

        // Enable double buffering
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();

        // Add paint event for 3D border effect
        this.Paint += ResizableBorderForm_Paint;
    }

    private void ResizableBorderForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var brassColor = ColorTranslator.FromHtml("#D4AF37");
        var lightBrass = ControlPaint.Light(brassColor, 0.3f);
        var darkBrass = ControlPaint.Dark(brassColor, 0.2f);

        // Draw outer highlight (top and left)
        using (var pen = new Pen(lightBrass, 2))
        {
            // Top edge
            g.DrawLine(pen, 0, 0, this.Width, 0);
            // Left edge
            g.DrawLine(pen, 0, 0, 0, this.Height);
        }

        // Draw inner shadow (bottom and right)
        using (var pen = new Pen(darkBrass, 2))
        {
            // Bottom edge
            g.DrawLine(pen, 0, this.Height - 2, this.Width, this.Height - 2);
            // Right edge
            g.DrawLine(pen, this.Width - 2, 0, this.Width - 2, this.Height);
        }

        // Draw middle brass line for more depth
        using (var pen = new Pen(brassColor, 1))
        {
            // Inner rectangle
            g.DrawRectangle(pen, 1, 1, this.Width - 3, this.Height - 3);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // Apply rounded corners using Windows 11 API
        try
        {
            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
        catch
        {
            // Silently fail on older Windows versions
        }
    }

    public void SetContentForm(MainForm contentForm)
    {
        _contentForm = contentForm;
        _contentForm.TopLevel = false;
        _contentForm.FormBorderStyle = FormBorderStyle.None;
        _contentForm.Dock = DockStyle.None;

        // Position content form inside with border spacing
        _contentForm.Location = new Point(BorderThickness, BorderThickness);
        _contentForm.Size = new Size(
            this.ClientSize.Width - (BorderThickness * 2),
            this.ClientSize.Height - (BorderThickness * 2)
        );

        this.Controls.Add(_contentForm);
        _contentForm.Show();
    }

    protected override void WndProc(ref Message m)
    {
        // Handle hit testing for resizing
        if (m.Msg == WM_NCHITTEST)
        {
            base.WndProc(ref m);

            if ((int)m.Result == HTCLIENT)
            {
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));

                // Check corners first (they take priority)
                if (pos.X <= ResizeBorderWidth && pos.Y <= ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTTOPLEFT;
                    return;
                }
                if (pos.X >= this.Width - ResizeBorderWidth && pos.Y <= ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTTOPRIGHT;
                    return;
                }
                if (pos.X <= ResizeBorderWidth && pos.Y >= this.Height - ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                    return;
                }
                if (pos.X >= this.Width - ResizeBorderWidth && pos.Y >= this.Height - ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                    return;
                }

                // Check edges
                if (pos.X <= ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTLEFT;
                    return;
                }
                if (pos.X >= this.Width - ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTRIGHT;
                    return;
                }
                if (pos.Y <= ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTTOP;
                    return;
                }
                if (pos.Y >= this.Height - ResizeBorderWidth)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                    return;
                }
            }
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    private void RestorePosition()
    {
        var settings = _configManager.Settings;

        // Set size
        this.Size = new Size(settings.WindowSize.Width, settings.WindowSize.Height);

        // If no saved position, default to primary screen
        if (settings.WindowPosition.X == 0 && settings.WindowPosition.Y == 0)
        {
            var screen = Screen.PrimaryScreen;
            if (screen != null)
            {
                var workingArea = screen.WorkingArea;
                this.Location = new Point(
                    workingArea.Right - this.Width - 20,
                    workingArea.Bottom - this.Height - 20
                );
            }
        }
        else
        {
            this.Location = new Point(settings.WindowPosition.X, settings.WindowPosition.Y);
        }
    }

    private void SavePosition()
    {
        _configManager.UpdateSettings(settings =>
        {
            settings.WindowPosition.X = this.Location.X;
            settings.WindowPosition.Y = this.Location.Y;
            settings.WindowSize.Width = this.Width;
            settings.WindowSize.Height = this.Height;
        });
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // Update content form size to match
        if (_contentForm != null)
        {
            _contentForm.Size = new Size(
                this.ClientSize.Width - (BorderThickness * 2),
                this.ClientSize.Height - (BorderThickness * 2)
            );
        }

        this.Invalidate();
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);
        SavePosition();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
            _configManager.UpdateSettings(settings => settings.IsVisible = false);
        }
        else
        {
            base.OnFormClosing(e);
        }
    }

    public void UpdateOpacity(double opacity)
    {
        this.Opacity = opacity / 100.0;
    }

    public void UpdateAlwaysOnTop(bool alwaysOnTop)
    {
        this.TopMost = alwaysOnTop;
    }
}
