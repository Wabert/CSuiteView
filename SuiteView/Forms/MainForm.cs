using SuiteView.Managers;
using SuiteView.Models;
using SuiteView.Forms.LayeredForms;

namespace SuiteView.Forms;

/// <summary>
/// Content form - displays title bar and content
/// This is a child form inside ResizableBorderForm
/// </summary>
public partial class MainForm : Form
{
    private readonly ConfigManager _configManager;
    private ColorTheme _currentTheme;
    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _dragParentStart;
    private const int TitleBarHeight = 35;
    private Panel _contentPanel = null!;
    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Button _snapButton = null!;
    private Button _scanDirButton = null!;
    private Button _dbLibraryButton = null!;
    private Button _threePanelButton = null!;
    private Button _formBuilderButton = null!;
    private Form? _parentBorderForm;
    private WordDocumentManager? _wordDocManager;

    public MainForm(ConfigManager configManager)
    {
        _configManager = configManager;
        _currentTheme = ThemeManager.GetTheme(_configManager.Settings.SelectedTheme);

        // Enable double buffering to reduce flicker
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

        // Close button (transparent - will be drawn as circle in Paint)
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

        // Title label (on blue title bar, brass color)
        _titleLabel = new Label
        {
            Text = "SuiteView",
            ForeColor = _currentTheme.Accent, // Brass/gold color
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location = new Point(12, 8),
            AutoSize = true,
            BackColor = Color.Transparent, // Transparent to show gradient
            Cursor = Cursors.SizeAll,
            UseMnemonic = false
        };

        // Need to set parent to form for transparency to work
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

        // Content panel
        _contentPanel = new Panel
        {
            Location = new Point(0, TitleBarHeight),
            AutoScroll = true,
            BackColor = _currentTheme.Secondary
        };
        _contentPanel.Paint += ContentPanel_Paint;

        // Snap button (transparent - will be drawn as round in Paint)
        _snapButton = new Button
        {
            Text = "",
            Size = new Size(80, 32),
            Location = new Point(20, 20), // Will be adjusted in UpdateLayout
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _snapButton.FlatAppearance.BorderSize = 0;
        _snapButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _snapButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _snapButton.Click += SnapButton_Click;

        // Scan Directory button (transparent - will be drawn as round in Paint)
        _scanDirButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 65), // Will be adjusted in UpdateLayout
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _scanDirButton.FlatAppearance.BorderSize = 0;
        _scanDirButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _scanDirButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _scanDirButton.Click += ScanDirButton_Click;

        // DB Library button (transparent - will be drawn as round in Paint)
        _dbLibraryButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 110), // Will be adjusted in UpdateLayout
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _dbLibraryButton.FlatAppearance.BorderSize = 0;
        _dbLibraryButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _dbLibraryButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _dbLibraryButton.Click += DbLibraryButton_Click;

