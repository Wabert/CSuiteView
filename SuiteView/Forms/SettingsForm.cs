using SuiteView.Managers;
using SuiteView.Models;

namespace SuiteView.Forms;

/// <summary>
/// Settings form for configuring application preferences
/// </summary>
public class SettingsForm : Form
{
    private readonly ConfigManager _configManager;
    private readonly Form _mainForm;
    private readonly ResizableBorderForm? _borderForm;

    private ComboBox _themeComboBox;
    private CheckBox _launchOnStartupCheckBox;
    private TrackBar _opacityTrackBar;
    private Label _opacityValueLabel;
    private CheckBox _alwaysOnTopCheckBox;
    private Button _saveButton;
    private Button _cancelButton;

    private ColorTheme _currentTheme;

    public SettingsForm(ConfigManager configManager, Form mainForm, ResizableBorderForm? borderForm = null)
    {
        _configManager = configManager;
        _mainForm = mainForm;
        _borderForm = borderForm;
        _currentTheme = ThemeManager.GetTheme(_configManager.Settings.SelectedTheme);

        InitializeComponent();
        LoadSettings();
        ApplyTheme();
    }

    private void InitializeComponent()
    {
        this.Text = "SuiteView Settings";
        this.Size = new Size(450, 380);
        this.FormBorderStyle = FormBorderStyle.None; // Borderless for custom look
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = _currentTheme.Secondary;
        this.ForeColor = _currentTheme.TextOnSecondary;

        // Enable double buffering
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
        this.UpdateStyles();

        // Add paint event for custom styling
        this.Paint += SettingsForm_Paint;

        // Title bar height
        const int titleBarHeight = 35;
        const int borderWidth = 5;

        // Title label
        var titleLabel = new Label
        {
            Text = "Settings",
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location = new Point(borderWidth + 12, 8),
            AutoSize = true,
            BackColor = Color.Transparent
        };

        // Close button
        var closeButton = new Button
        {
            Text = "",
            Size = new Size(24, 24),
            Location = new Point(this.Width - 24 - borderWidth - 8, 6),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        closeButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        closeButton.Click += (s, e) => this.Close();

        this.Controls.Add(titleLabel);
        this.Controls.Add(closeButton);

        int yOffset = titleBarHeight + borderWidth + 20;
        int labelWidth = 150;
        int controlX = labelWidth + 30 + borderWidth;

        // Color Theme Selection
        var themeLabel = new Label
        {
            Text = "Color Theme:",
            Location = new Point(20 + borderWidth, yOffset),
            Size = new Size(labelWidth, 20),
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        _themeComboBox = new ComboBox
        {
            Location = new Point(controlX, yOffset - 2),
            Size = new Size(250, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9f)
        };
        _themeComboBox.Items.AddRange(ThemeManager.GetThemeNames().ToArray());
        _themeComboBox.SelectedIndexChanged += ThemeComboBox_SelectedIndexChanged;

        yOffset += 40;

        // Launch on Startup
        _launchOnStartupCheckBox = new CheckBox
        {
            Text = "Launch on Windows Startup",
            Location = new Point(20, yOffset),
            Size = new Size(400, 24),
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 9f)
        };

        yOffset += 40;

        // Opacity Slider
        var opacityLabel = new Label
        {
            Text = "Window Opacity:",
            Location = new Point(20, yOffset),
            Size = new Size(labelWidth, 20),
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        _opacityTrackBar = new TrackBar
        {
            Location = new Point(controlX, yOffset - 5),
            Size = new Size(200, 45),
            Minimum = 70,
            Maximum = 100,
            TickFrequency = 5,
            SmallChange = 1,
            LargeChange = 10
        };
        _opacityTrackBar.ValueChanged += OpacityTrackBar_ValueChanged;

        _opacityValueLabel = new Label
        {
            Location = new Point(controlX + 210, yOffset),
            Size = new Size(40, 20),
            Text = "100%",
            ForeColor = _currentTheme.TextOnSecondary,
            Font = new Font("Segoe UI", 9f)
        };

        yOffset += 50;

        // Always on Top
        _alwaysOnTopCheckBox = new CheckBox
        {
            Text = "Always on Top",
            Location = new Point(20, yOffset),
            Size = new Size(400, 24),
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 9f)
        };

        yOffset += 50;

        // Separator line
        var separator = new Panel
        {
            Location = new Point(20, yOffset),
            Size = new Size(400, 2),
            BackColor = _currentTheme.Primary
        };

        yOffset += 20;

        // Save Button
        _saveButton = new Button
        {
            Text = "Save",
            Location = new Point(controlX + 50, yOffset),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = _currentTheme.Primary,
            ForeColor = _currentTheme.TextOnPrimary,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        _saveButton.MouseEnter += SaveButton_MouseEnter;
        _saveButton.MouseLeave += SaveButton_MouseLeave;

        // Cancel Button
        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(controlX + 160, yOffset),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(108, 117, 125), // Gray
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f),
            Cursor = Cursors.Hand
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += CancelButton_Click;

        // Add all controls
        this.Controls.AddRange(new Control[]
        {
            themeLabel,
            _themeComboBox,
            _launchOnStartupCheckBox,
            opacityLabel,
            _opacityTrackBar,
            _opacityValueLabel,
            _alwaysOnTopCheckBox,
            separator,
            _saveButton,
            _cancelButton
        });
    }

    private void LoadSettings()
    {
        var settings = _configManager.Settings;

        // Set theme
        _themeComboBox.SelectedItem = settings.SelectedTheme;

        // Set launch on startup
        // Check actual registry state
        _launchOnStartupCheckBox.Checked = StartupManager.IsInStartup();

        // Set opacity
        _opacityTrackBar.Value = settings.Opacity;
        _opacityValueLabel.Text = $"{settings.Opacity}%";

        // Set always on top
        _alwaysOnTopCheckBox.Checked = settings.AlwaysOnTop;
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;

        foreach (Control control in this.Controls)
        {
            if (control is Label label)
            {
                label.ForeColor = _currentTheme.Accent;
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = _currentTheme.Accent;
            }
        }

        _saveButton.BackColor = _currentTheme.Primary;
        _saveButton.ForeColor = _currentTheme.TextOnPrimary;

        this.Invalidate();
    }

    private void ThemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Preview theme change
        if (_themeComboBox.SelectedItem is string themeName)
        {
            _currentTheme = ThemeManager.GetTheme(themeName);
            ApplyTheme();
        }
    }

