using SuiteView.Models;

namespace SuiteView.Forms;

/// <summary>
/// Filter window content form - displays filterable column values
/// </summary>
public class FilterWindowContentForm : Form, IContentForm
{
    private readonly ColorTheme _currentTheme;
    private Form? _parentBorderForm;
    
    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Panel _contentPanel = null!;
    private CheckedListBox _valuesListBox = null!;
    private CheckBox _selectAllCheckBox = null!;
    private CheckBox _deselectAllCheckBox = null!;
    private Button _applyButton = null!;
    private Button _clearButton = null!;
    private Label _infoLabel = null!;

    private const int TitleBarHeight = 35;
    private const int ControlMargin = 10;
    private const int ButtonHeight = 28;

    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _dragParentStart;

    public event EventHandler<FilterAppliedEventArgs>? FilterApplied;

    private readonly string _columnName;
    private readonly HashSet<string> _allValues;
    private readonly HashSet<string>? _currentSelection;

    public FilterWindowContentForm(ColorTheme theme, string columnName, HashSet<string> allValues, HashSet<string>? currentSelection)
    {
        _currentTheme = theme;
        _columnName = columnName;
        _allValues = allValues;
        _currentSelection = currentSelection;

        this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();

        InitializeComponent();
        SetupForm();
        ApplyTheme();
        LoadValues();
    }

    public void SetParentBorderForm(Form parent)
    {
        _parentBorderForm = parent;
    }