        // Three Panel button (transparent - will be drawn as round in Paint)
        _threePanelButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 155), // Will be adjusted in UpdateLayout
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _threePanelButton.FlatAppearance.BorderSize = 0;
        _threePanelButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _threePanelButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _threePanelButton.Click += ThreePanelButton_Click;

        // Form Builder button (transparent - will be drawn as round in Paint)
        _formBuilderButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 200), // Will be adjusted in UpdateLayout
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _formBuilderButton.FlatAppearance.BorderSize = 0;
        _formBuilderButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _formBuilderButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _formBuilderButton.Click += FormBuilderButton_Click;

        _contentPanel.Controls.Add(_snapButton);
        _contentPanel.Controls.Add(_scanDirButton);
        _contentPanel.Controls.Add(_dbLibraryButton);
        _contentPanel.Controls.Add(_threePanelButton);
        _contentPanel.Controls.Add(_formBuilderButton);

        this.Controls.Add(_contentPanel);
        this.Controls.Add(_titleLabel);
        this.Controls.Add(_closeButton);

        // Make title bar draggable
        _titleLabel.MouseDown += Form_MouseDown;
        _titleLabel.MouseMove += Form_MouseMove;
        _titleLabel.MouseUp += Form_MouseUp;

        // Make entire form background draggable (not content panel)
        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;

        // Paint event for custom rendering
        this.Paint += MainForm_Paint;
    }

    private void SetupForm()
    {
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        // Position close button
        _closeButton.Location = new Point(this.Width - _closeButton.Width - 6, 4);

        // Update content panel size
        _contentPanel.Size = new Size(this.Width, this.Height - TitleBarHeight);

        // Position snap button in content panel
        _snapButton.Location = new Point(20, 20);
        
        // Position scan directory button below snap button
        _scanDirButton.Location = new Point(20, 65);
        
        // Position DB library button below scan directory button
        _dbLibraryButton.Location = new Point(20, 110);
        
        // Position three panel button below DB library button
        _threePanelButton.Location = new Point(20, 155);
        
        // Position form builder button below three panel button
        _formBuilderButton.Location = new Point(20, 200);
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;
        _titleLabel.ForeColor = _currentTheme.Accent; // Brass on blue title bar
        _titleLabel.BackColor = Color.Transparent; // Transparent to show gradient
        _contentPanel.BackColor = _currentTheme.Secondary; // Walnut

        this.Invalidate();
    }

    public void UpdateTheme(string themeName)
    {
        _currentTheme = ThemeManager.GetTheme(themeName);
        ApplyTheme();
    }

    #region Dragging Logic (moves parent form)

    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && sender != _closeButton && _parentBorderForm != null)
        {
            // Don't drag if clicking on close button
            Point clientPos = this.PointToClient(Cursor.Position);
            if (_closeButton.Bounds.Contains(clientPos))
            {
                return;
            }

            _isDragging = true;
            _dragStartPoint = Cursor.Position;
            _dragParentStart = _parentBorderForm.Location;
        }
    }

    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging && e.Button == MouseButtons.Left && _parentBorderForm != null)
        {
            // Calculate offset
            int deltaX = Cursor.Position.X - _dragStartPoint.X;
            int deltaY = Cursor.Position.Y - _dragStartPoint.Y;

            // Update parent form position
            _parentBorderForm.Location = new Point(_dragParentStart.X + deltaX, _dragParentStart.Y + deltaY);
        }
    }

    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
        }
    }

    #endregion

    #region Custom Painting

    private void MainForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw gradient title bar background (lighter blue to darker blue)
        var titleRect = new Rectangle(0, 0, this.Width, TitleBarHeight);
        var lighterBlue = ControlPaint.Light(_currentTheme.Primary, 0.2f);
        using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            titleRect,
            lighterBlue,           // Start color (lighter at top)
            _currentTheme.Primary, // End color (darker at bottom)
            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
        {
            g.FillRectangle(gradientBrush, titleRect);
        }

        // Draw walnut content area background
        using (var brush = new SolidBrush(_currentTheme.Secondary))
        {
            g.FillRectangle(brush, 0, TitleBarHeight, this.Width, this.Height - TitleBarHeight);
        }

        // Draw brass accent line below title bar
        using (var pen = new Pen(_currentTheme.Accent, 2))
        {
            g.DrawLine(pen, 0, TitleBarHeight - 1, this.Width, TitleBarHeight - 1);
        }

        // Draw subtle inner border around content area
        using (var pen = new Pen(Color.FromArgb(60, _currentTheme.Accent.R, _currentTheme.Accent.G, _currentTheme.Accent.B), 1))
        {
            g.DrawRectangle(pen, 2, TitleBarHeight + 2, this.Width - 5, this.Height - TitleBarHeight - 5);
        }

        // Draw round close button
        DrawRoundCloseButton(g);
    }

    private void ContentPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw rounded snap button
        DrawRoundedSnapButton(g);
        
        // Draw rounded scan directory button
        DrawRoundedScanDirButton(g);
        
        // Draw rounded DB library button
        DrawRoundedDbLibraryButton(g);
        
        // Draw rounded three panel button
        DrawRoundedThreePanelButton(g);
        
        // Draw rounded form builder button
        DrawRoundedFormBuilderButton(g);
    }

    private void DrawRoundCloseButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var buttonRect = _closeButton.Bounds;

        // Draw brass circle
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillEllipse(brush, buttonRect);
        }

        // Draw X in the center
        using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "✕";
            var size = g.MeasureString(text, font);
            var x = buttonRect.X + (buttonRect.Width - size.Width) / 2;
            var y = buttonRect.Y + (buttonRect.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }

    private void DrawRoundedSnapButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Button position in content panel coordinates
        var buttonRect = _snapButton.Bounds;

        // Draw rounded brass background
        const int cornerRadius = 16;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddArc(buttonRect.X, buttonRect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(buttonRect.X, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }

        // Draw "Snap" text in blue
        using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "Snap";
            var size = g.MeasureString(text, font);
            var x = buttonRect.X + (buttonRect.Width - size.Width) / 2;
            var y = buttonRect.Y + (buttonRect.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }

    private void DrawRoundedScanDirButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Button position in content panel coordinates
        var buttonRect = _scanDirButton.Bounds;

        // Draw rounded brass background
        const int cornerRadius = 16;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddArc(buttonRect.X, buttonRect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(buttonRect.X, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }

        // Draw "Scan Directory" text in blue
        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "Scan Directory";
            var size = g.MeasureString(text, font);
            var x = buttonRect.X + (buttonRect.Width - size.Width) / 2;
            var y = buttonRect.Y + (buttonRect.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }

    private void DrawRoundedDbLibraryButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Button position in content panel coordinates
        var buttonRect = _dbLibraryButton.Bounds;

        // Draw rounded brass background
        const int cornerRadius = 16;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddArc(buttonRect.X, buttonRect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(buttonRect.X, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }

        // Draw "DB Library" text in blue
        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "DB Library";
            var size = g.MeasureString(text, font);
            var x = buttonRect.X + (buttonRect.Width - size.Width) / 2;
            var y = buttonRect.Y + (buttonRect.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }

    private void DrawRoundedThreePanelButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Button position in content panel coordinates
        var buttonRect = _threePanelButton.Bounds;

        // Draw rounded brass background
        const int cornerRadius = 16;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddArc(buttonRect.X, buttonRect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(buttonRect.X, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }

        // Draw "3Panel Form" text in blue
        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "3Panel Form";
            var size = g.MeasureString(text, font);
            var x = buttonRect.X + (buttonRect.Width - size.Width) / 2;
            var y = buttonRect.Y + (buttonRect.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }

    private void DrawRoundedFormBuilderButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Button position in content panel coordinates
        var buttonRect = _formBuilderButton.Bounds;

        // Draw rounded brass background
        const int cornerRadius = 16;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddArc(buttonRect.X, buttonRect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(buttonRect.Right - cornerRadius, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(buttonRect.X, buttonRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }

        // Draw "Form Builder" text in blue
        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "Form Builder";
            var size = g.MeasureString(text, font);
            var x = buttonRect.X + (buttonRect.Width - size.Width) / 2;
            var y = buttonRect.Y + (buttonRect.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }

    #endregion

    #region Button Events

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        if (_parentBorderForm != null)
        {
            _parentBorderForm.Hide();
        }
        _configManager.UpdateSettings(settings => settings.IsVisible = false);
    }

    private async void SnapButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Step 1: Minimize SuiteView immediately
            if (_parentBorderForm != null)
            {
                _parentBorderForm.WindowState = FormWindowState.Minimized;
            }

            // Step 2: Wait briefly for window to minimize
            await Task.Delay(200);

            // Step 3: Capture screenshot immediately
            var screenshot = ScreenshotManager.CaptureScreen();
            var tempPath = ScreenshotManager.SaveToTempFile(screenshot);
            screenshot.Dispose();

            // Step 4: Restore SuiteView
            if (_parentBorderForm != null)
            {
                _parentBorderForm.WindowState = FormWindowState.Normal;
            }

            // Step 5: Initialize WordDocumentManager if needed (AFTER screenshot)
            if (_wordDocManager == null)
            {
                _wordDocManager = new WordDocumentManager();
            }

            // Step 6: Check if document is still open, if not create new one
            if (!_wordDocManager.CheckDocumentOpen())
            {
                _wordDocManager.EnsureDocumentOpen();
            }

            // Step 7: Add screenshot to Word document
            _wordDocManager.AddScreenshot(tempPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error taking screenshot: {ex.Message}",
                "Screenshot Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ScanDirButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Create the Directory Scanner with the iconic bordered window look
            var contentForm = new DirectoryScannerContentForm(_currentTheme);
            var borderedWindow = new BorderedWindowForm(
                _currentTheme,
                initialSize: new Size(1000, 600),
                minimumSize: new Size(800, 400)
            );
            borderedWindow.Text = "Directory Scanner";
            borderedWindow.SetContentForm(contentForm);
            borderedWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening Directory Scanner: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void DbLibraryButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Create the Database Library Manager with the iconic bordered window look
            var contentForm = new DatabaseLibraryManagerContentForm(_currentTheme);
            var borderedWindow = new BorderedWindowForm(
                _currentTheme,
                initialSize: new Size(1200, 700),
                minimumSize: new Size(900, 500)
            );
            borderedWindow.Text = "Database Library Manager";
            borderedWindow.SetContentForm(contentForm);
            borderedWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening Directory Scanner: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ThreePanelButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Show a menu to select different panel configurations
            var menu = new ContextMenuStrip();
            
            // 1 Panel options
            menu.Items.Add("1 Panel (Header + Footer)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1000,
                    formHeight: 600,
                    layer2HeaderHeight: 30,
                    layer2FooterHeight: 30,
                    panelCount: 1,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "1-Panel Form";
                borderedWindow.Show();
            });
            menu.Items.Add("1 Panel (No Header/Footer)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1000,
                    formHeight: 600,
                    panelCount: 1,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "1-Panel Form";
                borderedWindow.Show();
            });
            
            menu.Items.Add(new ToolStripSeparator());
            
            // 2 Panel options
            menu.Items.Add("2 Panels (Header + Footer)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1000,
                    formHeight: 600,
                    layer2HeaderHeight: 30,
                    layer2FooterHeight: 30,
                    panelCount: 2,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "2-Panel Form";
                borderedWindow.Show();
            });
            menu.Items.Add("2 Panels (Header Only)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1000,
                    formHeight: 600,
                    layer2HeaderHeight: 30,
                    panelCount: 2,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "2-Panel Form";
                borderedWindow.Show();
            });
            
            menu.Items.Add(new ToolStripSeparator());
            
            // 3 Panel options (default)
            menu.Items.Add("3 Panels (Header + Footer) ⭐", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1200,
                    formHeight: 700,
                    layer2HeaderHeight: 30,
                    layer2FooterHeight: 30,
                    panelCount: 3,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "3-Panel Form";
                borderedWindow.Show();
            });
            menu.Items.Add("3 Panels (Footer Only)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1200,
                    formHeight: 700,
                    layer2FooterHeight: 30,
                    panelCount: 3,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "3-Panel Form";
                borderedWindow.Show();
            });
            menu.Items.Add("3 Panels (No Header/Footer)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1200,
                    formHeight: 700,
                    panelCount: 3,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "3-Panel Form";
                borderedWindow.Show();
            });
            
            menu.Items.Add(new ToolStripSeparator());
            
            // 4 Panel options
            menu.Items.Add("4 Panels (Header + Footer)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1400,
                    formHeight: 800,
                    layer2HeaderHeight: 30,
                    layer2FooterHeight: 30,
                    panelCount: 4,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "4-Panel Form";
                borderedWindow.Show();
            });
            menu.Items.Add("4 Panels (No Header/Footer)", null, (s, args) => 
            {
                var borderedWindow = LayeredFormFactory.CreateThreeLayerForm(
                    formWidth: 1400,
                    formHeight: 800,
                    panelCount: 4,
                    minimumSize: new Size(300, 200),
                    theme: _currentTheme);
                borderedWindow.Text = "4-Panel Form";
                borderedWindow.Show();
            });
            
            // Show menu at button location
            var buttonLocation = _threePanelButton.PointToScreen(new Point(0, _threePanelButton.Height));
            menu.Show(buttonLocation);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening Independent Panel Form: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void FormBuilderButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Create and show the Form Builder using FormBuilderForm (which is a TwoLayerForm)
            var formBuilder = new FormBuilderForm(_currentTheme);
            formBuilder.Text = "Form Builder";
            formBuilder.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening Form Builder: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    #endregion

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateLayout();
        this.Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _wordDocManager?.Dispose();
        }
        base.Dispose(disposing);
    }
}