    private void OpacityTrackBar_ValueChanged(object sender, EventArgs e)
    {
        _opacityValueLabel.Text = $"{_opacityTrackBar.Value}%";
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            var selectedTheme = _themeComboBox.SelectedItem as string ?? "Emerald";

            // Update settings
            _configManager.UpdateSettings(settings =>
            {
                settings.SelectedTheme = selectedTheme;
                settings.LaunchOnStartup = _launchOnStartupCheckBox.Checked;
                settings.Opacity = _opacityTrackBar.Value;
                settings.AlwaysOnTop = _alwaysOnTopCheckBox.Checked;
            });

            // Apply theme to main form
            var updateThemeMethod = _mainForm.GetType().GetMethod("UpdateTheme");
            if (updateThemeMethod != null)
            {
                updateThemeMethod.Invoke(_mainForm, new object[] { selectedTheme });
            }

            // Update border form properties (parent controls these settings)
            _borderForm.UpdateOpacity(_opacityTrackBar.Value);
            _borderForm.UpdateAlwaysOnTop(_alwaysOnTopCheckBox.Checked);

            // Update startup registry
            StartupManager.UpdateStartupStatus(_launchOnStartupCheckBox.Checked);

            MessageBox.Show(
                "Settings saved successfully!",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error saving settings: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        // Restore original theme preview
        _currentTheme = ThemeManager.GetTheme(_configManager.Settings.SelectedTheme);
        ApplyTheme();

        this.Close();
    }

    private void SaveButton_MouseEnter(object sender, EventArgs e)
    {
        _saveButton.BackColor = Color.FromArgb(
            Math.Min(255, _currentTheme.Primary.R + 30),
            Math.Min(255, _currentTheme.Primary.G + 30),
            Math.Min(255, _currentTheme.Primary.B + 30)
        );
    }

    private void SaveButton_MouseLeave(object sender, EventArgs e)
    {
        _saveButton.BackColor = _currentTheme.Primary;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        // Restore original theme if cancelled
        var updateThemeMethod = _mainForm.GetType().GetMethod("UpdateTheme");
        if (updateThemeMethod != null)
        {
            updateThemeMethod.Invoke(_mainForm, new object[] { _configManager.Settings.SelectedTheme });
        }
    }

    private void SettingsForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        const int titleBarHeight = 35;
        const int borderWidth = 5;

        // Draw 3D brass border
        var brassColor = ColorTranslator.FromHtml("#D4AF37");
        var lightBrass = ControlPaint.Light(brassColor, 0.3f);
        var darkBrass = ControlPaint.Dark(brassColor, 0.2f);

        // Fill border area with brass
        using (var brush = new SolidBrush(brassColor))
        {
            // Top border
            g.FillRectangle(brush, 0, 0, this.Width, borderWidth);
            // Left border
            g.FillRectangle(brush, 0, 0, borderWidth, this.Height);
            // Right border
            g.FillRectangle(brush, this.Width - borderWidth, 0, borderWidth, this.Height);
            // Bottom border
            g.FillRectangle(brush, 0, this.Height - borderWidth, this.Width, borderWidth);
        }

        // Draw outer highlight (top and left) for 3D effect
        using (var pen = new Pen(lightBrass, 2))
        {
            g.DrawLine(pen, 0, 0, this.Width, 0);
            g.DrawLine(pen, 0, 0, 0, this.Height);
        }

        // Draw inner shadow (bottom and right) for 3D effect
        using (var pen = new Pen(darkBrass, 2))
        {
            g.DrawLine(pen, 0, this.Height - 2, this.Width, this.Height - 2);
            g.DrawLine(pen, this.Width - 2, 0, this.Width - 2, this.Height);
        }

        // Draw gradient title bar
        var titleRect = new Rectangle(borderWidth, borderWidth, this.Width - (borderWidth * 2), titleBarHeight);
        var lighterBlue = ControlPaint.Light(_currentTheme.Primary, 0.2f);
        using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            titleRect,
            lighterBlue,
            _currentTheme.Primary,
            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
        {
            g.FillRectangle(gradientBrush, titleRect);
        }

        // Draw brass separator line below title bar
        using (var pen = new Pen(_currentTheme.Accent, 2))
        {
            int y = borderWidth + titleBarHeight;
            g.DrawLine(pen, borderWidth, y, this.Width - borderWidth, y);
        }

        // Draw walnut content area
        var contentRect = new Rectangle(
            borderWidth,
            borderWidth + titleBarHeight,
            this.Width - (borderWidth * 2),
            this.Height - (borderWidth * 2) - titleBarHeight
        );
        using (var brush = new SolidBrush(_currentTheme.Secondary))
        {
            g.FillRectangle(brush, contentRect);
        }

        // Draw round close button
        var closeButtonBounds = new Rectangle(this.Width - 24 - borderWidth - 8, 6, 24, 24);
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillEllipse(brush, closeButtonBounds);
        }
        using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var text = "✕";
            var size = g.MeasureString(text, font);
            var x = closeButtonBounds.X + (closeButtonBounds.Width - size.Width) / 2;
            var y = closeButtonBounds.Y + (closeButtonBounds.Height - size.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }
    }
}
