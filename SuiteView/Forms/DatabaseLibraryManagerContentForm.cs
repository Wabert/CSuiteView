using SuiteView.Managers;
using SuiteView.Models;
using SuiteView.Services;

namespace SuiteView.Forms;

/// <summary>
/// Database Library Manager - Tool for scanning and managing database table metadata
/// </summary>
public partial class DatabaseLibraryManagerContentForm : Form, IContentForm
{
    private readonly ColorTheme _currentTheme;
    private Form? _parentBorderForm;
    
    // UI Controls
    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Panel _contentPanel = null!;
    private TreeView _treeView = null!;
    private ListView _tablesListView = null!;
    private ListView _fieldsListView = null!;
    private Button _scanButton = null!;
    private Button _moveToLibraryButton = null!;
    private Button _getUniqueValuesButton = null!;
    private Button _exportFieldsButton = null!;
    private Label _tableNameLabel = null!;
    
    // Query Builder Controls
    private Panel _newQueryPanel = null!;
    private Label _newQueryTitleLabel = null!;
    private TextBox _queryNameTextBox = null!;
    private Button _newQueryButton = null!;
    private Panel _queryBuilderPanel = null!;
    private Label _queryBuilderTitleLabel = null!;
    private Button _saveQueryButton = null!;
    private TabControl _queryTabControl = null!;
    private TabPage _criteriaTab = null!;
    private TabPage _displayTab = null!;
    private TabPage _sqlTab = null!;
    private TabPage _resultsTab = null!;
    private FlowLayoutPanel _criteriaPanel = null!;
    private FlowLayoutPanel _displayPanel = null!;
    
    // Data
    private DatabaseLibraryConfig _config = null!;
    private TreeNode? _lifeProdNode;
    private TreeNode? _myLibraryNode;
    private TreeNode? _myQueriesNode;
    private TableMetadata? _currentTable;
    private TreeNode? _currentQueryNode;
    private QueryDefinition? _currentQuery;
    private Dictionary<string, (List<Control> criteriaControls, List<Control> displayControls)> _queryControlsCache = new();
    
    // Layout constants
    private const int TitleBarHeight = 35;
    private const int FooterHeight = 35;
    private const int ControlMargin = 10;
    private const int TreeViewWidth = 250;
    
    // Dragging support
    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _dragParentStart;

    public DatabaseLibraryManagerContentForm(ColorTheme theme)
    {
        _currentTheme = theme;
        
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();

        InitializeComponent();
        SetupForm();
        ApplyTheme();
        LoadConfiguration();
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
            Text = "Database Library Manager",
            ForeColor = _currentTheme.Accent,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Location = new Point(12, 6),
            AutoSize = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.SizeAll
        };

        // Content panel
        _contentPanel = new Panel
        {
            Location = new Point(0, TitleBarHeight),
            AutoScroll = false,
            BackColor = _currentTheme.Secondary
        };