    private void InitializeComponent()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = _currentTheme.Secondary;
        this.DoubleBuffered = true;

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
            Text = $"Filter: {_columnName}",
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Location = new Point(12, 8),
            AutoSize = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.SizeAll,
            UseMnemonic = false
        };

        // Content panel
        _contentPanel = new Panel
        {
            Location = new Point(0, TitleBarHeight),
            AutoScroll = false,
            BackColor = _currentTheme.Secondary
        };

        // Info label
        _infoLabel = new Label
        {
            Location = new Point(ControlMargin, ControlMargin),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8f),
            BackColor = Color.Transparent
        };

        // Select All checkbox
        _selectAllCheckBox = new CheckBox
        {
            Text = "(Select All)",
            Location = new Point(ControlMargin, _infoLabel.Bottom + 5),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.Transparent,
            Checked = _currentSelection == null
        };
        _selectAllCheckBox.CheckedChanged += SelectAllCheckBox_CheckedChanged;

        // Deselect All checkbox
        _deselectAllCheckBox = new CheckBox
        {
            Text = "(Deselect All)",
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.Transparent,
            Checked = false
        };
        _deselectAllCheckBox.CheckedChanged += DeselectAllCheckBox_CheckedChanged;

        // Values list box with checkboxes
        _valuesListBox = new CheckedListBox
        {
            CheckOnClick = true,
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false,
            Font = new Font("Segoe UI", 9f)
        };

        // Apply button
        _applyButton = new Button
        {
            Text = "Apply Filter",
            Height = ButtonHeight,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _applyButton.Click += ApplyButton_Click;

        // Clear button (clears filter for this column only)
        _clearButton = new Button
        {
            Text = "Clear",
            Height = ButtonHeight,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand
        };
        _clearButton.Click += ClearButton_Click;

        // Add controls
        _contentPanel.Controls.Add(_infoLabel);
        _contentPanel.Controls.Add(_selectAllCheckBox);
        _contentPanel.Controls.Add(_deselectAllCheckBox);
        _contentPanel.Controls.Add(_valuesListBox);
        _contentPanel.Controls.Add(_applyButton);
        _contentPanel.Controls.Add(_clearButton);

        this.Controls.Add(_contentPanel);
        this.Controls.Add(_titleLabel);
        this.Controls.Add(_closeButton);

        // Dragging support
        _titleLabel.MouseDown += Form_MouseDown;
        _titleLabel.MouseMove += Form_MouseMove;
        _titleLabel.MouseUp += Form_MouseUp;
        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;

        this.Paint += ContentForm_Paint;
    }

    private void SetupForm()
    {
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        _closeButton.Location = new Point(this.Width - _closeButton.Width - 6, 4);
        _contentPanel.Size = new Size(this.Width, this.Height - TitleBarHeight);

        int availableWidth = _contentPanel.Width - (ControlMargin * 2);
        
        // Update info label
        _infoLabel.MaximumSize = new Size(availableWidth, 0);
        _infoLabel.Text = $"Showing {Math.Min(_allValues.Count, 1000):N0} of {_allValues.Count:N0} values";

        // Position checkboxes side by side
        _selectAllCheckBox.Location = new Point(ControlMargin, _infoLabel.Bottom + 5);
        _deselectAllCheckBox.Location = new Point(_selectAllCheckBox.Right + 15, _infoLabel.Bottom + 5);

        // Update list box size
        int listBoxTop = _selectAllCheckBox.Bottom + 5;
        int listBoxHeight = _contentPanel.Height - listBoxTop - ButtonHeight - (ControlMargin * 3);
        _valuesListBox.Location = new Point(ControlMargin, listBoxTop);
        _valuesListBox.Size = new Size(availableWidth, listBoxHeight);

        // Position buttons at bottom - make them half width (about 25% each with gap)
        int buttonY = _contentPanel.Height - ButtonHeight - ControlMargin;
        int buttonWidth = (availableWidth - 5) / 4; // Half the previous width
        int buttonStartX = (availableWidth - (buttonWidth * 2 + 5)) / 2 + ControlMargin; // Center the buttons
        _applyButton.Location = new Point(buttonStartX, buttonY);
        _applyButton.Width = buttonWidth;
        _clearButton.Location = new Point(_applyButton.Right + 5, buttonY);
        _clearButton.Width = buttonWidth;
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;
        _titleLabel.ForeColor = _currentTheme.Accent;
        _contentPanel.BackColor = _currentTheme.Secondary;

        _valuesListBox.BackColor = ControlPaint.Light(_currentTheme.Secondary, 0.1f);
        _valuesListBox.ForeColor = Color.White;

        _applyButton.BackColor = _currentTheme.Accent;
        _applyButton.ForeColor = _currentTheme.Primary;
        _applyButton.FlatAppearance.BorderColor = _currentTheme.Accent;

        _clearButton.BackColor = ControlPaint.Dark(_currentTheme.Secondary, 0.1f);
        _clearButton.ForeColor = Color.White;
        _clearButton.FlatAppearance.BorderColor = _currentTheme.Accent;
    }

    private void LoadValues()
    {
        // Load values (limited to 1000 for performance)
        var sortedValues = _allValues.OrderBy(v => v).Take(1000).ToList();

        _valuesListBox.BeginUpdate();
        _valuesListBox.Items.Clear();

        foreach (var value in sortedValues)
        {
            string displayText = string.IsNullOrEmpty(value) ? "(Blank)" : value;
            if (displayText.Length > 100)
            {
                displayText = displayText.Substring(0, 97) + "...";
            }

            int index = _valuesListBox.Items.Add(displayText);
            
            // Set checked state based on current selection
            if (_currentSelection == null || _currentSelection.Contains(value))
            {
                _valuesListBox.SetItemChecked(index, true);
            }
        }

        _valuesListBox.EndUpdate();
    }

    private void SelectAllCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (_selectAllCheckBox.Checked)
        {
            // Uncheck deselect all
            _deselectAllCheckBox.CheckedChanged -= DeselectAllCheckBox_CheckedChanged;
            _deselectAllCheckBox.Checked = false;
            _deselectAllCheckBox.CheckedChanged += DeselectAllCheckBox_CheckedChanged;

            // Check all items
            for (int i = 0; i < _valuesListBox.Items.Count; i++)
            {
                _valuesListBox.SetItemChecked(i, true);
            }
        }
    }

    private void DeselectAllCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (_deselectAllCheckBox.Checked)
        {
            // Uncheck select all
            _selectAllCheckBox.CheckedChanged -= SelectAllCheckBox_CheckedChanged;
            _selectAllCheckBox.Checked = false;
            _selectAllCheckBox.CheckedChanged += SelectAllCheckBox_CheckedChanged;

            // Uncheck all items
            for (int i = 0; i < _valuesListBox.Items.Count; i++)
            {
                _valuesListBox.SetItemChecked(i, false);
            }
        }
    }

    private void ApplyButton_Click(object? sender, EventArgs e)
    {
        var selectedValues = new HashSet<string>();
        var sortedValues = _allValues.OrderBy(v => v).Take(1000).ToList();

        for (int i = 0; i < _valuesListBox.Items.Count; i++)
        {
            if (_valuesListBox.GetItemChecked(i))
            {
                selectedValues.Add(sortedValues[i]);
            }
        }

        // If no items are selected, treat as clearing the filter (show all)
        // If all AVAILABLE items are selected, treat as no filter
        // Otherwise, apply the filter with the selected values
        if (selectedValues.Count == 0 || selectedValues.Count == _valuesListBox.Items.Count)
        {
            // No filter - show all values
            FilterApplied?.Invoke(this, new FilterAppliedEventArgs(_columnName, null));
        }
        else
        {
            // Apply filter with selected values only
            FilterApplied?.Invoke(this, new FilterAppliedEventArgs(_columnName, selectedValues));
        }

        if (_parentBorderForm != null)
        {
            _parentBorderForm.Close();
        }
        else
        {
            this.Close();
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        // Clear filter for THIS column only (not all columns)
        FilterApplied?.Invoke(this, new FilterAppliedEventArgs(_columnName, null));

        if (_parentBorderForm != null)
        {
            _parentBorderForm.Close();
        }
        else
        {
            this.Close();
        }
    }

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        if (_parentBorderForm != null)
        {
            _parentBorderForm.Close();
        }
        else
        {
            this.Close();
        }
    }

    #region Custom Painting

    private void ContentForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw gradient title bar
        var titleRect = new Rectangle(0, 0, this.Width, TitleBarHeight);
        var lighterBlue = ControlPaint.Light(_currentTheme.Primary, 0.2f);
        using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            titleRect, lighterBlue, _currentTheme.Primary,
            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
        {
            g.FillRectangle(gradientBrush, titleRect);
        }

        // Draw content area
        using (var brush = new SolidBrush(_currentTheme.Secondary))
        {
            g.FillRectangle(brush, 0, TitleBarHeight, this.Width, this.Height - TitleBarHeight);
        }

        // Draw brass accent line
        using (var pen = new Pen(_currentTheme.Accent, 2))
        {
            g.DrawLine(pen, 0, TitleBarHeight - 1, this.Width, TitleBarHeight - 1);
        }

        // Draw close button
        DrawRoundCloseButton(g);
    }

    private void DrawRoundCloseButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var buttonRect = _closeButton.Bounds;

        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillEllipse(brush, buttonRect);
        }

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

    #endregion

    #region Dragging Logic

    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && sender != _closeButton && _parentBorderForm != null)
        {
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
            int deltaX = Cursor.Position.X - _dragStartPoint.X;
            int deltaY = Cursor.Position.Y - _dragStartPoint.Y;
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

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateLayout();
        this.Invalidate();
    }
}

/// <summary>
/// Event args for when a filter is applied
/// </summary>
public class FilterAppliedEventArgs : EventArgs
{
    public string ColumnName { get; }
    public HashSet<string>? SelectedValues { get; }

    public FilterAppliedEventArgs(string columnName, HashSet<string>? selectedValues)
    {
        ColumnName = columnName;
        SelectedValues = selectedValues;
    }
}
