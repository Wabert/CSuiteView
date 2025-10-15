using System;
using System.Windows.Forms;
using System.Drawing;
using SuiteView.Models;

namespace SuiteView.Forms.LayeredForms;

/// <summary>
/// Two-layer form: Layer 1 (Form Container with header/footer) contains content area
/// Similar to MainForm structure - simple border with content area
/// </summary>
public class TwoLayerForm : LayeredFormBase
{
    private Panel? _contentPanel;
    
    public TwoLayerForm(
        int formWidth = 1000,
        int formHeight = 600,
        int layer1HeaderHeight = 30,
        int layer1FooterHeight = 30,
        ColorTheme? theme = null)
        : base(formWidth, formHeight, layer1HeaderHeight, layer1FooterHeight, theme)
    {
        InitializeLayeredStructure();
        InitializeCloseButton();
        
        // Add paint event to draw brass lines where Layer 1 meets content panel
        this.Paint += TwoLayerForm_Paint;
    }
    
    private void InitializeLayeredStructure()
    {
        // Create content panel (fills area between Layer 1 header/footer)
        // Uses Secondary color by default to match the layered form pattern
        _contentPanel = new Panel
        {
            BackColor = _theme.Secondary,
            Location = new Point(0, _layer1HeaderHeight),
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - _layer1HeaderHeight - _layer1FooterHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        SetDoubleBuffered(_contentPanel);
        this.Controls.Add(_contentPanel);
    }
    
    /// <summary>
    /// Paint brass accent lines where Layer 1 (Form Container) meets the content panel
    /// </summary>
    private void TwoLayerForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Draw brass line at bottom of Layer 1 header (top of content panel)
        int topLineY = _layer1HeaderHeight - 1;
        using (var pen = new Pen(_theme.Accent, 2))
        {
            g.DrawLine(pen, 0, topLineY, this.Width, topLineY);
        }
        
        // Draw brass line at top of Layer 1 footer (bottom of content panel)
        int bottomLineY = this.Height - _layer1FooterHeight;
        using (var pen = new Pen(_theme.Accent, 2))
        {
            g.DrawLine(pen, 0, bottomLineY, this.Width, bottomLineY);
        }
    }
    
    /// <summary>
    /// Get the content panel where child controls should be added
    /// </summary>
    public Panel ContentPanel => _contentPanel ?? throw new InvalidOperationException("Content panel not initialized");
    
    /// <summary>
    /// Set custom content for this form
    /// </summary>
    public void SetContent(Control content)
    {
        if (_contentPanel == null) return;
        
        _contentPanel.Controls.Clear();
        content.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(content);
    }
}
