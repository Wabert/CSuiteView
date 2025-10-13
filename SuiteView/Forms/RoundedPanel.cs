using System.Drawing.Drawing2D;

namespace SuiteView.Forms;

/// <summary>
/// A panel with rounded corners and customizable border
/// </summary>
public class RoundedPanel : Panel
{
    public Color BorderColor { get; set; } = Color.Gray;
    public int BorderRadius { get; set; } = 8;

    public RoundedPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Create rounded rectangle path
        var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
        using (var path = GetRoundedRectPath(rect, BorderRadius))
        {
            // Fill background
            using (var brush = new SolidBrush(this.BackColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (var pen = new Pen(BorderColor, 1))
            {
                g.DrawPath(pen, path);
            }
        }
    }

    private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        this.Invalidate();
    }
}
