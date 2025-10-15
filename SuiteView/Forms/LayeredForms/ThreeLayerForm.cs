using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using SuiteView.Models;

namespace SuiteView.Forms.LayeredForms;

/// <summary>
/// Three-layer form with resizable panels:
/// - Layer 1: Form Container (PRIMARY color) with header/footer borders
/// - Layer 2: Interior Panel (SECONDARY color) with borders on all sides
/// - Layer 3: Panel Splitter Container with 1-4 resizable panels
/// </summary>
public class ThreeLayerForm : LayeredFormBase
{
    private Panel _layer2Panel = null!;
    private Panel _layer3PanelContainer = null!;
    
    // Panel splitter configuration
    private readonly int _panelCount;
    private readonly List<Panel> _contentPanels = new();
    private readonly List<SimpleSplitter> _splitters = new();
    
    // Layer 2 dimensions
    private readonly int _layer2HeaderHeight;
    private readonly int _layer2FooterHeight;
    private readonly int _layer2BorderWidth;
    
    // Splitter dragging state
    private bool _isDragging = false;
    private int _dragStartX = 0;
    private int _draggingSplitterIndex = -1;
    private int _splitterStartX = 0;
    private const int MinPanelWidth = 100;
    
    public ThreeLayerForm(
        int formWidth = 1000,
        int formHeight = 600,
        int layer1HeaderHeight = 30,
        int layer1FooterHeight = 30,
        int layer2HeaderHeight = 0,
        int layer2FooterHeight = 0,
        int layer2BorderWidth = 15,
        int panelCount = 3,
        ColorTheme? theme = null)
        : base(formWidth, formHeight, layer1HeaderHeight, layer1FooterHeight, theme)
    {
        if (panelCount < 1 || panelCount > 4)
            throw new ArgumentException("Panel count must be between 1 and 4", nameof(panelCount));
        
        _panelCount = panelCount;
        _layer2HeaderHeight = layer2HeaderHeight;
        _layer2FooterHeight = layer2FooterHeight;
        _layer2BorderWidth = layer2BorderWidth;
        
        InitializeLayeredStructure();
        InitializeCloseButton();
        InitializePanels();
    }
    