        // TreeView (left side)
        _treeView = new TreeView
        {
            BackColor = _currentTheme.Secondary,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10f),
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            FullRowSelect = true
        };
        _treeView.AfterSelect += TreeView_AfterSelect;

        // Tables ListView (right side - for showing tables)
        _tablesListView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = true,
            CheckBoxes = true,
            BackColor = _currentTheme.Secondary,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9f)
        };
        
        // Add columns to Tables ListView
        _tablesListView.Columns.Add("Table Name", 300);
        _tablesListView.Columns.Add("Last Scanned", 200);
        _tablesListView.Columns.Add("Fields", 100);

        // Fields ListView (right side - for showing fields of a selected table)
        _fieldsListView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = true,
            CheckBoxes = true,
            BackColor = _currentTheme.Secondary,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9f),
            Visible = false
        };
        
        // Add columns to Fields ListView
        _fieldsListView.Columns.Add("Field Name", 250);
        _fieldsListView.Columns.Add("Data Type", 150);
        _fieldsListView.Columns.Add("Unique Values Scanned", 180);
        _fieldsListView.Columns.Add("Unique Values", 300);

        // Scan button
        _scanButton = new Button
        {
            Text = "Scan Selected",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(120, 26),
            Enabled = false
        };
        _scanButton.FlatAppearance.BorderSize = 0;
        _scanButton.Click += ScanButton_Click;

        // Move to Library button
        _moveToLibraryButton = new Button
        {
            Text = "Move to Library",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 139, 34),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(120, 26),
            Enabled = false
        };
        _moveToLibraryButton.FlatAppearance.BorderSize = 0;
        _moveToLibraryButton.Click += MoveToLibraryButton_Click;

        // Get Unique Values button (brass colored)
        _getUniqueValuesButton = new Button
        {
            Text = "Get Unique Values",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(140, 26),
            Enabled = false,
            Visible = false
        };
        _getUniqueValuesButton.FlatAppearance.BorderSize = 0;
        _getUniqueValuesButton.Click += GetUniqueValuesButton_Click;

        // Export Fields button (green)
        _exportFieldsButton = new Button
        {
            Text = "Export",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 139, 34),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(80, 26),
            Enabled = false,
            Visible = false
        };
        _exportFieldsButton.FlatAppearance.BorderSize = 0;
        _exportFieldsButton.Click += ExportFieldsButton_Click;

        // Table Name Label (shown in fields view)
        _tableNameLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = _currentTheme.Accent,
            AutoSize = true,
            BackColor = Color.Transparent,
            Visible = false
        };

        // New Query Panel (shown when My_Queries is selected)
        _newQueryPanel = new Panel
        {
            BackColor = _currentTheme.Secondary,
            Visible = false
        };

        _newQueryTitleLabel = new Label
        {
            Text = "My_Queries",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };

        _queryNameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10f),
            BackColor = Color.White,
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Size = new Size(250, 25)
        };

        _newQueryButton = new Button
        {
            Text = "New Query",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(100, 26)
        };
        _newQueryButton.FlatAppearance.BorderSize = 0;
        _newQueryButton.Click += NewQueryButton_Click;

        _newQueryPanel.Controls.Add(_newQueryTitleLabel);
        _newQueryPanel.Controls.Add(_queryNameTextBox);
        _newQueryPanel.Controls.Add(_newQueryButton);

        // Query Builder Panel (shown when building a query)
        _queryBuilderPanel = new Panel
        {
            BackColor = _currentTheme.Secondary,
            Visible = false
        };

        _queryBuilderTitleLabel = new Label
        {
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };

        _saveQueryButton = new Button
        {
            Text = "Save",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(80, 26)
        };
        _saveQueryButton.FlatAppearance.BorderSize = 0;
        _saveQueryButton.Click += SaveQueryButton_Click;

        // Tab Control
        _queryTabControl = new TabControl
        {
            Font = new Font("Segoe UI", 9f)
        };

        _criteriaTab = new TabPage("Criteria");
        _displayTab = new TabPage("Display");
        _sqlTab = new TabPage("SQL");
        _resultsTab = new TabPage("Results");

        _queryTabControl.TabPages.Add(_criteriaTab);
        _queryTabControl.TabPages.Add(_displayTab);
        _queryTabControl.TabPages.Add(_sqlTab);
        _queryTabControl.TabPages.Add(_resultsTab);

        // Criteria Panel (drop zone for filter fields)
        _criteriaPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            AutoScroll = true,
            AllowDrop = true,
            Dock = DockStyle.Fill
        };
        _criteriaPanel.DragEnter += FieldsDropZone_DragEnter;
        _criteriaPanel.DragDrop += CriteriaDropZone_DragDrop;

        _criteriaTab.Controls.Add(_criteriaPanel);

        // Display Panel (drop zone for select fields)
        _displayPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            AutoScroll = true,
            AllowDrop = true,
            Dock = DockStyle.Fill
        };
        _displayPanel.DragEnter += FieldsDropZone_DragEnter;
        _displayPanel.DragDrop += DisplayDropZone_DragDrop;

        _displayTab.Controls.Add(_displayPanel);

        _queryBuilderPanel.Controls.Add(_queryBuilderTitleLabel);
        _queryBuilderPanel.Controls.Add(_saveQueryButton);
        _queryBuilderPanel.Controls.Add(_queryTabControl);

        // Add controls to content panel
        _contentPanel.Controls.Add(_treeView);
        _contentPanel.Controls.Add(_tablesListView);
        _contentPanel.Controls.Add(_fieldsListView);
        _contentPanel.Controls.Add(_newQueryPanel);
        _contentPanel.Controls.Add(_queryBuilderPanel);
        _contentPanel.Controls.Add(_scanButton);
        _contentPanel.Controls.Add(_moveToLibraryButton);
        _contentPanel.Controls.Add(_getUniqueValuesButton);
        _contentPanel.Controls.Add(_exportFieldsButton);
        _contentPanel.Controls.Add(_tableNameLabel);

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

        // TreeView on the left
        _treeView.Location = new Point(ControlMargin, ControlMargin);
        _treeView.Size = new Size(TreeViewWidth, _contentPanel.Height - (ControlMargin * 2) - FooterHeight);

        // Right panel area (for ListViews)
        int rightPanelX = TreeViewWidth + (ControlMargin * 2);
        int rightPanelWidth = _contentPanel.Width - TreeViewWidth - (ControlMargin * 3);
        int rightPanelHeight = _contentPanel.Height - (ControlMargin * 2) - FooterHeight;

        // Tables ListView (on the right when showing tables)
        _tablesListView.Location = new Point(rightPanelX, ControlMargin);
        _tablesListView.Size = new Size(rightPanelWidth, rightPanelHeight);

        // Fields ListView (on the right when showing fields)
        // Button and label area at top
        int fieldsListViewY = ControlMargin + 35;
        _fieldsListView.Location = new Point(rightPanelX, fieldsListViewY);
        _fieldsListView.Size = new Size(rightPanelWidth, rightPanelHeight - 35);

        // Table Name Label (top left of right panel when showing fields)
        _tableNameLabel.Location = new Point(rightPanelX, ControlMargin + 5);

        // Export Fields button (top right, first button)
        _exportFieldsButton.Location = new Point(
            rightPanelX + rightPanelWidth - _exportFieldsButton.Width,
            ControlMargin
        );

        // Get Unique Values button (top right, next to Export)
        _getUniqueValuesButton.Location = new Point(
            _exportFieldsButton.Left - _getUniqueValuesButton.Width - 5,
            ControlMargin
        );

        // New Query Panel layout
        _newQueryPanel.Location = new Point(rightPanelX, ControlMargin);
        _newQueryPanel.Size = new Size(rightPanelWidth, rightPanelHeight);
        _newQueryTitleLabel.Location = new Point(10, 10);
        _queryNameTextBox.Location = new Point(10, 40);
        _newQueryButton.Location = new Point(_queryNameTextBox.Right + 10, 39);

        // Query Builder Panel layout
        _queryBuilderPanel.Location = new Point(rightPanelX, ControlMargin);
        _queryBuilderPanel.Size = new Size(rightPanelWidth, rightPanelHeight);
        _queryBuilderTitleLabel.Location = new Point(10, 10);
        _saveQueryButton.Location = new Point(rightPanelWidth - _saveQueryButton.Width - 10, 9);
        _queryTabControl.Location = new Point(0, 40);
        _queryTabControl.Size = new Size(rightPanelWidth, rightPanelHeight - 40);

        // Buttons at the bottom right
        int buttonY = _contentPanel.Height - FooterHeight + 5;
        _moveToLibraryButton.Location = new Point(
            _contentPanel.Width - _moveToLibraryButton.Width - ControlMargin,
            buttonY
        );
        _scanButton.Location = new Point(
            _moveToLibraryButton.Left - _scanButton.Width - 5,
            buttonY
        );
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;
        _contentPanel.BackColor = _currentTheme.Secondary;
        _treeView.BackColor = _currentTheme.Secondary;
        _treeView.ForeColor = Color.White;
        _tablesListView.BackColor = _currentTheme.Secondary;
        _tablesListView.ForeColor = Color.White;
        _fieldsListView.BackColor = _currentTheme.Secondary;
        _fieldsListView.ForeColor = Color.White;
    }

    private void LoadConfiguration()
    {
        _config = DatabaseLibraryManager.LoadConfig();
        
        // Initialize tree view
        _treeView.Nodes.Clear();
        
        // Add LifeProd_Library node
        _lifeProdNode = new TreeNode("LifeProd_Library")
        {
            Tag = "database_source"
        };
        _treeView.Nodes.Add(_lifeProdNode);
        
        // Add My_Library node
        _myLibraryNode = new TreeNode("My_Library")
        {
            Tag = "user_library"
        };
        _treeView.Nodes.Add(_myLibraryNode);
        
        // Add My_Queries node (sibling to My_Library)
        _myQueriesNode = new TreeNode("My_Queries")
        {
            Tag = "user_queries"
        };
        _treeView.Nodes.Add(_myQueriesNode);
        
        // Load existing library tables
        LoadMyLibrary();
        
        // Load saved queries
        LoadQueries();
    }

    private void LoadMyLibrary()
    {
        if (_myLibraryNode == null) return;
        
        // Clear all table nodes from My_Library
        _myLibraryNode.Nodes.Clear();
        
        var lifeProdDb = _config.Databases.FirstOrDefault(d => d.Name == "LifeProd_Library");
        if (lifeProdDb != null)
        {
            foreach (var table in lifeProdDb.Tables.Where(t => t.IsInLibrary))
            {
                var tableNode = new TreeNode(table.TableName)
                {
                    Tag = table
                };
                
                // Add field nodes
                foreach (var field in table.Fields)
                {
                    var fieldNode = new TreeNode($"{field.FieldName} ({field.DataType})")
                    {
                        Tag = field
                    };
                    tableNode.Nodes.Add(fieldNode);
                }
                
                _myLibraryNode.Nodes.Add(tableNode);
            }
        }
    }

    private void LoadQueries()
    {
        if (_myQueriesNode == null) return;
        
        // Clear existing query nodes
        _myQueriesNode.Nodes.Clear();
        
        // Load all saved queries
        foreach (var query in _config.Queries)
        {
            var queryNode = new TreeNode(query.Name)
            {
                Tag = query
            };
            _myQueriesNode.Nodes.Add(queryNode);
        }
        
        // Expand My_Queries if there are queries
        if (_config.Queries.Count > 0)
        {
            _myQueriesNode.Expand();
        }
    }

    private async void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node == _lifeProdNode)
        {
            // Show tables view
            ShowTablesView();
            await LoadDatabaseTablesAsync();
        }
        else if (e.Node == _myLibraryNode)
        {
            // Show empty tables view
            ShowTablesView();
            _tablesListView.Items.Clear();
            _scanButton.Enabled = false;
            _moveToLibraryButton.Enabled = false;
        }
        else if (e.Node == _myQueriesNode)
        {
            // Show new query panel
            ShowNewQueryView();
        }
        else if (e.Node?.Parent == _myQueriesNode)
        {
            // User selected a query node
            ShowQueryBuilderView(e.Node);
        }
        else if (e.Node?.Parent == _myLibraryNode && e.Node.Tag is TableMetadata table)
        {
            // Show fields view for the selected table
            ShowFieldsView(table);
        }
    }

    private void ShowTablesView()
    {
        _tablesListView.Visible = true;
        _fieldsListView.Visible = false;
        _newQueryPanel.Visible = false;
        _queryBuilderPanel.Visible = false;
        _scanButton.Visible = true;
        _moveToLibraryButton.Visible = true;
        _getUniqueValuesButton.Visible = false;
        _exportFieldsButton.Visible = false;
        _tableNameLabel.Visible = false;
        _currentTable = null;
    }

    private void ShowNewQueryView()
    {
        _tablesListView.Visible = false;
        _fieldsListView.Visible = false;
        _newQueryPanel.Visible = true;
        _queryBuilderPanel.Visible = false;
        _scanButton.Visible = false;
        _moveToLibraryButton.Visible = false;
        _getUniqueValuesButton.Visible = false;
        _exportFieldsButton.Visible = false;
        _tableNameLabel.Visible = false;
        _queryNameTextBox.Clear();
    }

    private void ShowQueryBuilderView(TreeNode queryNode)
    {
        _currentQueryNode = queryNode;
        _currentQuery = queryNode.Tag as QueryDefinition;
        
        _tablesListView.Visible = false;
        _fieldsListView.Visible = false;
        _newQueryPanel.Visible = false;
        _queryBuilderPanel.Visible = true;
        _scanButton.Visible = false;
        _moveToLibraryButton.Visible = false;
        _getUniqueValuesButton.Visible = false;
        _exportFieldsButton.Visible = false;
        _tableNameLabel.Visible = false;
        
        // Update title
        _queryBuilderTitleLabel.Text = $"My_Queries → {queryNode.Text}";
        
        // Clear both panels
        _criteriaPanel.Controls.Clear();
        _displayPanel.Controls.Clear();
        
        // Load saved criteria and display fields if query exists
        if (_currentQuery != null)
        {
            LoadQueryFields(_currentQuery);
        }
        
        // Enable drag-drop on TreeView fields (remove first to prevent duplicates)
        _treeView.ItemDrag -= TreeView_ItemDrag;
        _treeView.ItemDrag += TreeView_ItemDrag;
    }

    private void ShowFieldsView(TableMetadata table)
    {
        _currentTable = table;
        
        _tablesListView.Visible = false;
        _fieldsListView.Visible = true;
        _newQueryPanel.Visible = false;
        _queryBuilderPanel.Visible = false;
        _scanButton.Visible = false;
        _moveToLibraryButton.Visible = false;
        _getUniqueValuesButton.Visible = true;
        _getUniqueValuesButton.Enabled = true;
        _exportFieldsButton.Visible = true;
        _exportFieldsButton.Enabled = true;
        _tableNameLabel.Visible = true;
        _tableNameLabel.Text = table.TableName;

        // Populate fields ListView
        _fieldsListView.Items.Clear();

        foreach (var field in table.Fields)
        {
            var item = new ListViewItem(field.FieldName);
            item.SubItems.Add(field.DataType + (field.MaxLength.HasValue ? $"({field.MaxLength})" : ""));
            
            // Unique values scanned date
            item.SubItems.Add(field.UniqueValuesScannedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never");
            
            // Unique values display
            string uniqueValuesDisplay;
            if (field.HasMoreThan200UniqueValues)
            {
                // Show count if we have it stored
                uniqueValuesDisplay = $"More than 200 unique values ({field.UniqueValuesCount ?? 0:N0})";
            }
            else if (field.UniqueValues != null && field.UniqueValues.Count > 0)
            {
                uniqueValuesDisplay = string.Join(", ", field.UniqueValues);
            }
            else
            {
                uniqueValuesDisplay = "";
            }
            item.SubItems.Add(uniqueValuesDisplay);
            
            item.Tag = new { Table = table, Field = field };
            _fieldsListView.Items.Add(item);
        }
    }

    private async Task LoadDatabaseTablesAsync()
    {
        try
        {
            _tablesListView.Items.Clear();
            _scanButton.Enabled = false;
            _moveToLibraryButton.Enabled = false;

            Cursor = Cursors.WaitCursor;

            // Get or create database config
            var lifeProdDb = DatabaseLibraryManager.GetOrCreateDatabase(_config, "LifeProd_Library", "UL_Rates", "UL_Rates");

            // Fetch table names from database
            var tableNames = await DatabaseService.GetTableNamesAsync("UL_Rates", "UL_Rates");

            // Display tables in ListView
            foreach (var tableName in tableNames)
            {
                var existingTable = lifeProdDb.Tables.FirstOrDefault(t => t.TableName == tableName);
                if (existingTable == null)
                {
                    existingTable = new TableMetadata { TableName = tableName };
                    lifeProdDb.Tables.Add(existingTable);
                }

                var item = new ListViewItem(tableName);
                item.SubItems.Add(existingTable.LastScanned?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never");
                item.SubItems.Add(existingTable.Fields.Count.ToString());
                item.Tag = existingTable;
                _tablesListView.Items.Add(item);
            }

            _scanButton.Enabled = true;
            _moveToLibraryButton.Enabled = true;

            // Save configuration
            DatabaseLibraryManager.SaveConfig(_config);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tables: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async void ScanButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = _tablesListView.CheckedItems.Cast<ListViewItem>().ToList();
        if (selectedItems.Count == 0)
        {
            MessageBox.Show("Please select tables to scan by checking their checkboxes.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            _scanButton.Enabled = false;

            foreach (ListViewItem item in selectedItems)
            {
                var table = (TableMetadata)item.Tag;
                
                // Scan table fields
                var fields = await DatabaseService.GetTableFieldsAsync("UL_Rates", "UL_Rates", table.TableName);
                table.Fields = fields;
                table.LastScanned = DateTime.Now;

                // Update ListView
                item.SubItems[1].Text = table.LastScanned.Value.ToString("yyyy-MM-dd HH:mm:ss");
                item.SubItems[2].Text = table.Fields.Count.ToString();
            }

            // Save configuration
            DatabaseLibraryManager.SaveConfig(_config);

            MessageBox.Show($"Successfully scanned {selectedItems.Count} table(s)!", "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error scanning tables: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _scanButton.Enabled = true;
        }
    }

    private void MoveToLibraryButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = _tablesListView.CheckedItems.Cast<ListViewItem>().ToList();
        if (selectedItems.Count == 0)
        {
            MessageBox.Show("Please select tables to move by checking their checkboxes.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Mark tables as in library
        foreach (ListViewItem item in selectedItems)
        {
            var table = (TableMetadata)item.Tag;
            if (!table.IsInLibrary && table.LastScanned.HasValue)
            {
                table.IsInLibrary = true;
            }
            else if (!table.LastScanned.HasValue)
            {
                MessageBox.Show($"Table '{table.TableName}' must be scanned before moving to library.", "Not Scanned", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // Save configuration
        DatabaseLibraryManager.SaveConfig(_config);

        // Reload My_Library
        LoadMyLibrary();
        _myLibraryNode?.Expand();

        MessageBox.Show($"Successfully moved {selectedItems.Count} table(s) to library!", "Move Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void GetUniqueValuesButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = _fieldsListView.CheckedItems.Cast<ListViewItem>().ToList();
        if (selectedItems.Count == 0)
        {
            MessageBox.Show("Please select fields to analyze by checking their checkboxes.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            _getUniqueValuesButton.Enabled = false;

            foreach (ListViewItem item in selectedItems)
            {
                dynamic tagData = item.Tag!;
                TableMetadata table = tagData.Table;
                FieldMetadata field = tagData.Field;

                // Get unique values from database
                var (uniqueValues, hasMoreThan200, totalCount) = await DatabaseService.GetUniqueValuesAsync(
                    "UL_Rates", 
                    "UL_Rates", 
                    table.TableName, 
                    field.FieldName
                );

                // Update field metadata
                field.UniqueValuesScannedDate = DateTime.Now;
                field.HasMoreThan200UniqueValues = hasMoreThan200;
                field.UniqueValuesCount = totalCount;
                
                if (hasMoreThan200)
                {
                    field.UniqueValues = null;
                    item.SubItems[3].Text = $"More than 200 unique values ({totalCount:N0})";
                }
                else
                {
                    field.UniqueValues = uniqueValues;
                    item.SubItems[3].Text = string.Join(", ", uniqueValues);
                }

                // Update scanned date
                item.SubItems[2].Text = field.UniqueValuesScannedDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Auto-uncheck the field since it's done
                item.Checked = false;
            }

            // Save configuration
            DatabaseLibraryManager.SaveConfig(_config);

            MessageBox.Show($"Successfully analyzed {selectedItems.Count} field(s)!", "Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing fields: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _getUniqueValuesButton.Enabled = true;
        }
    }

    private void ExportFieldsButton_Click(object? sender, EventArgs e)
    {
        if (_currentTable == null)
        {
            MessageBox.Show("No table selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add(_currentTable.TableName);

            // Add headers
            worksheet.Cell(1, 1).Value = "Field Name";
            worksheet.Cell(1, 2).Value = "Data Type";
            worksheet.Cell(1, 3).Value = "Unique Values Scanned";
            worksheet.Cell(1, 4).Value = "Unique Values";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

            // Add data
            int row = 2;
            foreach (var field in _currentTable.Fields)
            {
                worksheet.Cell(row, 1).Value = field.FieldName;
                worksheet.Cell(row, 2).Value = field.DataType + (field.MaxLength.HasValue ? $"({field.MaxLength})" : "");
                worksheet.Cell(row, 3).Value = field.UniqueValuesScannedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";
                
                // Unique values display
                string uniqueValuesDisplay;
                if (field.HasMoreThan200UniqueValues)
                {
                    uniqueValuesDisplay = $"More than 200 unique values ({field.UniqueValuesCount ?? 0:N0})";
                }
                else if (field.UniqueValues != null && field.UniqueValues.Count > 0)
                {
                    uniqueValuesDisplay = string.Join(", ", field.UniqueValues);
                }
                else
                {
                    uniqueValuesDisplay = "";
                }
                worksheet.Cell(row, 4).Value = uniqueValuesDisplay;
                
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Save to temp file and open
            string tempPath = Path.Combine(Path.GetTempPath(), $"{_currentTable.TableName}_Fields_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            workbook.SaveAs(tempPath);

            // Open in Excel
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            });

            MessageBox.Show($"Fields exported successfully!\nOpened in Excel.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting fields: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    #region Query Builder Methods

    private void NewQueryButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_queryNameTextBox.Text))
        {
            MessageBox.Show("Please enter a query name.", "No Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string queryName = _queryNameTextBox.Text.Trim();
        
        // Check if query name already exists
        if (_config.Queries.Any(q => q.Name.Equals(queryName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("A query with this name already exists.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        // Create new query definition
        var newQuery = new QueryDefinition
        {
            Name = queryName,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };
        
        // Add to config
        _config.Queries.Add(newQuery);
        
        // Save to JSON
        DatabaseLibraryManager.SaveConfig(_config);
        
        // Create new query node under My_Queries
        var queryNode = new TreeNode(queryName)
        {
            Tag = newQuery  // Store the query definition
        };
        _myQueriesNode?.Nodes.Add(queryNode);
        
        // Expand My_Queries to show the new query
        _myQueriesNode?.Expand();
        
        // Select the new query node
        _treeView.SelectedNode = queryNode;
        
        // This will trigger ShowQueryBuilderView via TreeView_AfterSelect
    }
    
    private void LoadQueryFields(QueryDefinition query)
    {
        // Get reference to LifeProd database for field metadata
        var lifeProdDb = _config.Databases.FirstOrDefault(d => d.Name == "LifeProd_Library");
        if (lifeProdDb == null) return;

        // Load criteria fields
        foreach (var criteriaField in query.CriteriaFields)
        {
            var table = lifeProdDb.Tables.FirstOrDefault(t => t.TableName == criteriaField.TableName);
            if (table == null) continue;

            var field = table.Fields.FirstOrDefault(f => f.FieldName == criteriaField.FieldName);
            if (field == null) continue;

            var tableMetadata = new TableMetadata { TableName = table.TableName };
            var fieldPanel = CreateCriteriaFieldPanelFromSaved(tableMetadata, field, criteriaField);
            _criteriaPanel.Controls.Add(fieldPanel);
        }

        // Load display fields
        foreach (var displayField in query.DisplayFields)
        {
            var table = lifeProdDb.Tables.FirstOrDefault(t => t.TableName == displayField.TableName);
            if (table == null) continue;

            var field = table.Fields.FirstOrDefault(f => f.FieldName == displayField.FieldName);
            if (field == null) continue;

            var tableMetadata = new TableMetadata { TableName = table.TableName };
            CreateDisplayFieldPanel(tableMetadata, field);
        }
    }

    private void SaveQueryButton_Click(object? sender, EventArgs e)
    {
        if (_currentQueryNode == null || _currentQuery == null)
        {
            MessageBox.Show("No query selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Save criteria fields
            _currentQuery.CriteriaFields.Clear();
            foreach (Control control in _criteriaPanel.Controls)
            {
                if (control is RoundedPanel panel && panel.Tag != null)
                {
                    var tag = panel.Tag;
                    var tableProperty = tag.GetType().GetProperty("Table");
                    var fieldProperty = tag.GetType().GetProperty("Field");
                    
                    if (tableProperty == null || fieldProperty == null) continue;
                    
                    var table = tableProperty.GetValue(tag) as TableMetadata;
                    var field = fieldProperty.GetValue(tag) as FieldMetadata;
                    
                    if (table == null || field == null) continue;

                    var criteriaField = new QueryCriteriaField
                    {
                        TableName = table.TableName,
                        FieldName = field.FieldName,
                        DataType = field.DataType,
                        PanelHeight = panel.Height
                    };

                    // Check for CheckedListBox (has unique values)
                    var listBox = panel.Controls.OfType<CheckedListBox>().FirstOrDefault();
                    if (listBox != null)
                    {
                        criteriaField.HasListBox = true;
                        criteriaField.SelectedValues = listBox.CheckedItems.Cast<string>().ToList();
                    }
                    else
                    {
                        // Check for TextBox
                        var textBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
                        if (textBox != null)
                        {
                            criteriaField.HasListBox = false;
                            criteriaField.TextValue = textBox.Text;
                        }
                    }

                    _currentQuery.CriteriaFields.Add(criteriaField);
                }
            }

            // Save display fields
            _currentQuery.DisplayFields.Clear();
            foreach (Control control in _displayPanel.Controls)
            {
                if (control is RoundedPanel panel && panel.Tag != null)
                {
                    var tag = panel.Tag;
                    var tableProperty = tag.GetType().GetProperty("Table");
                    var fieldProperty = tag.GetType().GetProperty("Field");
                    
                    if (tableProperty == null || fieldProperty == null) continue;
                    
                    var table = tableProperty.GetValue(tag) as TableMetadata;
                    var field = fieldProperty.GetValue(tag) as FieldMetadata;
                    
                    if (table == null || field == null) continue;

                    var displayField = new QueryDisplayField
                    {
                        TableName = table.TableName,
                        FieldName = field.FieldName,
                        DataType = field.DataType
                    };

                    _currentQuery.DisplayFields.Add(displayField);
                }
            }

            // Update last modified date
            _currentQuery.LastModifiedDate = DateTime.Now;

            // Save to JSON
            DatabaseLibraryManager.SaveConfig(_config);

            MessageBox.Show($"Query '{_currentQueryNode.Text}' saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving query: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TreeView_ItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (e.Item is TreeNode node)
        {
            // Only allow dragging field nodes (nodes with FieldMetadata tag)
            if (node.Tag is FieldMetadata)
            {
                _treeView.DoDragDrop(node, DragDropEffects.Copy);
            }
        }
    }

    private void FieldsDropZone_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(typeof(TreeNode)) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void CriteriaDropZone_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(typeof(TreeNode)) is TreeNode draggedNode && 
            draggedNode.Tag is FieldMetadata field)
        {
            // Get the table this field belongs to
            var tableNode = draggedNode.Parent;
            if (tableNode?.Tag is not TableMetadata table)
                return;

            // Create a criteria field panel
            CreateCriteriaFieldPanel(table, field);
        }
    }

    private void DisplayDropZone_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(typeof(TreeNode)) is TreeNode draggedNode && 
            draggedNode.Tag is FieldMetadata field)
        {
            // Get the table this field belongs to
            var tableNode = draggedNode.Parent;
            if (tableNode?.Tag is not TableMetadata table)
                return;

            // Create a display field panel
            CreateDisplayFieldPanel(table, field);
        }
    }

    private void CreateCriteriaFieldPanel(TableMetadata table, FieldMetadata field)
    {
        var fieldPanel = new RoundedPanel
        {
            Width = _criteriaPanel.ClientSize.Width - 20,
            Height = 35,
            BackColor = Color.FromArgb(250, 250, 250),
            Margin = new Padding(5, 2, 5, 2),
            BorderColor = Color.FromArgb(220, 220, 220),
            BorderRadius = 8,
            Tag = new { Table = table, Field = field }
        };

        // Remove button (subtle gray circle with ×)
        var removeButton = new Button
        {
            Text = "×",
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            BackColor = Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(22, 22),
            Location = new Point(5, 6),
            Cursor = Cursors.Hand
        };
        removeButton.FlatAppearance.BorderSize = 0;
        removeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 200, 200);
        removeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 150, 150);
        removeButton.Click += (s, e) => _criteriaPanel.Controls.Remove(fieldPanel);
        fieldPanel.Controls.Add(removeButton);

        // Field name label
        var fieldLabel = new Label
        {
            Text = $"{table.TableName}.{field.FieldName}",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Location = new Point(35, 8),
            AutoSize = true
        };
        fieldPanel.Controls.Add(fieldLabel);

        // Check if field has unique values
        if (field.UniqueValues != null && field.UniqueValues.Count > 0 && !field.HasMoreThan200UniqueValues)
        {
            // Create multi-select listbox with resize capability
            var listBox = new CheckedListBox
            {
                Location = new Point(250, 5),
                Size = new Size(fieldPanel.Width - 260, 70),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                IntegralHeight = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Add "(none)" option
            listBox.Items.Add("(none)", false);

            // Add unique values
            foreach (var value in field.UniqueValues)
            {
                listBox.Items.Add(value, false);
            }

            fieldPanel.Height = 80;
            fieldPanel.Controls.Add(listBox);

            // Add resize grip at bottom-right corner of list box
            var resizeGrip = new Label
            {
                Text = "⋮⋮",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.Gray,
                Size = new Size(20, 15),
                Location = new Point(listBox.Right - 20, fieldPanel.Height - 18),
                Cursor = Cursors.SizeNS,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            
            // Mouse events for resizing
            bool isResizing = false;
            int startY = 0;
            int startHeight = 0;

            resizeGrip.MouseDown += (s, e) =>
            {
                isResizing = true;
                startY = Cursor.Position.Y;
                startHeight = fieldPanel.Height;
            };

            resizeGrip.MouseMove += (s, e) =>
            {
                if (isResizing)
                {
                    int deltaY = Cursor.Position.Y - startY;
                    int newHeight = Math.Max(80, startHeight + deltaY);
                    fieldPanel.Height = newHeight;
                    listBox.Height = newHeight - 10;
                }
            };

            resizeGrip.MouseUp += (s, e) =>
            {
                isResizing = false;
            };

            fieldPanel.Controls.Add(resizeGrip);
        }
        else
        {
            // Create regular textbox
            var textBox = new TextBox
            {
                Location = new Point(250, 7),
                Size = new Size(fieldPanel.Width - 260, 23),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
            fieldPanel.Controls.Add(textBox);
        }

        _criteriaPanel.Controls.Add(fieldPanel);
    }

    private RoundedPanel CreateCriteriaFieldPanelFromSaved(TableMetadata table, FieldMetadata field, QueryCriteriaField savedField)
    {
        var fieldPanel = new RoundedPanel
        {
            Width = _criteriaPanel.ClientSize.Width - 20,
            Height = savedField.PanelHeight,
            BackColor = Color.FromArgb(250, 250, 250),
            Margin = new Padding(5, 2, 5, 2),
            BorderColor = Color.FromArgb(220, 220, 220),
            BorderRadius = 8,
            Tag = new { Table = table, Field = field }
        };

        // Remove button
        var removeButton = new Button
        {
            Text = "×",
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            BackColor = Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(22, 22),
            Location = new Point(5, 6),
            Cursor = Cursors.Hand
        };
        removeButton.FlatAppearance.BorderSize = 0;
        removeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 200, 200);
        removeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 150, 150);
        removeButton.Click += (s, e) => _criteriaPanel.Controls.Remove(fieldPanel);
        fieldPanel.Controls.Add(removeButton);

        // Field name label
        var fieldLabel = new Label
        {
            Text = $"{table.TableName}.{field.FieldName}",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Location = new Point(35, 8),
            AutoSize = true
        };
        fieldPanel.Controls.Add(fieldLabel);

        // Restore the control based on saved data
        if (savedField.HasListBox && field.UniqueValues != null && field.UniqueValues.Count > 0)
        {
            var listBox = new CheckedListBox
            {
                Location = new Point(250, 5),
                Size = new Size(fieldPanel.Width - 260, fieldPanel.Height - 10),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                IntegralHeight = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Add "(none)" option
            listBox.Items.Add("(none)", savedField.SelectedValues.Contains("(none)"));

            // Add unique values and restore selections
            foreach (var value in field.UniqueValues)
            {
                bool isChecked = savedField.SelectedValues.Contains(value);
                listBox.Items.Add(value, isChecked);
            }

            fieldPanel.Controls.Add(listBox);

            // Add resize grip
            var resizeGrip = new Label
            {
                Text = "⋮⋮",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.Gray,
                Size = new Size(20, 15),
                Location = new Point(listBox.Right - 20, fieldPanel.Height - 18),
                Cursor = Cursors.SizeNS,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            
            bool isResizing = false;
            int startY = 0;
            int startHeight = 0;

            resizeGrip.MouseDown += (s, e) =>
            {
                isResizing = true;
                startY = Cursor.Position.Y;
                startHeight = fieldPanel.Height;
            };

            resizeGrip.MouseMove += (s, e) =>
            {
                if (isResizing)
                {
                    int deltaY = Cursor.Position.Y - startY;
                    int newHeight = Math.Max(80, startHeight + deltaY);
                    fieldPanel.Height = newHeight;
                    listBox.Height = newHeight - 10;
                }
            };

            resizeGrip.MouseUp += (s, e) =>
            {
                isResizing = false;
            };

            fieldPanel.Controls.Add(resizeGrip);
        }
        else
        {
            // Restore textbox with saved value
            var textBox = new TextBox
            {
                Location = new Point(250, 7),
                Size = new Size(fieldPanel.Width - 260, 23),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
                Text = savedField.TextValue
            };
            fieldPanel.Controls.Add(textBox);
        }

        return fieldPanel;
    }

    private void CreateDisplayFieldPanel(TableMetadata table, FieldMetadata field)
    {
        var fieldPanel = new RoundedPanel
        {
            Width = _displayPanel.ClientSize.Width - 20,
            Height = 35,
            BackColor = Color.FromArgb(250, 250, 250),
            Margin = new Padding(5, 2, 5, 2),
            BorderColor = Color.FromArgb(220, 220, 220),
            BorderRadius = 8,
            Tag = new { Table = table, Field = field }
        };

        // Remove button (subtle gray circle with ×)
        var removeButton = new Button
        {
            Text = "×",
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            BackColor = Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(22, 22),
            Location = new Point(5, 6),
            Cursor = Cursors.Hand
        };
        removeButton.FlatAppearance.BorderSize = 0;
        removeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 200, 200);
        removeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 150, 150);
        removeButton.Click += (s, e) => _displayPanel.Controls.Remove(fieldPanel);
        fieldPanel.Controls.Add(removeButton);

        // Field name label
        var fieldLabel = new Label
        {
            Text = $"{table.TableName}.{field.FieldName}",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Location = new Point(35, 8),
            AutoSize = true
        };
        fieldPanel.Controls.Add(fieldLabel);

        // Data type label
        var dataTypeLabel = new Label
        {
            Text = $"({field.DataType})",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.Gray,
            Location = new Point(fieldLabel.Right + 5, 10),
            AutoSize = true
        };
        fieldPanel.Controls.Add(dataTypeLabel);

        _displayPanel.Controls.Add(fieldPanel);
    }

    #endregion

    #region Painting and UI

    private void ContentForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw gradient title bar
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

        // Draw brass accent line below title bar
        using (var pen = new Pen(_currentTheme.Accent, 2))
        {
            g.DrawLine(pen, 0, TitleBarHeight - 1, this.Width, TitleBarHeight - 1);
        }

        // Draw footer background
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

        // Draw brass accent line above footer
        using (var pen = new Pen(_currentTheme.Accent, 2))
        {
            g.DrawLine(pen, 0, TitleBarHeight + footerTop, this.Width, TitleBarHeight + footerTop);
        }

        DrawRoundCloseButton(g);
    }

    private void DrawRoundCloseButton(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(_closeButton.Location.X + 2, _closeButton.Location.Y + 2, 20, 20);
        
        using (var brush = new SolidBrush(_currentTheme.Accent))
        {
            g.FillEllipse(brush, rect);
        }

        using (var pen = new Pen(_currentTheme.Primary, 2))
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int size = 6;
            g.DrawLine(pen, centerX - size, centerY - size, centerX + size, centerY + size);
            g.DrawLine(pen, centerX + size, centerY - size, centerX - size, centerY + size);
        }
    }

    #endregion

    #region Event Handlers

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
            Point delta = new Point(e.Location.X - _dragStartPoint.X, e.Location.Y - _dragStartPoint.Y);
            _parentBorderForm.Location = new Point(_dragParentStart.X + delta.X, _dragParentStart.Y + delta.Y);
        }
    }

    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateLayout();
        this.Invalidate();
    }

    #endregion
}
