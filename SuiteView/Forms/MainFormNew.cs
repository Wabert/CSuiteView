using SuiteView.Managers;
using SuiteView.Models;
using SuiteView.Forms.LayeredForms;
using System.Drawing;
using System.Windows.Forms;

namespace SuiteView.Forms;

/// <summary>
/// Main SuiteView form - built using TwoLayerForm for consistent layered architecture
/// Layer 1: Royal blue border with brass accent lines
/// Content: Medium blue panel with application buttons and Form Builder
/// </summary>
public class MainFormNew : TwoLayerForm
{
    private readonly ConfigManager _configManager;
    private ColorTheme _currentTheme;
    
    // Main action buttons
    private Button _snapButton = null!;
    private Button _scanDirButton = null!;
    private Button _dbLibraryButton = null!;
    private Button _threePanelButton = null!;
    private Button _testFormBuilderButton = null!;
    
    private WordDocumentManager? _wordDocManager;

    public MainFormNew(ConfigManager configManager)
        : base(
            formWidth: 800,
            formHeight: 600,
            layer1HeaderHeight: 30,
            layer1FooterHeight: 30,
            theme: ThemeManager.GetTheme(configManager.Settings.SelectedTheme))
    {
        _configManager = configManager;
        _currentTheme = _theme;
        
        InitializeMainFormControls();
    }

