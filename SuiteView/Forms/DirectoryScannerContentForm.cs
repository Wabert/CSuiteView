using SuiteView.Managers;
using SuiteView.Models;
using System.ComponentModel;

namespace SuiteView.Forms;

/// <summary>
/// Directory Scanner Content Form - Scans directories and displays file/folder information
/// Uses the iconic SuiteView look with bordered window wrapper
/// </summary>
public partial class DirectoryScannerContentForm : Form, IContentForm
{
    private readonly ColorTheme _currentTheme;
    private Form? _parentBorderForm;
    
    // UI Controls
    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Panel _contentPanel = null!;
    private TextBox _pathTextBox = null!;
    private Button _browseButton = null!;
    private Button _scanButton = null!;
    private DataGridView _resultsGrid = null!;
    private Label _recordCountLabel = null!;
    private Button _clearAllButton = null!;
    private Button _exportButton = null!;
    private Panel _footerPanel = null!;
    
    // Data structures
    private BindingList<FileSystemItem> _dataSource = null!;
    private List<FileSystemItem> _allItems = new();
    
    // Cached unique values for filtering (built during scan for performance)
    private Dictionary<string, HashSet<string>> _cachedColumnValues = new();
    private Dictionary<string, HashSet<string>> _activeFilters = new();
    
    // Track open filter windows to prevent duplicates
    private Dictionary<string, BorderedWindowForm> _openFilterWindows = new();
    
    // Layout constants
    private const int TitleBarHeight = 35;
    private const int FooterHeight = 35;
    private const int InputSectionHeight = 40;
    private const int ControlMargin = 10;
    
    // Dragging support
    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _dragParentStart;

    public DirectoryScannerContentForm(ColorTheme theme)
    {
        _currentTheme = theme;
        
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
            Text = "Directory Scanner",
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
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

        // Path text box
        _pathTextBox = new TextBox
        {
            Location = new Point(ControlMargin, ControlMargin),
            Font = new Font("Segoe UI", 10f),
            TabIndex = 0
        };

        // Browse button
        _browseButton = new Button
        {
            Text = "Browse...",
            Size = new Size(100, _pathTextBox.Height),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabIndex = 1
        };
        _browseButton.Click += BrowseButton_Click;

        // Scan button
        _scanButton = new Button
        {
            Text = "Scan",
            Size = new Size(100, _pathTextBox.Height),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabIndex = 2
        };
        _scanButton.Click += ScanButton_Click;

        // Initialize data source
        _dataSource = new BindingList<FileSystemItem>();

        // Results DataGridView with virtualization for performance
        _resultsGrid = new DataGridView
        {
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = true,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = _currentTheme.Secondary,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9f),
            DataSource = _dataSource,
            VirtualMode = false, // BindingList handles this efficiently
            TabIndex = 3
        };