    private void InitializeLayeredStructure()
    {
        // LAYER 2: Interior Panel (inset from Layer 1 by header/footer heights)
        _layer2Panel = new Panel
        {
            BackColor = _theme.Secondary,
            Location = new Point(0, _layer1HeaderHeight),
            Size = new Size(this.ClientSize.Width, this.ClientSize.Height - _layer1HeaderHeight - _layer1FooterHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        SetDoubleBuffered(_layer2Panel);
        _layer2Panel.Resize += Layer2Panel_Resize;
        this.Controls.Add(_layer2Panel);
        
        // Add paint event to draw brass lines where Layer 1 meets Layer 2
        this.Paint += ThreeLayerForm_Paint;
        
        // LAYER 3: Panel Splitter Container (manually positioned within Layer 2)
        _layer3PanelContainer = new Panel
        {
            BackColor = _theme.LightBlue,
            Dock = DockStyle.None,
            Parent = _layer2Panel
        };
        SetDoubleBuffered(_layer3PanelContainer);
        PositionLayer3Container();
    }
    
    private void PositionLayer3Container()
    {
        // Calculate position to reveal Layer 2 borders
        int topOffset = _layer2HeaderHeight;
        int bottomOffset = _layer2FooterHeight;
        
        _layer3PanelContainer.Location = new Point(_layer2BorderWidth, topOffset + _layer2BorderWidth);
        _layer3PanelContainer.Size = new Size(
            _layer2Panel.ClientSize.Width - (_layer2BorderWidth * 2),
            _layer2Panel.ClientSize.Height - topOffset - bottomOffset - (_layer2BorderWidth * 2)
        );
    }
    
    private void Layer2Panel_Resize(object? sender, EventArgs e)
    {
        PositionLayer3Container();
    }
    
    /// <summary>
    /// Paint brass accent lines where Layer 1 (Form Container) meets Layer 2 (Interior Panel)
    /// </summary>
    private void ThreeLayerForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Draw brass line at bottom of Layer 1 header (top of Layer 2)
        int topLineY = _layer1HeaderHeight - 1;
        using (var pen = new Pen(_theme.Accent, 2))
        {
            g.DrawLine(pen, 0, topLineY, this.Width, topLineY);
        }
        
        // Draw brass line at top of Layer 1 footer (bottom of Layer 2)
        int bottomLineY = this.Height - _layer1FooterHeight;
        using (var pen = new Pen(_theme.Accent, 2))
        {
            g.DrawLine(pen, 0, bottomLineY, this.Width, bottomLineY);
        }
    }
    
    private void InitializePanels()
    {
        // Create content panels
        for (int i = 0; i < _panelCount; i++)
        {
            var panel = new Panel
            {
                BackColor = _theme.LightBlue,
                Parent = _layer3PanelContainer
            };
            SetDoubleBuffered(panel);
            panel.Resize += (s, e) => { };
            _contentPanels.Add(panel);
        }
        
        // Create splitters (one less than panel count)
        for (int i = 0; i < _panelCount - 1; i++)
        {
            var splitter = new SimpleSplitter
            {
                BackColor = _theme.Secondary,
                Width = 4,
                Cursor = Cursors.SizeWE,
                Parent = _layer3PanelContainer
            };
            
            int splitterIndex = i;
            splitter.MouseDown += (s, e) => Splitter_MouseDown(s, e, splitterIndex);
            splitter.MouseMove += (s, e) => Splitter_MouseMove(s, e, splitterIndex);
            splitter.MouseUp += (s, e) => Splitter_MouseUp(s, e, splitterIndex);
            
            _splitters.Add(splitter);
        }
        
        // Initial layout
        _layer3PanelContainer.Resize += (s, e) => OnContainerResize();
        LayoutPanels();
    }
    
    private void LayoutPanels()
    {
        if (_contentPanels.Count == 0) return;
        
        // Get the available space in the panel splitter container
        int containerWidth = _layer3PanelContainer.ClientSize.Width;
        int containerHeight = _layer3PanelContainer.ClientSize.Height;

        // Calculate usable width (excluding splitters)
        int splitterWidth = 4;
        int totalSplitterWidth = (_panelCount - 1) * splitterWidth;
        int usableWidth = containerWidth - totalSplitterWidth;
        
        // Divide width equally among panels initially
        int panelWidth = usableWidth / _panelCount;
        int currentX = 0;

        for (int i = 0; i < _panelCount; i++)
        {
            // Adjust last panel to fill any rounding remainder
            int width = (i == _panelCount - 1) ? (containerWidth - currentX) : panelWidth;
            
            _contentPanels[i].Location = new Point(currentX, 0);
            _contentPanels[i].Size = new Size(width, containerHeight);
            
            currentX += width;
            
            // Add splitter after panel (except last)
            if (i < _splitters.Count)
            {
                _splitters[i].Location = new Point(currentX, 0);
                _splitters[i].Size = new Size(splitterWidth, containerHeight);
                _splitters[i].BringToFront();
                currentX += splitterWidth;
            }
        }
    }
    
    private void OnContainerResize()
    {
        if (_contentPanels.Count == 0) return;
        
        // Calculate current proportions before resize
        int totalPanelWidth = 0;
        foreach (var panel in _contentPanels)
        {
            totalPanelWidth += panel.Width;
        }
        
        if (totalPanelWidth > 0)
        {
            // Calculate proportions for each panel
            double[] ratios = new double[_panelCount];
            for (int i = 0; i < _panelCount; i++)
            {
                ratios[i] = (double)_contentPanels[i].Width / totalPanelWidth;
            }
            
            // Calculate new usable width in panel splitter container
            int containerWidth = _layer3PanelContainer.ClientSize.Width;
            int splitterWidth = 4;
            int totalSplitterWidth = _splitters.Count * splitterWidth;
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
            int currentX = 0;
            
            for (int i = 0; i < _panelCount; i++)
            {
                _contentPanels[i].Left = currentX;
                _contentPanels[i].Width = newWidths[i];
                currentX += newWidths[i];
                
                if (i < _splitters.Count)
                {
                    _splitters[i].Left = currentX;
                    currentX += splitterWidth;
                }
            }
        }
        
        UpdatePanelHeights();
    }
    
    private void UpdatePanelHeights()
    {
        if (_contentPanels.Count == 0) return;
        
        int containerHeight = _layer3PanelContainer.ClientSize.Height;
        
        foreach (var panel in _contentPanels)
        {
            panel.Height = containerHeight;
        }
        
        foreach (var splitter in _splitters)
        {
            splitter.Height = containerHeight;
        }
    }
    
    private void Splitter_MouseDown(object? sender, MouseEventArgs e, int splitterIndex)
    {
        if (e.Button == MouseButtons.Left && splitterIndex >= 0 && splitterIndex < _splitters.Count)
        {
            _isDragging = true;
            _draggingSplitterIndex = splitterIndex;
            _dragStartX = _layer3PanelContainer.PointToClient(Cursor.Position).X;
            _splitterStartX = _splitters[splitterIndex].Left;
            _splitters[splitterIndex].Capture = true;
        }
    }

    private void Splitter_MouseMove(object? sender, MouseEventArgs e, int splitterIndex)
    {
        if (_isDragging && _draggingSplitterIndex == splitterIndex)
        {
            int currentX = _layer3PanelContainer.PointToClient(Cursor.Position).X;
            int deltaX = currentX - _dragStartX;
            int newSplitterX = _splitterStartX + deltaX;
            int splitterWidth = 4;

            // Calculate boundaries
            int minSplitterX = _contentPanels[splitterIndex].Left + MinPanelWidth;
            int maxSplitterX = (splitterIndex + 1 < _contentPanels.Count) 
                ? _contentPanels[splitterIndex + 1].Left + _contentPanels[splitterIndex + 1].Width - splitterWidth - MinPanelWidth
                : _layer3PanelContainer.ClientSize.Width - splitterWidth - MinPanelWidth;

            if (newSplitterX >= minSplitterX && newSplitterX <= maxSplitterX)
            {
                // Resize panel on the left of the splitter
                int newLeftPanelWidth = newSplitterX - _contentPanels[splitterIndex].Left;
                _contentPanels[splitterIndex].Width = newLeftPanelWidth;

                // Move the splitter
                _splitters[splitterIndex].Left = newSplitterX;

                // Resize panel on the right of the splitter
                if (splitterIndex + 1 < _contentPanels.Count)
                {
                    int newRightPanelLeft = newSplitterX + splitterWidth;
                    int nextPanelEnd = (splitterIndex + 2 < _contentPanels.Count)
                        ? _splitters[splitterIndex + 1].Left
                        : _layer3PanelContainer.ClientSize.Width;
                    
                    _contentPanels[splitterIndex + 1].Left = newRightPanelLeft;
                    _contentPanels[splitterIndex + 1].Width = nextPanelEnd - newRightPanelLeft;
                }
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
    
    protected override bool ShouldPreventDragging(Point clientPos)
    {
        // Don't drag if clicking on a splitter
        Point transformedPos = _layer3PanelContainer.PointToClient(this.PointToScreen(clientPos));
        foreach (var splitter in _splitters)
        {
            if (splitter.Bounds.Contains(transformedPos))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Get a specific content panel by index (0-based)
    /// </summary>
    public Panel GetPanel(int index)
    {
        if (index < 0 || index >= _panelCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"Panel index must be between 0 and {_panelCount - 1}");
        return _contentPanels[index];
    }
    
    /// <summary>
    /// Get all content panels
    /// </summary>
    public IReadOnlyList<Panel> GetAllPanels() => _contentPanels.AsReadOnly();
}