    private void InitializeMainFormControls()
    {
        var contentPanel = this.ContentPanel;
        
        // Snap button
        _snapButton = new Button
        {
            Text = "",
            Size = new Size(80, 32),
            Location = new Point(20, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _snapButton.FlatAppearance.BorderSize = 0;
        _snapButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _snapButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _snapButton.Click += SnapButton_Click;
        _snapButton.Paint += SnapButton_Paint;

        // Scan Directory button
        _scanDirButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 65),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _scanDirButton.FlatAppearance.BorderSize = 0;
        _scanDirButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _scanDirButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _scanDirButton.Click += ScanDirButton_Click;
        _scanDirButton.Paint += ScanDirButton_Paint;

        // DB Library button
        _dbLibraryButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 110),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _dbLibraryButton.FlatAppearance.BorderSize = 0;
        _dbLibraryButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _dbLibraryButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _dbLibraryButton.Click += DbLibraryButton_Click;
        _dbLibraryButton.Paint += DbLibraryButton_Paint;

        // Three Panel button
        _threePanelButton = new Button
        {
            Text = "",
            Size = new Size(120, 32),
            Location = new Point(20, 155),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _threePanelButton.FlatAppearance.BorderSize = 0;
        _threePanelButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _threePanelButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _threePanelButton.Click += ThreePanelButton_Click;
        _threePanelButton.Paint += ThreePanelButton_Paint;

        // Test Form Builder button
        _testFormBuilderButton = new Button
        {
            Text = "",
            Size = new Size(150, 32),
            Location = new Point(20, 200),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _testFormBuilderButton.FlatAppearance.BorderSize = 0;
        _testFormBuilderButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _testFormBuilderButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _testFormBuilderButton.Click += TestFormBuilderButton_Click;
        _testFormBuilderButton.Paint += TestFormBuilderButton_Paint;

        contentPanel.Controls.Add(_snapButton);
        contentPanel.Controls.Add(_scanDirButton);
        contentPanel.Controls.Add(_dbLibraryButton);
        contentPanel.Controls.Add(_threePanelButton);
        contentPanel.Controls.Add(_testFormBuilderButton);
    }

    #region Button Paint Events

    private void SnapButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, button.Width, button.Height);
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillRoundedRectangle(brush, rect, 8);
        }

        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Snap", new Font("Segoe UI", 10f, FontStyle.Bold), brush, rect, sf);
        }
    }

    private void ScanDirButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, button.Width, button.Height);
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillRoundedRectangle(brush, rect, 8);
        }

        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Scan Directory", new Font("Segoe UI", 9f, FontStyle.Bold), brush, rect, sf);
        }
    }

    private void DbLibraryButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, button.Width, button.Height);
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillRoundedRectangle(brush, rect, 8);
        }

        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("DB Library", new Font("Segoe UI", 9f, FontStyle.Bold), brush, rect, sf);
        }
    }

    private void ThreePanelButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, button.Width, button.Height);
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillRoundedRectangle(brush, rect, 8);
        }

        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("3-Panel Form", new Font("Segoe UI", 9f, FontStyle.Bold), brush, rect, sf);
        }
    }

    #endregion

    #region Button Click Handlers

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
            await System.Threading.Tasks.Task.Delay(200);

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
        var scannerForm = LayeredFormFactory.CreateThreeLayerForm(
            formWidth: 1200,
            formHeight: 700,
            layer1HeaderHeight: 30,
            layer1FooterHeight: 30,
            layer2HeaderHeight: 0,
            layer2FooterHeight: 0,
            layer2BorderWidth: 15,
            panelCount: 2,
            minimumSize: new Size(800, 500),
            theme: _currentTheme);

        var contentForm = new DirectoryScannerContentForm(_currentTheme);
        (scannerForm.Controls[0] as TwoLayerForm)?.SetContent(contentForm);
        scannerForm.Text = "Directory Scanner";
        scannerForm.Show();
    }

    private void DbLibraryButton_Click(object? sender, EventArgs e)
    {
        var libraryForm = LayeredFormFactory.CreateThreeLayerForm(
            formWidth: 1200,
            formHeight: 700,
            layer1HeaderHeight: 30,
            layer1FooterHeight: 30,
            layer2HeaderHeight: 0,
            layer2FooterHeight: 0,
            layer2BorderWidth: 15,
            panelCount: 2,
            minimumSize: new Size(800, 500),
            theme: _currentTheme);

        var contentForm = new DatabaseLibraryManagerContentForm(_currentTheme);
        (libraryForm.Controls[0] as TwoLayerForm)?.SetContent(contentForm);
        libraryForm.Text = "Database Library Manager";
        libraryForm.Show();
    }

    private void ThreePanelButton_Click(object? sender, EventArgs e)
    {
        var threePanelForm = LayeredFormFactory.CreateThreeLayerForm(
            formWidth: 1200,
            formHeight: 700,
            layer1HeaderHeight: 30,
            layer1FooterHeight: 30,
            layer2HeaderHeight: 0,
            layer2FooterHeight: 0,
            layer2BorderWidth: 15,
            panelCount: 3,
            minimumSize: new Size(800, 500),
            theme: _currentTheme);

        threePanelForm.Text = "Three Panel Form";
        threePanelForm.Show();
    }

    #endregion

    #region Button Click Handlers (continued)

    private void TestFormBuilderButton_Click(object? sender, EventArgs e)
    {
        // Create the Form Builder form wrapped in BorderedWindowForm
        var formBuilderContentForm = new FormBuilderForm(_currentTheme);
        
        var borderedWindow = new BorderedWindowForm(
            theme: _currentTheme,
            initialSize: new Size(400, 680),
            minimumSize: new Size(400, 600));

        borderedWindow.SetContentForm(formBuilderContentForm);
        borderedWindow.Text = "Layered Form Builder";
        borderedWindow.Show();
    }

    private void TestFormBuilderButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, button.Width, button.Height);
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillRoundedRectangle(brush, rect, 8);
        }

        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Test Form Builder", new Font("Segoe UI", 9f, FontStyle.Bold), brush, rect, sf);
        }
    }

    #endregion

    public void UpdateTheme(string themeName)
    {
        _currentTheme = ThemeManager.GetTheme(themeName);
        
        // Update the form's background color
        this.BackColor = _currentTheme.Primary;
        this.ContentPanel.BackColor = _currentTheme.Secondary;
        
        // Refresh button paints
        _snapButton.Invalidate();
        _scanDirButton.Invalidate();
        _dbLibraryButton.Invalidate();
        _threePanelButton.Invalidate();
        _testFormBuilderButton.Invalidate();
        
        this.Invalidate();
    }
}

// Extension method for rounded rectangles
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}