        // Define columns
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "FullPath",
            HeaderText = "Full Path",
            Name = "FullPath",
            FillWeight = 40
        });
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Name",
            HeaderText = "Name",
            Name = "Name",
            FillWeight = 25
        });
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Type",
            HeaderText = "Type",
            Name = "Type",
            FillWeight = 10
        });
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "DateModified",
            HeaderText = "Date Modified",
            Name = "DateModified",
            FillWeight = 15,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
        });
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Size",
            HeaderText = "Size",
            Name = "Size",
            FillWeight = 10
        });

        _resultsGrid.ColumnHeaderMouseClick += ResultsGrid_ColumnHeaderMouseClick;
        _resultsGrid.ColumnHeaderMouseDoubleClick += ResultsGrid_ColumnHeaderMouseDoubleClick;

        // Footer panel (styled like title bar)
        _footerPanel = new Panel
        {
            BackColor = _currentTheme.Primary,
            Height = FooterHeight
        };

        // Record count label
        _recordCountLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.Transparent,
            Text = "0 records displayed"
        };

        // Clear All button (brass/gold)
        _clearAllButton = new Button
        {
            Text = "Clear All",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(80, 26),
            TabIndex = 4
        };
        _clearAllButton.FlatAppearance.BorderSize = 0;
        _clearAllButton.Click += ClearAllButton_Click;

        // Export button (green)
        _exportButton = new Button
        {
            Text = "Export",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 139, 34), // Forest green
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(80, 26),
            TabIndex = 5
        };
        _exportButton.FlatAppearance.BorderSize = 0;
        _exportButton.Click += ExportButton_Click;

        // Add controls to footer panel
        _footerPanel.Controls.Add(_recordCountLabel);
        _footerPanel.Controls.Add(_clearAllButton);
        _footerPanel.Controls.Add(_exportButton);

        // Add controls to content panel
        _contentPanel.Controls.Add(_pathTextBox);
        _contentPanel.Controls.Add(_browseButton);
        _contentPanel.Controls.Add(_scanButton);
        _contentPanel.Controls.Add(_resultsGrid);
        _contentPanel.Controls.Add(_footerPanel);

        // Add controls to form
        this.Controls.Add(_contentPanel);
        this.Controls.Add(_titleLabel);
        this.Controls.Add(_closeButton);

        // Make title bar draggable
        _titleLabel.MouseDown += Form_MouseDown;
        _titleLabel.MouseMove += Form_MouseMove;
        _titleLabel.MouseUp += Form_MouseUp;

        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;

        // Paint event for custom rendering
        this.Paint += ContentForm_Paint;
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

        // Calculate available width for path textbox
        int availableWidth = _contentPanel.Width - (ControlMargin * 2) - _browseButton.Width - _scanButton.Width - 10;
        _pathTextBox.Width = Math.Max(200, availableWidth);
        
        // Position controls
        _pathTextBox.Location = new Point(ControlMargin, ControlMargin);
        _browseButton.Location = new Point(_pathTextBox.Right + 5, ControlMargin);
        _scanButton.Location = new Point(_browseButton.Right + 5, ControlMargin);

        // Position and size the results grid (leave room for footer at bottom)
        int gridTop = InputSectionHeight + ControlMargin;
        _resultsGrid.Location = new Point(ControlMargin, gridTop);
        _resultsGrid.Size = new Size(
            _contentPanel.Width - (ControlMargin * 2),
            _contentPanel.Height - gridTop - FooterHeight
        );

        // Position footer panel at bottom (full width, like title bar)
        _footerPanel.Location = new Point(0, _contentPanel.Height - FooterHeight);
        _footerPanel.Size = new Size(_contentPanel.Width, FooterHeight);

        // Position record count label in footer (left side, vertically centered)
        _recordCountLabel.Location = new Point(
            ControlMargin,
            (FooterHeight - _recordCountLabel.Height) / 2
        );

        // Position Export button in footer (right side, vertically centered)
        _exportButton.Location = new Point(
            _footerPanel.Width - _exportButton.Width - ControlMargin,
            (FooterHeight - _exportButton.Height) / 2
        );

        // Position Clear All button in footer (next to Export button, vertically centered)
        _clearAllButton.Location = new Point(
            _exportButton.Left - _clearAllButton.Width - 5,
            (FooterHeight - _clearAllButton.Height) / 2
        );
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;
        _titleLabel.ForeColor = _currentTheme.Accent;
        _titleLabel.BackColor = Color.Transparent;
        _contentPanel.BackColor = _currentTheme.Secondary;

        // Apply theme to input controls
        _pathTextBox.BackColor = ControlPaint.Light(_currentTheme.Secondary, 0.1f);
        _pathTextBox.ForeColor = Color.White; // White text for path input

        _browseButton.BackColor = _currentTheme.Accent;
        _browseButton.ForeColor = _currentTheme.Primary;
        _browseButton.FlatAppearance.BorderColor = _currentTheme.Accent;

        _scanButton.BackColor = _currentTheme.Accent;
        _scanButton.ForeColor = _currentTheme.Primary;
        _scanButton.FlatAppearance.BorderColor = _currentTheme.Accent;

        // Apply theme to DataGridView (no alternating colors as requested)
        _resultsGrid.BackgroundColor = _currentTheme.Secondary;
        _resultsGrid.GridColor = ControlPaint.Light(_currentTheme.Secondary, 0.2f);
        
        // All text in table should be white
        _resultsGrid.DefaultCellStyle.BackColor = _currentTheme.Secondary;
        _resultsGrid.DefaultCellStyle.ForeColor = Color.White;
        _resultsGrid.DefaultCellStyle.SelectionBackColor = _currentTheme.Primary;
        _resultsGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        
        // Headers: brass/gold color with raised 3D effect
        _resultsGrid.ColumnHeadersDefaultCellStyle.BackColor = _currentTheme.Accent;
        _resultsGrid.ColumnHeadersDefaultCellStyle.ForeColor = _currentTheme.Primary;
        _resultsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        _resultsGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = _currentTheme.Accent; // Keep brass when clicked
        _resultsGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = _currentTheme.Primary; // Keep blue text when clicked
        _resultsGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised;
        _resultsGrid.EnableHeadersVisualStyles = false;
    }

    #region Custom Painting

    private void ContentForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw gradient title bar background
        var titleRect = new Rectangle(0, 0, this.Width, TitleBarHeight);
        var lighterBlue = ControlPaint.Light(_currentTheme.Primary, 0.2f);
        using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            titleRect,
            lighterBlue,
            _currentTheme.Primary,
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

        // Draw footer background (gradient like title bar)
        int footerTop = this.Height - TitleBarHeight - FooterHeight;
        var footerRect = new Rectangle(0, TitleBarHeight + footerTop, this.Width, FooterHeight);
        using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            footerRect,
            _currentTheme.Primary,
            ControlPaint.Dark(_currentTheme.Primary, 0.1f),
            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
        {
            g.FillRectangle(gradientBrush, footerRect);
        }

        // Draw brass accent line above footer (matching title bar style)
        using (var pen = new Pen(_currentTheme.Accent, 2))
        {
            g.DrawLine(pen, 0, TitleBarHeight + footerTop, this.Width, TitleBarHeight + footerTop);
        }

        // Draw close button
        DrawRoundCloseButton(g);
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

    #region Button Events

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

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using (var folderDialog = new FolderBrowserDialog())
        {
            folderDialog.Description = "Select a folder to scan";
            folderDialog.ShowNewFolderButton = false;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _pathTextBox.Text = folderDialog.SelectedPath;
            }
        }
    }

    private async void ScanButton_Click(object? sender, EventArgs e)
    {
        string path = _pathTextBox.Text.Trim();

        if (string.IsNullOrEmpty(path))
        {
            MessageBox.Show("Please enter or browse to a folder path.", "No Path Specified",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Directory.Exists(path))
        {
            MessageBox.Show("The specified directory does not exist.", "Invalid Path",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Clear previous results
        _allItems.Clear();
        _dataSource.Clear();
        _cachedColumnValues.Clear();
        _activeFilters.Clear();

        // Initialize cached column values
        _cachedColumnValues["FullPath"] = new HashSet<string>();
        _cachedColumnValues["Name"] = new HashSet<string>();
        _cachedColumnValues["Type"] = new HashSet<string>();
        _cachedColumnValues["DateModified"] = new HashSet<string>();
        _cachedColumnValues["Size"] = new HashSet<string>();

        // Disable scan button during scan
        _scanButton.Enabled = false;
        _scanButton.Text = "Scanning...";
        this.Cursor = Cursors.WaitCursor;

        try
        {
            await Task.Run(() => ScanDirectory(path));
            
            // After scan, populate the grid
            foreach (var item in _allItems)
            {
                _dataSource.Add(item);
            }

            // Update record count
            UpdateRecordCount();

            MessageBox.Show($"Scan complete! Found {_allItems.Count} items.", "Scan Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error scanning directory: {ex.Message}", "Scan Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _scanButton.Enabled = true;
            _scanButton.Text = "Scan";
            this.Cursor = Cursors.Default;
        }
    }

    #endregion

    #region Directory Scanning

    private void ScanDirectory(string path)
    {
        try
        {
            // Scan directories
            var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            foreach (var dir in directories)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var item = new FileSystemItem
                    {
                        FullPath = dir,
                        Name = dirInfo.Name,
                        Type = "Folder",
                        DateModified = dirInfo.LastWriteTime,
                        Size = ""
                    };

                    // Cache values for filtering
                    _cachedColumnValues["FullPath"].Add(item.FullPath);
                    _cachedColumnValues["Name"].Add(item.Name);
                    _cachedColumnValues["Type"].Add(item.Type);
                    _cachedColumnValues["DateModified"].Add(item.DateModified.ToString("yyyy-MM-dd HH:mm:ss"));
                    _cachedColumnValues["Size"].Add(item.Size);

                    _allItems.Add(item);
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we don't have access to
                }
            }

            // Scan files
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    string fileType = "File";

                    // Check if it's a link/shortcut
                    if (fileInfo.Extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        fileType = "Link";
                    }

                    string fileSize = FormatFileSize(fileInfo.Length);

                    var item = new FileSystemItem
                    {
                        FullPath = file,
                        Name = fileInfo.Name,
                        Type = fileType,
                        DateModified = fileInfo.LastWriteTime,
                        Size = fileSize
                    };

                    // Cache values for filtering
                    _cachedColumnValues["FullPath"].Add(item.FullPath);
                    _cachedColumnValues["Name"].Add(item.Name);
                    _cachedColumnValues["Type"].Add(item.Type);
                    _cachedColumnValues["DateModified"].Add(item.DateModified.ToString("yyyy-MM-dd HH:mm:ss"));
                    _cachedColumnValues["Size"].Add(item.Size);

                    _allItems.Add(item);
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip files we don't have access to
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip the entire directory if we don't have access
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    #endregion

    #region Filtering

    private void ResultsGrid_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            var column = _resultsGrid.Columns[e.ColumnIndex];
            ShowFilterMenu(column, e.ColumnIndex);
        }
    }

    private void ResultsGrid_ColumnHeaderMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        // Double-click any header to clear ALL filters
        ClearAllFilters();
    }

    private void ShowFilterMenu(DataGridViewColumn column, int columnIndex)
    {
        string columnName = column.DataPropertyName;
        if (!_cachedColumnValues.ContainsKey(columnName))
        {
            return;
        }

        // Check if a filter window is already open for this column
        if (_openFilterWindows.ContainsKey(columnName))
        {
            var existingWindow = _openFilterWindows[columnName];
            if (existingWindow != null && !existingWindow.IsDisposed)
            {
                // Bring existing window to front
                existingWindow.BringToFront();
                existingWindow.Activate();
                return;
            }
            else
            {
                // Window was closed, remove from tracking
                _openFilterWindows.Remove(columnName);
            }
        }

        // CUMULATIVE FILTERING: Get only the values that exist in rows passing all OTHER filters
        // This is how Excel works - filters are cascading/cumulative
        var availableValues = GetAvailableValuesForColumn(columnName);

        // Get current filter selection for this column
        HashSet<string>? currentSelection = _activeFilters.ContainsKey(columnName) 
            ? _activeFilters[columnName] 
            : null;

        // Create filter window with SuiteView look and feel
        var filterContent = new FilterWindowContentForm(_currentTheme, columnName, availableValues, currentSelection);
        
        // Wire up events
        filterContent.FilterApplied += (s, e) =>
        {
            if (e.SelectedValues == null)
            {
                // No filter (all selected or cleared)
                _activeFilters.Remove(e.ColumnName);
            }
            else
            {
                // Apply filter
                _activeFilters[e.ColumnName] = e.SelectedValues;
            }
            ApplyFilters();
        };

        // Create bordered window
        var filterWindow = new BorderedWindowForm(
            _currentTheme,
            initialSize: new Size(350, 450),
            minimumSize: new Size(250, 300)
        );
        filterWindow.Text = $"Filter: {columnName}";
        filterWindow.SetContentForm(filterContent);

        // Track this window
        _openFilterWindows[columnName] = filterWindow;

        // Remove from tracking when closed
        filterWindow.FormClosed += (s, e) =>
        {
            _openFilterWindows.Remove(columnName);
        };

        // Position window right below the column header
        var headerCell = _resultsGrid.GetCellDisplayRectangle(columnIndex, -1, true);
        var screenPoint = _resultsGrid.PointToScreen(new Point(headerCell.Left, headerCell.Bottom));
        filterWindow.StartPosition = FormStartPosition.Manual;
        filterWindow.Location = screenPoint;

        // Show as dialog so it stays on top
        filterWindow.Show();
    }

    /// <summary>
    /// Gets the available values for a column based on all OTHER active filters.
    /// This implements Excel-style cumulative filtering.
    /// </summary>
    private HashSet<string> GetAvailableValuesForColumn(string columnName)
    {
        var availableValues = new HashSet<string>();

        // Get all active filters EXCEPT the one for this column
        var tempFilters = new Dictionary<string, HashSet<string>>(_activeFilters);
        tempFilters.Remove(columnName);

        // If no other filters are active, return all cached values for this column
        if (tempFilters.Count == 0)
        {
            return _cachedColumnValues[columnName];
        }

        // Apply all OTHER filters to find which items are visible
        foreach (var item in _allItems)
        {
            bool matchesOtherFilters = true;

            // Check if this item passes all OTHER filters
            foreach (var filter in tempFilters)
            {
                string filterColumnName = filter.Key;
                var allowedValues = filter.Value;

                string itemValue = filterColumnName switch
                {
                    "FullPath" => item.FullPath,
                    "Name" => item.Name,
                    "Type" => item.Type,
                    "DateModified" => item.DateModified.ToString("yyyy-MM-dd HH:mm:ss"),
                    "Size" => item.Size,
                    _ => string.Empty
                };

                if (!allowedValues.Contains(itemValue))
                {
                    matchesOtherFilters = false;
                    break;
                }
            }

            // If this item passes all other filters, include its value for THIS column
            if (matchesOtherFilters)
            {
                string value = columnName switch
                {
                    "FullPath" => item.FullPath,
                    "Name" => item.Name,
                    "Type" => item.Type,
                    "DateModified" => item.DateModified.ToString("yyyy-MM-dd HH:mm:ss"),
                    "Size" => item.Size,
                    _ => string.Empty
                };
                availableValues.Add(value);
            }
        }

        return availableValues;
    }

    private void ApplyFilters()
    {
        _dataSource.Clear();

        if (_activeFilters.Count == 0)
        {
            // No filters, show all items
            foreach (var item in _allItems)
            {
                _dataSource.Add(item);
            }
        }
        else
        {
            // Filter items
            foreach (var item in _allItems)
            {
                bool matches = true;

                foreach (var filter in _activeFilters)
                {
                    string columnName = filter.Key;
                    var allowedValues = filter.Value;

                    string itemValue = columnName switch
                    {
                        "FullPath" => item.FullPath,
                        "Name" => item.Name,
                        "Type" => item.Type,
                        "DateModified" => item.DateModified.ToString("yyyy-MM-dd HH:mm:ss"),
                        "Size" => item.Size,
                        _ => ""
                    };

                    if (!allowedValues.Contains(itemValue))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    _dataSource.Add(item);
                }
            }
        }

        // Update record count display
        UpdateRecordCount();
    }

    private void UpdateRecordCount()
    {
        int displayedCount = _dataSource.Count;
        int totalCount = _allItems.Count;

        if (displayedCount == totalCount)
        {
            _recordCountLabel.Text = $"{totalCount:N0} records displayed";
        }
        else
        {
            _recordCountLabel.Text = $"{displayedCount:N0} of {totalCount:N0} records displayed";
        }
    }

    private void ClearAllButton_Click(object? sender, EventArgs e)
    {
        ClearAllFilters();
    }

    private void ExportButton_Click(object? sender, EventArgs e)
    {
        if (_dataSource.Count == 0)
        {
            MessageBox.Show("No data to export. Please scan a directory first.", 
                "No Data", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            // Create a temporary file for the workbook
            string tempFile = Path.Combine(Path.GetTempPath(), $"DirectoryScan_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            // Create workbook using ClosedXML
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Directory Scan");

                // Write headers
                worksheet.Cell(1, 1).Value = "Full Path";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Type";
                worksheet.Cell(1, 4).Value = "Date Modified";
                worksheet.Cell(1, 5).Value = "Size";

                // Format header row (brass background with blue text)
                var headerRow = worksheet.Range(1, 1, 1, 5);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(_currentTheme.Accent.R, _currentTheme.Accent.G, _currentTheme.Accent.B);
                headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromArgb(_currentTheme.Primary.R, _currentTheme.Primary.G, _currentTheme.Primary.B);
                headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;

                // Write data (from filtered data source)
                int row = 2;
                foreach (var item in _dataSource)
                {
                    worksheet.Cell(row, 1).Value = item.FullPath;
                    worksheet.Cell(row, 2).Value = item.Name;
                    worksheet.Cell(row, 3).Value = item.Type;
                    worksheet.Cell(row, 4).Value = item.DateModified.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cell(row, 5).Value = item.Size;
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Add autofilter to header row
                var usedRange = worksheet.RangeUsed();
                if (usedRange != null)
                {
                    usedRange.SetAutoFilter();
                }

                // Freeze header row
                worksheet.SheetView.FreezeRows(1);

                // Save to temp file
                workbook.SaveAs(tempFile);
            }

            // Open the Excel file directly - Excel will treat it as an unsaved workbook
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting to Excel: {ex.Message}", 
                "Export Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);
        }
    }

    private void ClearAllFilters()
    {
        _activeFilters.Clear();
        ApplyFilters();
    }

    #endregion

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateLayout();
        this.Invalidate();
    }
}
