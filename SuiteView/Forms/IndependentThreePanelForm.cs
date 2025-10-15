using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using SuiteView.Models;

namespace SuiteView.Forms;

/// <summary>
/// Configurable multi-panel form with independent splitters
/// Supports 1-4 panels with optional header and footer
/// Uses layered structure: Form Container → Interior Panel → Header/Splitters/Footer
/// </summary>
public class IndependentThreePanelForm : Form, IContentForm
{
    // Interior panel that holds everything
    private Panel _interiorPanel = null!;  // Initialized in constructor
    
    // Header and footer (optional)
    private Panel? _headerPanel;
    private Panel? _footerPanel;
    
    // Panel splitter container and its children
    private Panel _panelSplitterContainer = null!;  // Initialized in constructor
    private List<Panel> _contentPanels = new List<Panel>();
    private List<Panel> _splitters = new List<Panel>();
    
    private Form? _parentBorderForm;  // Reference to the BorderedWindowForm wrapper
    private Button? _closeButton;  // Close button in top right corner
    
    // Form dragging state
    private bool _isFormDragging = false;
    private Point _formDragStartPoint;
    private Point _formDragParentStart;
    
    private const int SplitterWidth = 8;
    private const int MinPanelWidth = 100;
    
    // Configurable spacing and heights (with defaults)
    private readonly int _headerHeight;
    private readonly int _footerHeight;
    private readonly int _topMargin;
    private readonly int _bottomMargin;
    
    // Configuration
    private readonly bool _showHeader;
    private readonly bool _showFooter;
    private readonly int _panelCount;
    private readonly string _headerText;
    private readonly string _footerText;
    private readonly ColorTheme _theme;
    
    // Dragging state
    private bool _isDragging = false;
    private int _dragStartX = 0;
    private int _draggingSplitterIndex = -1;
    private int _splitterStartX = 0;

    /// <summary>
    /// Creates a configurable multi-panel form
    /// </summary>
    /// <param name="panelCount">Number of panels (1-4)</param>
    /// <param name="showHeader">Whether to show header banner</param>
    /// <param name="showFooter">Whether to show footer banner</param>
    /// <param name="headerText">Text to display in header (optional)</param>
    /// <param name="footerText">Text to display in footer (optional)</param>
    /// <param name="theme">Color theme to use (optional)</param>
    /// <param name="headerHeight">Height of header (default: 30)</param>
    /// <param name="footerHeight">Height of footer (default: 30)</param>
    /// <param name="topMargin">Space between top of interior panel and panel splitter (default: 20)</param>
    /// <param name="bottomMargin">Space between bottom of panel splitter and interior panel (default: 20)</param>
    public IndependentThreePanelForm(
        int panelCount = 3, 
        bool showHeader = true, 
        bool showFooter = true,
        string? headerText = null,
        string? footerText = null,
        ColorTheme? theme = null,
        int headerHeight = 30,
        int footerHeight = 30,
        int topMargin = 20,
        int bottomMargin = 20)
    {
        // Validate and store configuration
        _panelCount = Math.Clamp(panelCount, 1, 4);
        _showHeader = showHeader;
        _showFooter = showFooter;
        _headerText = headerText ?? $"{_panelCount}-Panel Form";
        _footerText = footerText ?? "Status: Ready";
        _theme = theme ?? Managers.ThemeManager.GetTheme("Royal Classic");
        _headerHeight = headerHeight;
        _footerHeight = footerHeight;
        _topMargin = Math.Max(0, topMargin);
        _bottomMargin = Math.Max(0, bottomMargin);
        
        this.Text = $"Independent {_panelCount}-Panel Splitter";
        this.Width = 1000;
        this.Height = 600;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;  // Remove title bar
        this.BackColor = _theme.Primary;  // Form Container = Royal blue (PRIMARY = #2C5AA0)

        // Enable double buffering to reduce flicker
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                      ControlStyles.AllPaintingInWmPaint | 
                      ControlStyles.UserPaint | 
                      ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();

        InitializeLayeredStructure();
        LayoutPanels();
        
        this.Resize += OnFormResize;
        this.Paint += ContentForm_Paint;
        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;
    }

    /// <summary>
    /// Set the parent BorderedWindowForm (if using wrapped mode)
    /// </summary>
    public void SetParentBorderForm(Form parent)
    {
        _parentBorderForm = parent;
    }

