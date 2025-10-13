using SuiteView.Models;
using System.Runtime.InteropServices;

namespace SuiteView.Forms;

/// <summary>
/// Reusable bordered window form that provides the iconic SuiteView look
/// Can wrap any content form to give it the brass border and resizable functionality
/// </summary>
public class BorderedWindowForm : Form
{
    private Form? _contentForm;
    private readonly ColorTheme _theme;
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

    public BorderedWindowForm(ColorTheme theme, Size? initialSize = null, Size? minimumSize = null)
    {
        _theme = theme;
        InitializeComponent(initialSize, minimumSize);
    }

    private void InitializeComponent(Size? initialSize, Size? minimumSize)
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = minimumSize ?? new Size(400, 300);
        this.Size = initialSize ?? new Size(800, 600);
        this.BackColor = _theme.Accent; // Brass border
        this.TopMost = false;

        // Enable double buffering
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();

        // Add paint event for 3D border effect
        this.Paint += BorderedWindowForm_Paint;
    }

    private void BorderedWindowForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var brassColor = _theme.Accent;
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

    public void SetContentForm(Form contentForm)
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

        // If content form has a parent reference method, call it
        if (_contentForm is IContentForm contentFormInterface)
        {
            contentFormInterface.SetParentBorderForm(this);
        }
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
}

/// <summary>
/// Interface for content forms that need to communicate with their parent border form
/// </summary>
public interface IContentForm
{
    void SetParentBorderForm(Form parent);
}
