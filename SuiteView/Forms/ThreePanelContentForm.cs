using SuiteView.Models;
using SuiteView.Managers;

namespace SuiteView.Forms;

/// <summary>
/// Template form with three adjustable panels - left, middle, and right
/// The dividers can be moved left or right using split containers
/// </summary>
public partial class ThreePanelContentForm : Form, IContentForm
{
    private readonly ColorTheme _currentTheme;
    private Form? _parentBorderForm;
    
    // UI Controls
    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Panel _contentPanel = null!;
    private Panel _headerPanel = null!;
    private Panel _footerPanel = null!;
    
    // Split containers for resizable panels
    private SplitContainer _mainSplitContainer = null!;  // Splits left from middle+right
    private SplitContainer _rightSplitContainer = null!; // Splits middle from right
    
    // The three main panels
    private Panel _leftPanel = null!;
    private Panel _middlePanel = null!;
    private Panel _rightPanel = null!;
    
    // Layout constants
    private const int TitleBarHeight = 35;
    private const int HeaderHeight = 40;
    private const int FooterHeight = 35;
    private const int ControlMargin = 10;
    
    // Dragging support
    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _dragParentStart;

    public ThreePanelContentForm()
    {
        _currentTheme = ThemeManager.GetTheme("Dark"); // Default theme
        
        // Enable double buffering
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();

        InitializeComponent();
        SetupForm();
        ApplyTheme();
    }

    public void SetParentBorderForm(Form parent)
    {
        _parentBorderForm = parent;
    }

    private void InitializeComponent()
    {
        // Form settings
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = _currentTheme.Secondary;
        this.DoubleBuffered = true;
        this.Size = new Size(1000, 600);

        // Close button
        _closeButton = new Button
        {
            Text = "",
            Size = new Size(24, 24),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _closeButton.FlatAppearance.BorderSize = 0;
        _closeButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _closeButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _closeButton.Click += CloseButton_Click;

        // Title label
        _titleLabel = new Label
        {
            Text = "Three Panel Template",
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location = new Point(12, 8),
            AutoSize = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.SizeAll
        };

        // Header panel (optional - for header content)
        _headerPanel = new Panel
        {
            Location = new Point(0, TitleBarHeight),
            Size = new Size(this.Width, HeaderHeight),
            BackColor = _currentTheme.Secondary
        };

        // Footer panel (optional - for footer content)
        _footerPanel = new Panel
        {
            BackColor = _currentTheme.Secondary
        };

        // Content panel (contains the split containers)
        _contentPanel = new Panel
        {
            BackColor = _currentTheme.Secondary
        };

        // Create the three panels
        _leftPanel = new Panel
        {
            BackColor = _currentTheme.LightBlue,
            Dock = DockStyle.Fill
        };
        
        _middlePanel = new Panel
        {
            BackColor = _currentTheme.Secondary,
            Dock = DockStyle.Fill
        };
        
        _rightPanel = new Panel
        {
            BackColor = _currentTheme.LightBlue,
            Dock = DockStyle.Fill
        };

        // Add sample labels to show which panel is which
        var leftLabel = new Label
        {
            Text = "Left Panel",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _leftPanel.Controls.Add(leftLabel);

        var middleLabel = new Label
        {
            Text = "Middle Panel",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _middlePanel.Controls.Add(middleLabel);

        var rightLabel = new Label
        {
            Text = "Right Panel",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _rightPanel.Controls.Add(rightLabel);

        // Create split containers - following the pattern that works
        // Middle and Right Split
        _rightSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 300,  // Set distance here, not in Load event
            SplitterWidth = 8,
            BackColor = _currentTheme.Accent,
            Panel1MinSize = 100,
            Panel2MinSize = 100,
            IsSplitterFixed = false
        };
        _rightSplitContainer.Panel1.Controls.Add(_middlePanel);
        _rightSplitContainer.Panel2.Controls.Add(_rightPanel);

        // Main split container (left | middle+right)
        _mainSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 300,  // Set distance here, not in Load event
            SplitterWidth = 8,
            BackColor = _currentTheme.Accent,
            Panel1MinSize = 100,
            Panel2MinSize = 200,
            IsSplitterFixed = false
        };
        _mainSplitContainer.Panel1.Controls.Add(_leftPanel);
        _mainSplitContainer.Panel2.Controls.Add(_rightSplitContainer);

        // Add split container to content panel
        _contentPanel.Controls.Add(_mainSplitContainer);

        // Add all panels to form
        this.Controls.Add(_contentPanel);
        this.Controls.Add(_headerPanel);
        this.Controls.Add(_footerPanel);
        this.Controls.Add(_titleLabel);
        this.Controls.Add(_closeButton);

        // Make title bar draggable
        _titleLabel.MouseDown += Form_MouseDown;
        _titleLabel.MouseMove += Form_MouseMove;
        _titleLabel.MouseUp += Form_MouseUp;
        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;

        this.Paint += ContentForm_Paint;
        this.Resize += ThreePanelContentForm_Resize;
    }

    private void SetupForm()
    {
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        _closeButton.Location = new Point(this.Width - _closeButton.Width - 6, 4);
        
        _headerPanel.Location = new Point(0, TitleBarHeight);
        _headerPanel.Size = new Size(this.Width, HeaderHeight);
        
        _footerPanel.Location = new Point(0, this.Height - FooterHeight);
        _footerPanel.Size = new Size(this.Width, FooterHeight);
        
        _contentPanel.Location = new Point(0, TitleBarHeight + HeaderHeight);
        _contentPanel.Size = new Size(this.Width, this.Height - TitleBarHeight - HeaderHeight - FooterHeight);
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;
        _contentPanel.BackColor = _currentTheme.Secondary;
        _headerPanel.BackColor = _currentTheme.Secondary;
        _footerPanel.BackColor = _currentTheme.Secondary;
        _leftPanel.BackColor = _currentTheme.LightBlue;
        _middlePanel.BackColor = _currentTheme.Secondary;
        _rightPanel.BackColor = _currentTheme.LightBlue;
        _mainSplitContainer.BackColor = _currentTheme.Accent;
        _rightSplitContainer.BackColor = _currentTheme.Accent;
    }

    private void ThreePanelContentForm_Resize(object? sender, EventArgs e)
    {
        UpdateLayout();
        this.Invalidate();
    }

    private void ContentForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw title bar gradient (blue gradient)
        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Rectangle(0, 0, this.Width, TitleBarHeight),
            _currentTheme.Primary,
            _currentTheme.LightBlue,
            90f))
        {
            g.FillRectangle(brush, 0, 0, this.Width, TitleBarHeight);
        }