    /// <summary>
    /// Initialize the layered structure: Form Container → Interior Panel → Header/PanelSplitter/Footer
    /// </summary>
    private void InitializeLayeredStructure()
    {
        // LAYER 1: Interior Panel (medium blue SECONDARY color #3a71c5)
        // Inset 30px from top and bottom to show Form Container (primary royal blue) color
        int formToInteriorPadding = 30;  // Space at top and bottom to show Form Container
        
        _interiorPanel = new Panel
        {
            BackColor = _theme.Secondary,  // Medium blue interior (SECONDARY = #3a71c5)
            Location = new Point(0, formToInteriorPadding),
            Size = new Size(this.ClientSize.Width, 
                           this.ClientSize.Height - (formToInteriorPadding * 2)),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        SetDoubleBuffered(_interiorPanel);
        _interiorPanel.Paint += InteriorPanel_Paint;  // Add paint event for bottom brass line
        _interiorPanel.Resize += InteriorPanel_Resize;  // Handle resize to reposition panel splitter
        this.Controls.Add(_interiorPanel);

        // Add close button in top right corner (centered in Layer 1 - Form Container header)
        _closeButton = new Button
        {
            Text = "",
            Size = new Size(24, 24),
            Location = new Point(this.ClientSize.Width - 34, (formToInteriorPadding / 2) - 12),  // Centered in Form Container layer (30px / 2 - 12px = 3px from top)
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _closeButton.FlatAppearance.BorderSize = 0;
        _closeButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _closeButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _closeButton.Click += CloseButton_Click;
        _closeButton.Paint += CloseButton_Paint;
        this.Controls.Add(_closeButton);
        _closeButton.BringToFront();

        // LAYER 2A: Header Panel (optional)
        if (_showHeader)
        {
            _headerPanel = new Panel
            {
                BackColor = _theme.Secondary,  // Medium blue header (matches interior)
                Dock = DockStyle.Top,
                Height = _headerHeight,
                Parent = _interiorPanel
            };
            
            var headerLabel = new Label
            {
                Text = _headerText,
                ForeColor = _theme.Accent,  // Brass/gold text
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(15, (_headerHeight - 20) / 2)  // Center vertically
            };
            _headerPanel.Controls.Add(headerLabel);
            SetDoubleBuffered(_headerPanel);
        }

        // LAYER 2B: Footer Panel (optional)
        if (_showFooter)
        {
            _footerPanel = new Panel
            {
                BackColor = _theme.Secondary,  // Medium blue footer (matches interior)
                Dock = DockStyle.Bottom,
                Height = _footerHeight,
                Parent = _interiorPanel
            };
            
            var footerLabel = new Label
            {
                Text = _footerText,
                ForeColor = _theme.Accent,  // Brass/gold text
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(15, (_footerHeight - 18) / 2)  // Center vertically
            };
            _footerPanel.Controls.Add(footerLabel);
            SetDoubleBuffered(_footerPanel);
        }

        // LAYER 2C: Panel Splitter Container (manually sized to reveal Interior Panel borders)
        // No Padding - we'll size and position it manually to reveal the border
        _panelSplitterContainer = new Panel
        {
            BackColor = _theme.LightBlue,  // Light blue background (same as panels)
            Dock = DockStyle.None,  // Manual positioning, not docked
            Parent = _interiorPanel
        };
        SetDoubleBuffered(_panelSplitterContainer);
        
        // Position and size the Panel Splitter Container to show borders
        PositionPanelSplitterContainer();

        // LAYER 3: Content panels and splitters (inside the panel splitter container)
        Color panelColor = _theme.LightBlue;  // Light blue panels
        string[] panelNames = { "First", "Second", "Third", "Fourth" };
        
        for (int i = 0; i < _panelCount; i++)
        {
            var panel = new Panel
            {
                BackColor = panelColor,
                Tag = i,  // Store panel index
                Parent = _panelSplitterContainer
            };
            
            var label = new Label
            {
                Text = $"{panelNames[i]} Panel\n({i + 1} of {_panelCount})",
                ForeColor = _theme.Primary,  // Blue text
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            panel.Controls.Add(label);
            
            SetDoubleBuffered(panel);
            _contentPanels.Add(panel);
            
            // Add splitter after each panel except the last
            if (i < _panelCount - 1)
            {
                var splitter = new Panel
                {
                    BackColor = _theme.Secondary,  // Splitters match interior panel (medium blue #3a71c5)
                    Width = SplitterWidth,
                    Cursor = Cursors.VSplit,
                    Tag = i,  // Store splitter index
                    Parent = _panelSplitterContainer
                };
                
                int splitterIndex = i;
                splitter.MouseDown += (s, e) => Splitter_MouseDown(s, e, splitterIndex);
                splitter.MouseMove += (s, e) => Splitter_MouseMove(s, e, splitterIndex);
                splitter.MouseUp += (s, e) => Splitter_MouseUp(s, e, splitterIndex);
                
                SetDoubleBuffered(splitter);
                _splitters.Add(splitter);
            }
        }
    }

    /// <summary>
    /// Position and size the Panel Splitter Container to reveal Interior Panel borders
    /// </summary>
    private void PositionPanelSplitterContainer()
    {
        if (_panelSplitterContainer == null || _interiorPanel == null) return;
        
        int borderWidth = 15;  // Border on all sides
        int topOffset = _showHeader ? _headerHeight : 0;
        int bottomOffset = _showFooter ? _footerHeight : 0;
        
        _panelSplitterContainer.Location = new Point(borderWidth, topOffset + borderWidth);
        _panelSplitterContainer.Size = new Size(
            _interiorPanel.ClientSize.Width - (borderWidth * 2),
            _interiorPanel.ClientSize.Height - topOffset - bottomOffset - (borderWidth * 2)
        );
    }
    
    /// <summary>
    /// Handle interior panel resize to reposition panel splitter container
    /// </summary>
    private void InteriorPanel_Resize(object? sender, EventArgs e)
    {
        PositionPanelSplitterContainer();
    }

    /// <summary>
    /// Layout the panels and splitters within the panel splitter container
    /// </summary>
    private void LayoutPanels()
    {
        if (_contentPanels.Count == 0) return;
        
        // Get the available space in the panel splitter container
        int containerWidth = _panelSplitterContainer.ClientSize.Width;
        int containerHeight = _panelSplitterContainer.ClientSize.Height;

        // Calculate usable width (excluding splitters)
        int totalSplitterWidth = (_panelCount - 1) * SplitterWidth;
        int usableWidth = containerWidth - totalSplitterWidth;
        
        // Divide width equally among panels initially
        int panelWidth = usableWidth / _panelCount;
        int currentX = 0;  // Start at left edge

        for (int i = 0; i < _panelCount; i++)
        {
            // Adjust last panel to fill any rounding remainder
            int width = (i == _panelCount - 1) ? (containerWidth - currentX) : panelWidth;
            
            _contentPanels[i].Location = new Point(currentX, 0);  // Start at top edge
            _contentPanels[i].Size = new Size(width, containerHeight);
            
            currentX += width;
            
            // Add splitter after panel (except last)
            if (i < _splitters.Count)
            {
                _splitters[i].Location = new Point(currentX, 0);  // Start at top edge
                _splitters[i].Size = new Size(SplitterWidth, containerHeight);
                currentX += SplitterWidth;
            }
        }
    }

    /// <summary>
    /// Handle form resize - maintain panel proportions
    /// </summary>
    private void OnFormResize(object? sender, EventArgs e)
    {
        if (_contentPanels.Count == 0) return;
        
        // Calculate current proportions before resize
        int totalPanelWidth = _contentPanels.Sum(p => p.Width);
        
        if (totalPanelWidth > 0)
        {
            // Calculate proportions for each panel
            double[] ratios = _contentPanels.Select(p => (double)p.Width / totalPanelWidth).ToArray();
            
            // Calculate new usable width in panel splitter container
            int containerWidth = _panelSplitterContainer.ClientSize.Width;  // No padding to account for
            int totalSplitterWidth = _splitters.Count * SplitterWidth;
            int usableNewWidth = containerWidth - totalSplitterWidth;
            
            // Apply proportions to new widths
            int[] newWidths = new int[_panelCount];
            int allocatedWidth = 0;
            
            for (int i = 0; i < _panelCount - 1; i++)
            {
                newWidths[i] = Math.Max(MinPanelWidth, (int)Math.Round(usableNewWidth * ratios[i]));
                allocatedWidth += newWidths[i];
            }
            // Last panel gets any rounding remainder
            newWidths[_panelCount - 1] = Math.Max(MinPanelWidth, usableNewWidth - allocatedWidth);
            
            // Update panel positions and sizes
            this.SuspendLayout();
            
            int currentX = 0;  // Start at left edge
            
            for (int i = 0; i < _panelCount; i++)
            {
                _contentPanels[i].Left = currentX;
                _contentPanels[i].Width = newWidths[i];
                currentX += newWidths[i];
                
                if (i < _splitters.Count)
                {
                    _splitters[i].Left = currentX;
                    currentX += SplitterWidth;
                }
            }
            
            this.ResumeLayout(false);
        }
        
        UpdatePanelHeights();
    }

    /// <summary>
    /// Update panel and splitter heights when form resizes
    /// </summary>
    private void UpdatePanelHeights()
    {
        if (_contentPanels.Count == 0) return;
        
        // No padding - panels fill entire container height
        int containerHeight = _panelSplitterContainer.ClientSize.Height;
        
        foreach (var panel in _contentPanels)
        {
            panel.Height = containerHeight;
        }
        
        foreach (var splitter in _splitters)
        {
            splitter.Height = containerHeight;
        }
    }
    
    #region Form Dragging
    
    /// <summary>
    /// Handle mouse down for form dragging
    /// </summary>
    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _parentBorderForm != null)
        {
            // Don't drag if clicking on close button
            Point clientPos = this.PointToClient(Cursor.Position);
            if (_closeButton != null && _closeButton.Bounds.Contains(clientPos))
            {
                return;
            }
            
            // Don't drag if clicking on a splitter
            foreach (var splitter in _splitters)
            {
                if (splitter.Bounds.Contains(_panelSplitterContainer.PointToClient(this.PointToClient(Cursor.Position))))
                {
                    return;
                }
            }

            _isFormDragging = true;
            _formDragStartPoint = Cursor.Position;
            _formDragParentStart = _parentBorderForm.Location;
        }
    }

    /// <summary>
    /// Handle mouse move for form dragging
    /// </summary>
    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isFormDragging && e.Button == MouseButtons.Left && _parentBorderForm != null)
        {
            // Calculate offset
            int deltaX = Cursor.Position.X - _formDragStartPoint.X;
            int deltaY = Cursor.Position.Y - _formDragStartPoint.Y;

            // Update parent form position
            _parentBorderForm.Location = new Point(_formDragParentStart.X + deltaX, _formDragParentStart.Y + deltaY);
        }
    }

    /// <summary>
    /// Handle mouse up for form dragging
    /// </summary>
    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isFormDragging)
        {
            _isFormDragging = false;
        }
    }
    
    #endregion

    // Generic splitter event handlers
    private void Splitter_MouseDown(object? sender, MouseEventArgs e, int splitterIndex)
    {
        if (e.Button == MouseButtons.Left && splitterIndex >= 0 && splitterIndex < _splitters.Count)
        {
            _isDragging = true;
            _draggingSplitterIndex = splitterIndex;
            _dragStartX = this.PointToClient(Cursor.Position).X;
            _splitterStartX = _splitters[splitterIndex].Left;
            _splitters[splitterIndex].Capture = true;
        }
    }

    private void Splitter_MouseMove(object? sender, MouseEventArgs e, int splitterIndex)
    {
        if (_isDragging && _draggingSplitterIndex == splitterIndex)
        {
            int currentX = this.PointToClient(Cursor.Position).X;
            int deltaX = currentX - _dragStartX;
            int newSplitterX = _splitterStartX + deltaX;

            // Calculate boundaries
            int minSplitterX = _contentPanels[splitterIndex].Left + MinPanelWidth;
            int maxSplitterX = (splitterIndex + 1 < _contentPanels.Count) 
                ? _contentPanels[splitterIndex + 1].Left + _contentPanels[splitterIndex + 1].Width - SplitterWidth - MinPanelWidth
                : this.ClientSize.Width - SplitterWidth - MinPanelWidth;

            if (newSplitterX >= minSplitterX && newSplitterX <= maxSplitterX)
            {
                this.SuspendLayout();

                // Resize panel on the left of the splitter
                int newLeftPanelWidth = newSplitterX - _contentPanels[splitterIndex].Left;
                _contentPanels[splitterIndex].Width = newLeftPanelWidth;

                // Move the splitter
                _splitters[splitterIndex].Left = newSplitterX;

                // Resize panel on the right of the splitter
                if (splitterIndex + 1 < _contentPanels.Count)
                {
                    int newRightPanelLeft = newSplitterX + SplitterWidth;
                    int nextPanelEnd = (splitterIndex + 2 < _contentPanels.Count)
                        ? _splitters[splitterIndex + 1].Left
                        : this.ClientSize.Width;
                    
                    _contentPanels[splitterIndex + 1].Left = newRightPanelLeft;
                    _contentPanels[splitterIndex + 1].Width = nextPanelEnd - newRightPanelLeft;
                }

                this.ResumeLayout(false);
            }
        }
    }

    private void Splitter_MouseUp(object? sender, MouseEventArgs e, int splitterIndex)
    {
        if (_draggingSplitterIndex == splitterIndex)
        {
            _isDragging = false;
            _draggingSplitterIndex = -1;
            if (splitterIndex >= 0 && splitterIndex < _splitters.Count)
            {
                _splitters[splitterIndex].Capture = false;
            }
        }
    }
    
    /// <summary>
    /// Enable double buffering on a control to reduce flicker
    /// </summary>
    private void SetDoubleBuffered(Control control)
    {
        typeof(Control).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic,
            null, control, new object[] { true });
    }

    /// <summary>
    /// Close button click handler
    /// </summary>
    private void CloseButton_Click(object? sender, EventArgs e)
    {
        // If wrapped in BorderedWindowForm, close the parent instead
        if (_parentBorderForm != null)
        {
            _parentBorderForm.Close();
        }
        else
        {
            this.Close();
        }
    }

    /// <summary>
    /// Paint the brass round close button with X
    /// </summary>
    private void CloseButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(2, 2, 20, 20);
        
        // Draw brass circle
        using (var brush = new SolidBrush(_theme.Accent))
        {
            g.FillEllipse(brush, rect);
        }

        // Draw X in royal blue
        using (var pen = new Pen(_theme.Primary, 2))
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int size = 6;
            g.DrawLine(pen, centerX - size, centerY - size, centerX + size, centerY + size);
            g.DrawLine(pen, centerX + size, centerY - size, centerX - size, centerY + size);
        }
    }

    /// <summary>
    /// Paint event for interior panel to draw bottom brass line
    /// </summary>
    private void InteriorPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw brass accent line at bottom of Interior Panel (where it meets Form Container)
        using (var pen = new Pen(_theme.Accent, 2))
        {
            g.DrawLine(pen, 0, _interiorPanel.Height - 1, _interiorPanel.Width, _interiorPanel.Height - 1);
        }
    }

    /// <summary>
    /// Paint event to draw gradient header/footer and brass accent lines on the interior panel
    /// </summary>
    private void ContentForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw gradient header if shown (directly on interior panel area)
        if (_showHeader && _headerPanel != null)
        {
            var titleRect = new Rectangle(0, 0, _interiorPanel.Width, _headerHeight);
            var lighterBlue = ControlPaint.Light(_theme.Primary, 0.2f);
            using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                titleRect,
                lighterBlue,
                _theme.Primary,
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(gradientBrush, titleRect);
            }

            // Draw brass accent line below header
            using (var pen = new Pen(_theme.Accent, 2))
            {
                g.DrawLine(pen, 0, _headerHeight - 1, _interiorPanel.Width, _headerHeight - 1);
            }
        }

        // Draw brass accent line at top of panel splitter (above panels)
        if (_panelSplitterContainer != null)
        {
            int panelTop = _showHeader ? _headerHeight : 0;
            panelTop += _topMargin;
            
            using (var pen = new Pen(_theme.Accent, 2))
            {
                g.DrawLine(pen, 0, panelTop - 1, _interiorPanel.Width, panelTop - 1);
            }
        }

        // Draw gradient footer if shown
        if (_showFooter && _footerPanel != null)
        {
            int footerTop = this.Height - _footerHeight;
            var footerRect = new Rectangle(0, footerTop, _interiorPanel.Width, _footerHeight);
            using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                footerRect,
                _theme.Primary,
                ControlPaint.Dark(_theme.Primary, 0.1f),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(gradientBrush, footerRect);
            }

            // Draw brass accent line above footer
            using (var pen = new Pen(_theme.Accent, 2))
            {
                g.DrawLine(pen, 0, footerTop, _interiorPanel.Width, footerTop);
            }
        }
    }
}