        // Draw close button as red circle with X
        DrawCloseButton(g);

        // Draw bottom border line
        using (var pen = new Pen(_currentTheme.Accent, 1))
        {
            g.DrawLine(pen, 0, this.Height - 1, this.Width, this.Height - 1);
        }
    }

    private void DrawCloseButton(Graphics g)
    {
        var rect = _closeButton.Bounds;
        var isHovered = rect.Contains(this.PointToClient(Cursor.Position));

        using (var brush = new SolidBrush(isHovered ? Color.FromArgb(200, 50, 50) : Color.FromArgb(180, 50, 50)))
        {
            g.FillEllipse(brush, rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
        }

        using (var pen = new Pen(Color.White, 2))
        {
            int padding = 7;
            g.DrawLine(pen,
                rect.X + padding, rect.Y + padding,
                rect.X + rect.Width - padding, rect.Y + rect.Height - padding);
            g.DrawLine(pen,
                rect.X + rect.Width - padding, rect.Y + padding,
                rect.X + padding, rect.Y + rect.Height - padding);
        }
    }

    // Dragging implementation
    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && e.Y < TitleBarHeight)
        {
            _isDragging = true;
            _dragStartPoint = e.Location;
            if (_parentBorderForm != null)
            {
                _dragParentStart = _parentBorderForm.Location;
            }
        }
    }

    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging && _parentBorderForm != null)
        {
            Point diff = new Point(e.Location.X - _dragStartPoint.X, e.Location.Y - _dragStartPoint.Y);
            _parentBorderForm.Location = new Point(_dragParentStart.X + diff.X, _dragParentStart.Y + diff.Y);
        }
    }

    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        _parentBorderForm?.Close();
    }
}
