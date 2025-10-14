using System.Data;
using SuiteView.Managers;
using SuiteView.Models;
using SuiteView.Services;
using SuiteView.Forms.Controls;

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
    private TreeView _queriesTreeView = null!;
    private TreeView _dataTreeView = null!;
    private Label _queriesHeaderLabel = null!;
    private Label _dataHeaderLabel = null!;
    private Button _addQueryButton = null!;
    private Button _addDataSourceButton = null!;
    private Button _editModeButton = null!;
    private Label _editModeLabel = null!;
    private ListView _tablesListView = null!;
    private ListView _fieldsListView = null!;
    private Button _scanButton = null!;  // Now "Add" button for tables
    private Button _addTablesButton = null!;  // Add button above tables list
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
    private Button _runQueryButton = null!;
    private TabControl _queryTabControl = null!;
    private TabPage _criteriaTab = null!;
    private TabPage _displayTab = null!;
    private TabPage _tablesTab = null!;
    private TabPage _sqlTab = null!;
    private TabPage _resultsTab = null!;
    private FlowLayoutPanel _criteriaPanel = null!;
    private FlowLayoutPanel _displayPanel = null!;
    private Panel _tablesPanel = null!;
    private Label _tablesInvolvedLabel = null!;
    private FlowLayoutPanel _joinsPanel = null!;
    private Button _addJoinButton = null!;
    
    // Context Menus
    private ContextMenuStrip _treeContextMenu = null!;
    
    // Add Datasource Panel
    private Panel _addDatasourcePanel = null!;
    private Label _addDatasourceTitleLabel = null!;
    private Label _datasourceNameLabel = null!;
    private TextBox _datasourceNameTextBox = null!;
    private Label _odbcNameLabel = null!;
    private TextBox _odbcNameTextBox = null!;
    private Button _addDatasourceButton = null!;
    
    // Data
    private DatabaseLibraryConfig _config = null!;
    private TreeNode? _myQueriesNode;
    private TreeNode? _companyDataNode;
    private TableMetadata? _currentTable;
    private TreeNode? _currentQueryNode;
    private QueryDefinition? _currentQuery;
    // In-memory per-session cache so criteria/display persist without pressing Save
    private readonly Dictionary<string, (List<QueryCriteriaField> criteria, List<QueryDisplayField> display)> _querySessionCache = new();
    // Map datasource display names to their DatabaseConfig
    private readonly Dictionary<TreeNode, DatabaseConfig> _datasourceNodeMap = new();
    // Flag to prevent auto-save during query loading (prevents collection modification errors)
    private bool _isLoadingQuery = false;
    
    // Excel-style filtering support for ListViews
    private readonly Dictionary<string, HashSet<string>> _tablesActiveFilters = new();
    private readonly Dictionary<string, HashSet<string>> _fieldsActiveFilters = new();
    private readonly List<ListViewItem> _allTableItems = new();
    private readonly List<ListViewItem> _allFieldItems = new();
    
    // Layout constants
    private const int TitleBarHeight = 35;
    private const int FooterHeight = 35;
    private const int ControlMargin = 10;
    private const int QueriesTreeWidth = 220;
    private const int DataTreeWidth = 260;
    
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

        // My Queries header button (brass background with blue text, rounded corners like Snap button)
        _queriesHeaderLabel = new Label
        {
            Text = "My Queries",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,  // Blue text
            BackColor = Color.Transparent,   // Transparent for custom painting
            AutoSize = false,
            Size = new Size(100, 20),  // Reduced from 220 to 100
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            Padding = new Padding(5)
        };
        _queriesHeaderLabel.Click += AddQueryButton_Click;  // Make it clickable
        _queriesHeaderLabel.Paint += QueriesHeaderLabel_Paint;  // Custom rounded painting
        
        // Add Query button (will be hidden - header label is now the button)
        _addQueryButton = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(45, 20),
            Visible = false  // Hide this button
        };
        _addQueryButton.FlatAppearance.BorderSize = 0;
        _addQueryButton.Click += AddQueryButton_Click;
        
        _queriesTreeView = new TreeView
        {
            BackColor = _currentTheme.LightBlue,  // Light blue background
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10f),
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            FullRowSelect = true
        };
        _queriesTreeView.AfterSelect += QueriesTreeView_AfterSelect;

        // Data Sources header button (brass background with blue text, rounded corners like Snap button)
        _dataHeaderLabel = new Label
        {
            Text = "Data Sources",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,  // Blue text
            BackColor = Color.Transparent,   // Transparent for custom painting
            AutoSize = false,
            Size = new Size(100, 20),  // Reduced from 260 to 120
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            Padding = new Padding(5)
        };
        _dataHeaderLabel.Click += AddDataSourceHeaderButton_Click;  // Make it clickable
        _dataHeaderLabel.Paint += DataHeaderLabel_Paint;  // Custom rounded painting
        
        // Add Data Source button (will be hidden - header label is now the button)
        _addDataSourceButton = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(45, 20),
            Visible = false  // Hide this button
        };
        _addDataSourceButton.FlatAppearance.BorderSize = 0;
        _addDataSourceButton.Click += AddDataSourceHeaderButton_Click;
        
        // Edit Mode button - now a discrete label in footer (bottom right)
        _editModeButton = new Button
        {
            Text = "Edit",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            ForeColor = _currentTheme.Accent,  // Brass color
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(50, 20),
            TextAlign = ContentAlignment.MiddleRight
        };
        _editModeButton.FlatAppearance.BorderSize = 0;
        _editModeButton.Click += EditModeButton_Click;
        
        // Edit Mode Label (shown when edit mode is active - clickable)
        _editModeLabel = new Label
        {
            Text = "EDIT MODE: Click to Disable",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Visible = false
        };
        _editModeLabel.Click += EditModeButton_Click;  // Same handler as button
        
        _dataTreeView = new TreeView
        {
            BackColor = _currentTheme.LightBlue,  // Light blue background
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10f),
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            FullRowSelect = true
        };
        _dataTreeView.AfterSelect += DataTreeView_AfterSelect;
        _dataTreeView.ItemDrag += TreeView_ItemDrag;

        // Context Menu for tree nodes (disabled by default, enabled when Edit mode is on)
        _treeContextMenu = new ContextMenuStrip();
        var deleteMenuItem = new ToolStripMenuItem("Delete");
        deleteMenuItem.Click += DeleteTreeNode_Click;
        _treeContextMenu.Items.Add(deleteMenuItem);
        
        // Context menus are null by default (Edit mode off)
        _queriesTreeView.ContextMenuStrip = null;
        _dataTreeView.ContextMenuStrip = null;

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
            Font = new Font("Segoe UI", 9f),
            Visible = false  // Hidden by default - table grid removed
        };
        
        // Add columns to Tables ListView
        _tablesListView.Columns.Add("Table Name", 300);
        _tablesListView.Columns.Add("Last Scanned", 200);
        _tablesListView.Columns.Add("Fields", 100);
        
        // Add Excel-style filter support
        _tablesListView.ColumnClick += TablesListView_ColumnClick;

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
        
        // Add Excel-style filter support
        _fieldsListView.ColumnClick += FieldsListView_ColumnClick;

        // Add Tables button (brass, appears above tables list when datasource selected)
        _addTablesButton = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            BackColor = _currentTheme.Accent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(100, 26),
            Enabled = false,
            Visible = false
        };
        _addTablesButton.FlatAppearance.BorderSize = 0;
        _addTablesButton.Click += AddButton_Click;
        
        // Add button (scan and add tables to datasource) - keep for backward compatibility at bottom
        _scanButton = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 139, 34),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(100, 26),
            Enabled = false,
            Visible = false  // Hide this, use _addTablesButton instead
        };
        _scanButton.FlatAppearance.BorderSize = 0;
        _scanButton.Click += AddButton_Click;

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
            BackColor = Color.FromArgb(46, 204, 113), // Green color (consistent with query export)
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
            ForeColor = _currentTheme.Accent,  // Brass color
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

        // Add Datasource Panel (shown when Company_Data is selected)
        _addDatasourcePanel = new Panel
        {
            BackColor = _currentTheme.Secondary,
            Visible = false
        };

        _addDatasourceTitleLabel = new Label
        {
            Text = "Add Data Source",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = _currentTheme.Accent,
            AutoSize = true,
            Location = new Point(10, 10)
        };

        _datasourceNameLabel = new Label
        {
            Text = "Name your Data Source:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 50)
        };

        _datasourceNameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10f),
            BackColor = Color.White,
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Size = new Size(300, 25),
            Location = new Point(10, 75)
        };

        _odbcNameLabel = new Label
        {
            Text = "ODBC Datasource Name:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 110)
        };

        _odbcNameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10f),
            BackColor = Color.White,
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Size = new Size(300, 25),
            Location = new Point(10, 135)
        };

        _addDatasourceButton = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Green,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(100, 30),
            Location = new Point(10, 175)
        };
        _addDatasourceButton.FlatAppearance.BorderSize = 0;
        _addDatasourceButton.Click += AddDatasourceButton_Click;

        _addDatasourcePanel.Controls.Add(_addDatasourceTitleLabel);
        _addDatasourcePanel.Controls.Add(_datasourceNameLabel);
        _addDatasourcePanel.Controls.Add(_datasourceNameTextBox);
        _addDatasourcePanel.Controls.Add(_odbcNameLabel);
        _addDatasourcePanel.Controls.Add(_odbcNameTextBox);
        _addDatasourcePanel.Controls.Add(_addDatasourceButton);

        // Query Builder Panel (shown when building a query)
        _queryBuilderPanel = new Panel
        {
            BackColor = _currentTheme.Secondary,
            Visible = false
        };

        _queryBuilderTitleLabel = new Label
        {
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = _currentTheme.Accent,  // Brass color
            AutoSize = true
        };

        _runQueryButton = new Button
        {
            Text = "▶ Run",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,  // Blue text
            BackColor = _currentTheme.Accent,   // Brass background
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(90, 28)
        };
        _runQueryButton.FlatAppearance.BorderSize = 0;
        _runQueryButton.Click += RunQueryButton_Click;

        // Tab Control
        _queryTabControl = new TabControl
        {
            Font = new Font("Segoe UI", 9f)
        };

        _criteriaTab = new TabPage("Criteria");
        _displayTab = new TabPage("Display");
        _tablesTab = new TabPage("Tables");
        _sqlTab = new TabPage("SQL");
        _resultsTab = new TabPage("Results");

        _queryTabControl.TabPages.Add(_criteriaTab);
        _queryTabControl.TabPages.Add(_displayTab);
        _queryTabControl.TabPages.Add(_tablesTab);
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

        // Tables Tab Panel
        _tablesPanel = new Panel
        {
            BackColor = Color.White,
            AutoScroll = true,
            Dock = DockStyle.Fill
        };

        // Tables Involved Label
        _tablesInvolvedLabel = new Label
        {
            Text = "Tables: (none)",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = _currentTheme.Primary,
            AutoSize = false,
            Size = new Size(600, 30),
            Location = new Point(10, 10),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Add Join Button
        _addJoinButton = new Button
        {
            Text = "Add Join",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = _currentTheme.Primary,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(100, 30),
            Location = new Point(10, 50)
        };
        _addJoinButton.FlatAppearance.BorderSize = 0;
        _addJoinButton.Click += AddJoinButton_Click;

        // Joins Panel (holds all join controls)
        _joinsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            AutoScroll = true,
            Location = new Point(10, 90),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        _tablesPanel.Controls.Add(_tablesInvolvedLabel);
        _tablesPanel.Controls.Add(_addJoinButton);
        _tablesPanel.Controls.Add(_joinsPanel);
        _tablesTab.Controls.Add(_tablesPanel);

        _queryBuilderPanel.Controls.Add(_queryBuilderTitleLabel);
        _queryBuilderPanel.Controls.Add(_runQueryButton);
        _queryBuilderPanel.Controls.Add(_queryTabControl);

    // Add controls to content panel
    _contentPanel.Controls.Add(_queriesHeaderLabel);
    _contentPanel.Controls.Add(_addQueryButton);
    _contentPanel.Controls.Add(_queriesTreeView);
    _contentPanel.Controls.Add(_dataHeaderLabel);
    _contentPanel.Controls.Add(_addDataSourceButton);
    _contentPanel.Controls.Add(_dataTreeView);
        _contentPanel.Controls.Add(_tablesListView);
        _contentPanel.Controls.Add(_addTablesButton);
        _contentPanel.Controls.Add(_fieldsListView);
        _contentPanel.Controls.Add(_newQueryPanel);
        _contentPanel.Controls.Add(_addDatasourcePanel);
        _contentPanel.Controls.Add(_queryBuilderPanel);
        _contentPanel.Controls.Add(_scanButton);
        _contentPanel.Controls.Add(_getUniqueValuesButton);
        _contentPanel.Controls.Add(_exportFieldsButton);
        _contentPanel.Controls.Add(_tableNameLabel);

        // Add controls to form
        this.Controls.Add(_contentPanel);
        this.Controls.Add(_titleLabel);
        this.Controls.Add(_closeButton);
        this.Controls.Add(_editModeButton);  // Add to form, not content panel (for footer)
        this.Controls.Add(_editModeLabel);   // Add to form, not content panel (for footer)

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
        
        _contentPanel.Size = new Size(this.Width, this.Height - TitleBarHeight - FooterHeight);

    // Calculate dimensions first
    int headerButtonHeight = 28;
    int dataRight = _contentPanel.Width - ControlMargin;
    int dataLeft = dataRight - DataTreeWidth;
    int middlePanelX = ControlMargin + QueriesTreeWidth + ControlMargin;
    int rightPanelX = middlePanelX;
    int rightPanelWidth = dataLeft - middlePanelX - ControlMargin;
    int rightPanelHeight = _contentPanel.Height - (ControlMargin * 2);  // Extend to footer
    
    // Left column: My Queries button and tree
    _queriesHeaderLabel.Location = new Point(ControlMargin, ControlMargin);
    _queriesHeaderLabel.Size = new Size(100, 20); 
    // Listbox extends all the way to bottom of content panel (footer)
    int listBoxHeight = _contentPanel.Height - ControlMargin - headerButtonHeight - 8;
    _queriesTreeView.Location = new Point(ControlMargin, ControlMargin + headerButtonHeight + 4);
    _queriesTreeView.Size = new Size(QueriesTreeWidth, listBoxHeight);

    // Right column: Data Sources button and tree
    _dataHeaderLabel.Location = new Point(dataLeft, ControlMargin);
    _dataHeaderLabel.Size = new Size(100, 20);
    _dataTreeView.Location = new Point(dataLeft, ControlMargin + headerButtonHeight + 4);
    _dataTreeView.Size = new Size(DataTreeWidth, listBoxHeight);  // Same height as queries list
    
    // Edit Mode button and label (discrete in bottom right of footer)
    int footerY = this.Height - FooterHeight + 10;
    _editModeButton.Location = new Point(this.Width - _editModeButton.Width - ControlMargin, footerY);
    _editModeLabel.Location = new Point(this.Width - _editModeLabel.Width - ControlMargin, footerY);
    _editModeLabel.Visible = false;  // Hidden by default

        // Add Tables button (above tables list, brass)
        _addTablesButton.Location = new Point(rightPanelX, ControlMargin);
        
        // Tables ListView (below the Add button when showing tables)
        int tablesListY = ControlMargin + _addTablesButton.Height + 5;
        _tablesListView.Location = new Point(rightPanelX, tablesListY);
        _tablesListView.Size = new Size(rightPanelWidth, rightPanelHeight - _addTablesButton.Height - 5);

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
        _runQueryButton.Location = new Point(rightPanelWidth - _runQueryButton.Width - 10, 7);
        _queryTabControl.Location = new Point(0, 40);
        _queryTabControl.Size = new Size(rightPanelWidth, rightPanelHeight - 40);

        // Add Datasource Panel layout
        _addDatasourcePanel.Location = new Point(rightPanelX, ControlMargin);
        _addDatasourcePanel.Size = new Size(rightPanelWidth, rightPanelHeight);

        // Add button at the bottom right
        int buttonY = _contentPanel.Height - FooterHeight + 5;
        _scanButton.Location = new Point(
            _contentPanel.Width - _scanButton.Width - ControlMargin,
            buttonY
        );
    }

    private void ApplyTheme()
    {
        this.BackColor = _currentTheme.Secondary;
        _contentPanel.BackColor = _currentTheme.Secondary;
        _queriesTreeView.BackColor = _currentTheme.LightBlue;
        _queriesTreeView.ForeColor = Color.White;
        _dataTreeView.BackColor = _currentTheme.LightBlue;
        _dataTreeView.ForeColor = Color.White;
        _tablesListView.BackColor = _currentTheme.Secondary;
        _tablesListView.ForeColor = Color.White;
        _fieldsListView.BackColor = _currentTheme.Secondary;
        _fieldsListView.ForeColor = Color.White;
    }

    private void LoadConfiguration()
    {
        _config = DatabaseLibraryManager.LoadConfig();
        
        // Initialize Queries tree
        _queriesTreeView.Nodes.Clear();
        _myQueriesNode = new TreeNode("My_Queries") { Tag = "user_queries" };
        _queriesTreeView.Nodes.Add(_myQueriesNode);
        _myQueriesNode.Expand();

        // Initialize Data Sources tree
        _dataTreeView.Nodes.Clear();
        _companyDataNode = new TreeNode("Company_Data") { Tag = "company_data" };
        _dataTreeView.Nodes.Add(_companyDataNode);
        
        // Load all datasources from configuration
        LoadDatasourcesFromConfig();
        
        _companyDataNode.Expand();
        
        // Load saved queries
        LoadQueries();
        
        // Hide all center panels on startup - center area should be empty
        HideAllCenterPanels();
    }
    
    private void LoadDatasourcesFromConfig()
    {
        _datasourceNodeMap.Clear();
        _companyDataNode?.Nodes.Clear();
        
        // Load each database from config as a datasource node
        foreach (var database in _config.Databases)
        {
            var dsNode = new TreeNode(database.Name.Replace("_Library", "")) 
            { 
                Tag = database 
            };
            
            // Add tables that are in the library
            foreach (var table in database.Tables.Where(t => t.IsInLibrary))
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
                
                dsNode.Nodes.Add(tableNode);
            }
            
            _companyDataNode?.Nodes.Add(dsNode);
            _datasourceNodeMap[dsNode] = database;
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

    private void QueriesTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        // If leaving a query, snapshot current UI into session cache first
        if (_currentQueryNode != null && e.Node != _currentQueryNode)
        {
            SnapshotQueryToSessionCache();
        }

        if (e.Node == _myQueriesNode)
        {
            // Clicking on My_Queries root - hide all center panels (blank view)
            HideAllCenterPanels();
        }
        else if (e.Node != null && e.Node.Parent == _myQueriesNode)
        {
            ShowQueryBuilderView(e.Node);
        }
    }

    private async void DataTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        // Clicking on Company_Data root - hide all center panels (blank view)
        if (e.Node == _companyDataNode)
        {
            HideAllCenterPanels();
            return;
        }
        else if (e.Node != null && e.Node.Parent == _companyDataNode && e.Node.Tag is DatabaseConfig dbConfig)
        {
            // User clicked on a datasource node
            ShowTablesView();
            await LoadDatabaseTablesAsync(dbConfig);
        }
        else if (e.Node != null && e.Node.Parent?.Parent == _companyDataNode && e.Node.Tag is TableMetadata table)
        {
            // User clicked on a table node under a datasource
            ShowFieldsView(table);
        }
    }

    private void ShowAddDatasourceView()
    {
        HideAllCenterPanels();
        _addDatasourcePanel.Visible = true;
        _datasourceNameTextBox.Clear();
        _odbcNameTextBox.Clear();
    }

    private void ShowTablesView()
    {
        HideAllCenterPanels();
        _tablesListView.Visible = true;  // Show table grid when datasource is selected
        _addTablesButton.Visible = true;
        _addTablesButton.Enabled = false;  // Will be enabled when tables load
        _currentTable = null;
    }

    private void HideAllCenterPanels()
    {
        _tablesListView.Visible = false;
        _addTablesButton.Visible = false;
        _fieldsListView.Visible = false;
        _newQueryPanel.Visible = false;
        _addDatasourcePanel.Visible = false;
        _queryBuilderPanel.Visible = false;
        _scanButton.Visible = false;
        _getUniqueValuesButton.Visible = false;
        _exportFieldsButton.Visible = false;
        _tableNameLabel.Visible = false;
    }

    private void ShowNewQueryView()
    {
        HideAllCenterPanels();
        _newQueryPanel.Visible = true;
        _queryNameTextBox.Clear();
    }

    private void ShowQueryBuilderView(TreeNode queryNode)
    {
        _currentQueryNode = queryNode;
        _currentQuery = queryNode.Tag as QueryDefinition;
        
        HideAllCenterPanels();
        _queryBuilderPanel.Visible = true;
        
        // Update title
        _queryBuilderTitleLabel.Text = $"My_Queries → {queryNode.Text}";
        
        // Clear both panels
        _criteriaPanel.Controls.Clear();
        _displayPanel.Controls.Clear();

        // Restore from session cache first (unsaved UI), else from saved query
        if (_currentQueryNode != null && _querySessionCache.TryGetValue(_currentQueryNode.Text, out var cached))
        {
            RestoreFromSessionCache(cached);
        }
        else if (_currentQuery != null)
        {
            LoadQueryFields(_currentQuery);
        }
    }

    private void SnapshotQueryToSessionCache()
    {
        if (_currentQueryNode == null) return;

        var criteriaSnapshot = new List<QueryCriteriaField>();
        foreach (Control control in _criteriaPanel.Controls)
        {
            if (control is SuiteView.Forms.Controls.CriteriaFieldControl cfc)
            {
                criteriaSnapshot.Add(cfc.ToQueryCriteriaField());
            }
            else if (control is RoundedPanel panel && panel.Tag != null)
            {
                var tag = panel.Tag;
                var tableProperty = tag.GetType().GetProperty("Table");
                var fieldProperty = tag.GetType().GetProperty("Field");
                if (tableProperty == null || fieldProperty == null) continue;
                var table = tableProperty.GetValue(tag) as TableMetadata;
                var field = fieldProperty.GetValue(tag) as FieldMetadata;
                if (table == null || field == null) continue;

                var snapshot = new QueryCriteriaField
                {
                    TableName = table.TableName,
                    FieldName = field.FieldName,
                    DataType = field.DataType,
                    PanelHeight = panel.Height
                };

                var listBox = panel.Controls.OfType<CheckedListBox>().FirstOrDefault();
                if (listBox != null)
                {
                    snapshot.HasListBox = true;
                    snapshot.SelectedValues = listBox.CheckedItems.Cast<string>().ToList();
                }
                else
                {
                    var textBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
                    snapshot.HasListBox = false;
                    snapshot.TextValue = textBox?.Text ?? string.Empty;
                }

                criteriaSnapshot.Add(snapshot);
            }
        }

        var displaySnapshot = new List<QueryDisplayField>();
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

                displaySnapshot.Add(new QueryDisplayField
                {
                    TableName = table.TableName,
                    FieldName = field.FieldName,
                    DataType = field.DataType
                });
            }
        }

        _querySessionCache[_currentQueryNode.Text] = (criteriaSnapshot, displaySnapshot);
    }

    private void RestoreFromSessionCache((List<QueryCriteriaField> criteria, List<QueryDisplayField> display) cached)
    {
        // Set flag to prevent auto-save during restore (prevents collection modification errors)
        _isLoadingQuery = true;
        
        try
        {
            // Restore criteria
            foreach (var c in cached.criteria)
            {
                // Search across all databases for the table
                TableMetadata? table = null;
                foreach (var db in _config.Databases)
                {
                    table = db.Tables.FirstOrDefault(t => t.TableName == c.TableName);
                    if (table != null) break;
                }
                if (table == null) continue;
                
                var field = table.Fields.FirstOrDefault(f => f.FieldName == c.FieldName);
                if (field == null) continue;
                var tableMetadata = new TableMetadata { TableName = table.TableName };
                var panel = CreateCriteriaFieldPanelFromSaved(tableMetadata, field, c);
                _criteriaPanel.Controls.Add(panel);
            }

            // Restore display
            foreach (var d in cached.display)
            {
                // Search across all databases for the table
                TableMetadata? table = null;
                foreach (var db in _config.Databases)
                {
                    table = db.Tables.FirstOrDefault(t => t.TableName == d.TableName);
                    if (table != null) break;
                }
                if (table == null) continue;
                
                var field = table.Fields.FirstOrDefault(f => f.FieldName == d.FieldName);
                if (field == null) continue;
                var tableMetadata = new TableMetadata { TableName = table.TableName };
                CreateDisplayFieldPanel(tableMetadata, field);
            }
        }
        finally
        {
            // Always re-enable auto-save after restore completes
            _isLoadingQuery = false;
        }
    }

    private void ShowFieldsView(TableMetadata table)
    {
        _currentTable = table;
        
        HideAllCenterPanels();
        _fieldsListView.Visible = true;
        _getUniqueValuesButton.Visible = true;
        _getUniqueValuesButton.Enabled = true;
        _exportFieldsButton.Visible = true;
        _exportFieldsButton.Enabled = true;
        _tableNameLabel.Visible = true;
        _tableNameLabel.Text = table.TableName;

        // Populate fields ListView
        _fieldsListView.Items.Clear();
        _allFieldItems.Clear();
        _fieldsActiveFilters.Clear();

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
            _allFieldItems.Add((ListViewItem)item.Clone());
        }
    }

    private async Task LoadDatabaseTablesAsync(DatabaseConfig dbConfig)
    {
        try
        {
            _tablesListView.Items.Clear();
            _allTableItems.Clear();
            _tablesActiveFilters.Clear();
            _addTablesButton.Enabled = false;

            Cursor = Cursors.WaitCursor;

            // Fetch table names from database
            var tableNames = await DatabaseService.GetTableNamesAsync(dbConfig.OdbcDsn, dbConfig.DatabaseName);

            // Display tables in ListView
            foreach (var tableName in tableNames)
            {
                var existingTable = dbConfig.Tables.FirstOrDefault(t => t.TableName == tableName);
                if (existingTable == null)
                {
                    existingTable = new TableMetadata { TableName = tableName };
                    dbConfig.Tables.Add(existingTable);
                }

                var item = new ListViewItem(tableName);
                item.SubItems.Add(existingTable.LastScanned?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never");
                item.SubItems.Add(existingTable.Fields.Count.ToString());
                item.Tag = new { Database = dbConfig, Table = existingTable };
                _tablesListView.Items.Add(item);
                _allTableItems.Add((ListViewItem)item.Clone());
            }

            _addTablesButton.Enabled = true;

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

    private async void AddButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = _tablesListView.CheckedItems.Cast<ListViewItem>().ToList();
        if (selectedItems.Count == 0)
        {
            MessageBox.Show("Please select tables to add by checking their checkboxes.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Get the currently selected datasource node
        var selectedDsNode = _dataTreeView.SelectedNode;
        if (selectedDsNode == null || selectedDsNode.Parent != _companyDataNode || selectedDsNode.Tag is not DatabaseConfig dbConfig)
        {
            MessageBox.Show("Please select a datasource first.", "No Datasource Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            _scanButton.Enabled = false;

            foreach (ListViewItem item in selectedItems)
            {
                dynamic tagData = item.Tag!;
                DatabaseConfig database = tagData.Database;
                TableMetadata table = tagData.Table;

                // Scan table fields
                var fields = await DatabaseService.GetTableFieldsAsync(database.OdbcDsn, database.DatabaseName, table.TableName);
                table.Fields = fields;
                table.LastScanned = DateTime.Now;
                table.IsInLibrary = true;

                // Create table node under the datasource node
                var tableNode = new TreeNode(table.TableName) { Tag = table };
                
                // Add field nodes as children
                foreach (var field in table.Fields)
                {
                    var fieldNode = new TreeNode($"{field.FieldName} ({field.DataType})") { Tag = field };
                    tableNode.Nodes.Add(fieldNode);
                }
                
                selectedDsNode.Nodes.Add(tableNode);
            }

            // Save configuration
            DatabaseLibraryManager.SaveConfig(_config);

            // Expand to show new tables
            selectedDsNode.Expand();

            MessageBox.Show($"Successfully added {selectedItems.Count} table(s) to {selectedDsNode.Text}!", "Add Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Refresh the tables list to remove added tables
            await LoadDatabaseTablesAsync(dbConfig);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding tables: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _addTablesButton.Enabled = true;
        }
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

            // Analysis complete - no popup needed
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
        
    // Select the new query node in Queries tree
    _queriesTreeView.SelectedNode = queryNode;
        
        // This will trigger ShowQueryBuilderView via TreeView_AfterSelect
    }
    
    private void LoadQueryFields(QueryDefinition query)
    {
        // Set flag to prevent auto-save during loading (prevents collection modification errors)
        _isLoadingQuery = true;
        
        try
        {
            // Load criteria fields
            foreach (var criteriaField in query.CriteriaFields)
            {
                // Search across all databases for the table
                TableMetadata? table = null;
                foreach (var db in _config.Databases)
                {
                    table = db.Tables.FirstOrDefault(t => t.TableName == criteriaField.TableName);
                    if (table != null) break;
                }
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
                // Search across all databases for the table
                TableMetadata? table = null;
                foreach (var db in _config.Databases)
                {
                    table = db.Tables.FirstOrDefault(t => t.TableName == displayField.TableName);
                    if (table != null) break;
                }
                if (table == null) continue;

                var field = table.Fields.FirstOrDefault(f => f.FieldName == displayField.FieldName);
                if (field == null) continue;

                var tableMetadata = new TableMetadata { TableName = table.TableName };
                CreateDisplayFieldPanel(tableMetadata, field);
            }
        }
        finally
        {
            // Always re-enable auto-save after loading completes
            _isLoadingQuery = false;
        }
    }

    /// <summary>
    /// Auto-save the current query to the JSON configuration file.
    /// Called automatically whenever criteria or display fields change.
    /// </summary>
    private void AutoSaveQuery()
    {
        // Don't save if no query is selected or if we're currently loading a query
        if (_currentQueryNode == null || _currentQuery == null || _isLoadingQuery)
            return;

        try
        {
            // Save criteria fields
            _currentQuery.CriteriaFields.Clear();
            foreach (Control control in _criteriaPanel.Controls)
            {
                // Check for CriteriaFieldControl first (it extends RoundedPanel)
                if (control is CriteriaFieldControl cfc)
                {
                    var criteriaField = cfc.ToQueryCriteriaField();
                    _currentQuery.CriteriaFields.Add(criteriaField);
                    
                    System.Diagnostics.Debug.WriteLine($"AutoSave - Captured criteria for {criteriaField.TableName}.{criteriaField.FieldName}:");
                    System.Diagnostics.Debug.WriteLine($"  HasListBox: {criteriaField.HasListBox}");
                    System.Diagnostics.Debug.WriteLine($"  SelectedValues: [{string.Join(", ", criteriaField.SelectedValues)}]");
                    System.Diagnostics.Debug.WriteLine($"  TextValue: '{criteriaField.TextValue}'");
                    System.Diagnostics.Debug.WriteLine($"  StringOperator: {criteriaField.StringOperator}");
                }
                // Fallback for old-style RoundedPanel criteria (if any exist)
                else if (control is RoundedPanel panel && panel.Tag != null)
                {
                    var tag = panel.Tag;
                    var tableProperty = tag.GetType().GetProperty("Table");
                    var fieldProperty = tag.GetType().GetProperty("Field");

                    var table = tableProperty?.GetValue(tag) as TableMetadata;
                    var field = fieldProperty?.GetValue(tag) as FieldMetadata;
                    if (table == null || field == null) continue;

                    var criteriaField = new QueryCriteriaField
                    {
                        TableName = table.TableName,
                        FieldName = field.FieldName,
                        DataType = field.DataType,
                        PanelHeight = panel.Height
                    };
                    var listBox = panel.Controls.OfType<CheckedListBox>().FirstOrDefault();
                    if (listBox != null)
                    {
                        criteriaField.HasListBox = true;
                        criteriaField.SelectedValues = listBox.CheckedItems.Cast<string>().ToList();
                    }
                    else
                    {
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

                    var table = tableProperty?.GetValue(tag) as TableMetadata;
                    var field = fieldProperty?.GetValue(tag) as FieldMetadata;
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

            // Save joins
            _currentQuery.Joins.Clear();
            foreach (Control control in _joinsPanel.Controls)
            {
                if (control is JoinControl joinControl)
                {
                    var queryJoin = new QueryJoin
                    {
                        LeftTable = joinControl.LeftTable,
                        JoinType = joinControl.JoinType,
                        RightTable = joinControl.RightTable,
                        Conditions = joinControl.Conditions.Select(c => new QueryJoinCondition
                        {
                            LeftField = c.leftField,
                            RightField = c.rightField
                        }).ToList()
                    };
                    _currentQuery.Joins.Add(queryJoin);
                }
            }

            // Update last modified date
            _currentQuery.LastModifiedDate = DateTime.Now;

            // Save to JSON (silently)
            DatabaseLibraryManager.SaveConfig(_config);
        }
        catch (Exception ex)
        {
            // Log error but don't show message box for auto-save
            System.Diagnostics.Debug.WriteLine($"Auto-save error: {ex.Message}");
        }
    }

    #endregion

    #region Query Execution

    private async void RunQueryButton_Click(object? sender, EventArgs e)
    {
        if (_currentQuery == null || _currentQueryNode == null)
        {
            MessageBox.Show("No query selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Validate query has display fields
        if (_currentQuery.DisplayFields.Count == 0)
        {
            MessageBox.Show("Query must have at least one display field.\n\nDrag fields from the data tree to the Display tab.", 
                "No Display Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Determine datasource from first display field's table
        if (string.IsNullOrEmpty(_currentQuery.DataSourceName))
        {
            // Infer from first table
            var firstTableName = _currentQuery.DisplayFields.FirstOrDefault()?.TableName ?? "";
            DatabaseConfig? database = null;
            
            foreach (var db in _config.Databases)
            {
                if (db.Tables.Any(t => t.TableName == firstTableName))
                {
                    database = db;
                    _currentQuery.DataSourceName = db.Name;
                    AutoSaveQuery(); // Save the datasource association
                    break;
                }
            }

            if (database == null)
            {
                MessageBox.Show("Could not determine which datasource this query uses.\n\nPlease ensure the tables exist in a configured datasource.", 
                    "Datasource Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        // Get the database config
        var dbConfig = _config.Databases.FirstOrDefault(db => db.Name == _currentQuery.DataSourceName);
        if (dbConfig == null)
        {
            MessageBox.Show($"Datasource '{_currentQuery.DataSourceName}' not found in configuration.", 
                "Datasource Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            // Save current query state before building SQL (capture all criteria/display fields from UI)
            AutoSaveQuery();
            
            // Debug: Log criteria fields
            System.Diagnostics.Debug.WriteLine($"Building SQL with {_currentQuery.CriteriaFields.Count} criteria fields:");
            foreach (var crit in _currentQuery.CriteriaFields)
            {
                System.Diagnostics.Debug.WriteLine($"  - {crit.TableName}.{crit.FieldName}: HasListBox={crit.HasListBox}, TextValue='{crit.TextValue}', SelectedValues={string.Join(",", crit.SelectedValues)}");
            }
            
            // Build SQL query
            string sql = BuildSqlQuery(_currentQuery, dbConfig.OdbcDsn);
            
            // Display formatted SQL in SQL tab
            DisplayFormattedSql(sql);
            
            // Switch to SQL tab to show the query
            _queryTabControl.SelectedTab = _sqlTab;
            
            // Execute query and show results (with debug logging enabled)
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var results = await DatabaseService.ExecuteQueryAsync(sql, dbConfig.OdbcDsn, dbConfig.DatabaseName, debugLog: true);
            stopwatch.Stop();
            
            // Display results
            DisplayQueryResults(results, stopwatch.Elapsed);
            
            // Switch to Results tab
            _queryTabControl.SelectedTab = _resultsTab;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Query execution failed:\n\n{ex.Message}", 
                "Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            System.Diagnostics.Debug.WriteLine($"Query execution error: {ex}");
        }
    }

    private string BuildSqlQuery(QueryDefinition query, string odbcDsn)
    {
        var sql = new System.Text.StringBuilder();
        
        // Determine if this is DB2
        bool isDb2 = odbcDsn.Equals("NEON_DSN", StringComparison.OrdinalIgnoreCase);
        
        // Add WITH DUMMY clause for DB2
        if (isDb2)
        {
            sql.AppendLine("WITH DUMBY AS (SELECT 1 FROM DB2TAB.LH_COV_PHA)");
        }
        
        // SELECT clause
        sql.AppendLine("SELECT");
        for (int i = 0; i < query.DisplayFields.Count; i++)
        {
            var field = query.DisplayFields[i];
            sql.Append($"    {field.TableName}.{field.FieldName}");
            if (i < query.DisplayFields.Count - 1)
                sql.AppendLine(",");
            else
                sql.AppendLine();
        }
        
        // FROM clause - get unique tables
        var tables = new HashSet<string>();
        foreach (var field in query.DisplayFields)
            tables.Add(field.TableName);
        foreach (var field in query.CriteriaFields)
            tables.Add(field.TableName);
        
        // Determine base table (first table that appears in joins or first display field)
        string baseTable = query.Joins.Count > 0 
            ? query.Joins[0].LeftTable 
            : query.DisplayFields[0].TableName;
        
        sql.AppendLine("FROM");
        sql.AppendLine($"    {baseTable}");
        
        // JOIN clauses
        foreach (var join in query.Joins)
        {
            sql.AppendLine($"    {join.JoinType} {join.RightTable}");
            for (int i = 0; i < join.Conditions.Count; i++)
            {
                var condition = join.Conditions[i];
                string keyword = i == 0 ? "ON" : "AND";
                sql.AppendLine($"        {keyword} {join.LeftTable}.{condition.LeftField} = {join.RightTable}.{condition.RightField}");
            }
        }
        
        // WHERE clause
        if (query.CriteriaFields.Count > 0)
        {
            var whereConditions = new List<string>();
            
            foreach (var criteria in query.CriteriaFields)
            {
                string condition = BuildWhereCondition(criteria);
                if (!string.IsNullOrEmpty(condition))
                {
                    whereConditions.Add(condition);
                }
            }
            
            if (whereConditions.Count > 0)
            {
                sql.AppendLine("WHERE");
                for (int i = 0; i < whereConditions.Count; i++)
                {
                    if (i > 0)
                        sql.Append("    AND ");
                    else
                        sql.Append("    ");
                    
                    sql.AppendLine(whereConditions[i]);
                }
            }
        }
        
        return sql.ToString();
    }

    private string BuildWhereCondition(QueryCriteriaField criteria)
    {
        string fieldRef = $"{criteria.TableName}.{criteria.FieldName}";
        
        System.Diagnostics.Debug.WriteLine($"BuildWhereCondition for {fieldRef}:");
        System.Diagnostics.Debug.WriteLine($"  HasListBox: {criteria.HasListBox}");
        System.Diagnostics.Debug.WriteLine($"  SelectedValues: {string.Join(", ", criteria.SelectedValues)}");
        System.Diagnostics.Debug.WriteLine($"  TextValue: '{criteria.TextValue}'");
        System.Diagnostics.Debug.WriteLine($"  StringOperator: {criteria.StringOperator}");
        
        // Handle list box with multiple selections
        if (criteria.HasListBox && criteria.SelectedValues.Count > 0)
        {
            // Filter out "(none)" if present
            var values = criteria.SelectedValues.Where(v => v != "(none)").ToList();
            if (values.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"  Result: EMPTY (all values were '(none)')");
                return string.Empty;
            }
            
            if (values.Count == 1)
            {
                return $"{fieldRef} = '{EscapeSqlString(values[0])}'";
            }
            else
            {
                var inList = string.Join(", ", values.Select(v => $"'{EscapeSqlString(v)}'"));
                return $"{fieldRef} IN ({inList})";
            }
        }
        
        // Handle text input with operators
        if (!string.IsNullOrWhiteSpace(criteria.TextValue))
        {
            string value = criteria.TextValue;
            string op = criteria.StringOperator ?? "none";
            
            // Check if this is a numeric field using ODBC SQL type codes
            // ODBC type codes: 4=INT, -5=BIGINT, 3=DECIMAL, 2=NUMERIC, 6=FLOAT, 7=REAL, 8=DOUBLE, 5=SMALLINT, -6=TINYINT
            // String types: 1=CHAR, 12=VARCHAR, -1=LONGVARCHAR, -8=NCHAR, -9=NVARCHAR, -10=LONGNVARCHAR
            bool isNumeric = false;
            if (int.TryParse(criteria.DataType, out int typeCode))
            {
                // Numeric ODBC type codes
                isNumeric = typeCode == 4 || typeCode == -5 || typeCode == 3 || typeCode == 2 || 
                           typeCode == 6 || typeCode == 7 || typeCode == 8 || typeCode == 5 || typeCode == -6;
            }
            else
            {
                // Fallback for text-based type names (in case we get "INTEGER" instead of "4")
                isNumeric = criteria.DataType.Contains("INT", StringComparison.OrdinalIgnoreCase) ||
                           criteria.DataType.Contains("DECIMAL", StringComparison.OrdinalIgnoreCase) ||
                           criteria.DataType.Contains("NUMERIC", StringComparison.OrdinalIgnoreCase) ||
                           criteria.DataType.Contains("FLOAT", StringComparison.OrdinalIgnoreCase) ||
                           criteria.DataType.Contains("DOUBLE", StringComparison.OrdinalIgnoreCase) ||
                           criteria.DataType.Contains("MONEY", StringComparison.OrdinalIgnoreCase);
            }
            
            System.Diagnostics.Debug.WriteLine($"  DataType: '{criteria.DataType}', IsNumeric: {isNumeric}");
            
            if (isNumeric)
            {
                // Numeric field - use direct comparison without quotes
                return $"{fieldRef} = {value}";
            }
            else
            {
                // String field (default) - use quotes and apply operator
                switch (op.ToLower())
                {
                    case "equals":
                        return $"{fieldRef} = '{EscapeSqlString(value)}'";
                    case "contains":
                        return $"{fieldRef} LIKE '%{EscapeSqlString(value)}%'";
                    case "begins with":
                        return $"{fieldRef} LIKE '{EscapeSqlString(value)}%'";
                    case "ends with":
                        return $"{fieldRef} LIKE '%{EscapeSqlString(value)}'";
                    case "none":
                    default:
                        return $"{fieldRef} = '{EscapeSqlString(value)}'";
                }
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"  Result: EMPTY (no valid criteria)");
        return string.Empty;
    }

    private string EscapeSqlString(string value)
    {
        // Escape single quotes by doubling them
        return value.Replace("'", "''");
    }

    private void DisplayFormattedSql(string sql)
    {
        // Clear existing controls
        _sqlTab.Controls.Clear();
        
        // Create RichTextBox for SQL display
        var sqlTextBox = new RichTextBox
        {
            Text = sql,
            Font = new Font("Consolas", 10f),
            ReadOnly = true,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(220, 220, 220),
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            WordWrap = false
        };
        
        _sqlTab.Controls.Add(sqlTextBox);
    }

    private void DisplayQueryResults(DataTable results, TimeSpan elapsed)
    {
        // Clear existing controls
        _resultsTab.Controls.Clear();
        
        // Create info panel at bottom
        var infoPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            BackColor = _currentTheme.Secondary
        };
        
        var rowCountLabel = new Label
        {
            Text = $"Rows: {results.Rows.Count:N0}  |  Columns: {results.Columns.Count}  |  Time: {elapsed.TotalSeconds:F2}s",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = _currentTheme.Accent,
            Location = new Point(10, 10),
            AutoSize = true
        };
        infoPanel.Controls.Add(rowCountLabel);
        
        // Create export button (green)
        var exportButton = new Button
        {
            Text = "Export",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(46, 204, 113), // Green color
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 26),
            Location = new Point(infoPanel.Width - 110, 7),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        exportButton.FlatAppearance.BorderSize = 0;
        exportButton.Click += (s, e) => ExportToExcel(results);
        infoPanel.Controls.Add(exportButton);
        
        _resultsTab.Controls.Add(infoPanel);
        
        // Create DataGridView for results (supports up to 300K rows)
        var resultsGrid = new DataGridView
        {
            DataSource = results,
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToOrderColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _currentTheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            }
        };
        
        _resultsTab.Controls.Add(resultsGrid);
    }

    private void ExportToExcel(DataTable data)
    {
        // TODO: Implement Excel export using EPPlus or similar library
        // For now, button exists but does nothing when clicked
    }

    #endregion

    #region Joins

    private void AddJoinButton_Click(object? sender, EventArgs e)
    {
        var tables = GetInvolvedTables();
        if (tables.Count < 2)
        {
            MessageBox.Show("You need at least 2 tables to create a join. Add fields from different tables first.", 
                "Insufficient Tables", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var joinControl = new JoinControl(_currentTheme, tables, _config);
        joinControl.RemoveRequested += (s, args) =>
        {
            _joinsPanel.Controls.Remove(joinControl);
            joinControl.Dispose();
        };
        
        _joinsPanel.Controls.Add(joinControl);
    }

    private void AddQueryButton_Click(object? sender, EventArgs e)
    {
        ShowNewQueryView();
    }

    private void AddDataSourceHeaderButton_Click(object? sender, EventArgs e)
    {
        ShowAddDatasourceView();
    }

    private bool _editModeEnabled = false;
    
    private void EditModeButton_Click(object? sender, EventArgs e)
    {
        _editModeEnabled = !_editModeEnabled;
        
        if (_editModeEnabled)
        {
            // Enable edit mode - change background to light red, show label, hide button
            _editModeButton.Visible = false;
            _editModeLabel.Visible = true;
            _queriesTreeView.BackColor = ColorTranslator.FromHtml("#FFB3B3");  // Light red
            _dataTreeView.BackColor = ColorTranslator.FromHtml("#FFB3B3");  // Light red
            _queriesTreeView.ContextMenuStrip = _treeContextMenu;
            _dataTreeView.ContextMenuStrip = _treeContextMenu;
        }
        else
        {
            // Disable edit mode - restore original background, show button, hide label
            _editModeButton.Visible = true;
            _editModeLabel.Visible = false;
            _queriesTreeView.BackColor = _currentTheme.LightBlue;
            _dataTreeView.BackColor = _currentTheme.LightBlue;
            _queriesTreeView.ContextMenuStrip = null;
            _dataTreeView.ContextMenuStrip = null;
        }
    }

    private async void AddDatasourceButton_Click(object? sender, EventArgs e)
    {
        string dsName = _datasourceNameTextBox.Text.Trim();
        string odbcName = _odbcNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(dsName))
        {
            MessageBox.Show("Please enter a name for the data source.", "Missing Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(odbcName))
        {
            MessageBox.Show("Please enter an ODBC datasource name.", "Missing ODBC Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            _addDatasourceButton.Enabled = false;

            // Create or get database config
            var db = DatabaseLibraryManager.GetOrCreateDatabase(_config, $"{dsName}_Library", odbcName, odbcName);

            // Fetch table names from database
            List<string> tableNames;
            try
            {
                tableNames = await DatabaseService.GetTableNamesAsync(odbcName, odbcName);
            }
            catch (Exception dbEx)
            {
                MessageBox.Show($"Failed to connect to ODBC data source '{odbcName}'.\n\nError: {dbEx.Message}\n\nPlease verify:\n- The ODBC DSN name is correct\n- The data source is configured in ODBC Data Sources\n- You have the necessary permissions", 
                    "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (tableNames.Count == 0)
            {
                MessageBox.Show($"Connected to '{odbcName}' but found no tables.\n\nThis might mean:\n- The database is empty\n- You don't have permissions to view tables\n- The connection succeeded but schema retrieval failed", 
                    "No Tables Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add all tables to the database config (not in library yet)
            foreach (var tableName in tableNames)
            {
                var existingTable = db.Tables.FirstOrDefault(t => t.TableName == tableName);
                if (existingTable == null)
                {
                    existingTable = new TableMetadata { TableName = tableName };
                    db.Tables.Add(existingTable);
                }
            }

            // Save configuration
            DatabaseLibraryManager.SaveConfig(_config);

            // Reload all datasources from config to refresh the tree
            LoadDatasourcesFromConfig();
            
            // Expand to show the new datasource
            _companyDataNode?.Expand();

            MessageBox.Show($"Data source '{dsName}' added successfully with {tableNames.Count} tables!", 
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear inputs
            _datasourceNameTextBox.Clear();
            _odbcNameTextBox.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding data source:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _addDatasourceButton.Enabled = true;
        }
    }

    private void DeleteTreeNode_Click(object? sender, EventArgs e)
    {
        // Determine which tree view triggered the context menu
        TreeView? activeTreeView = null;
        TreeNode? selectedNode = null;

        if (_queriesTreeView.SelectedNode != null && _queriesTreeView.Focused)
        {
            activeTreeView = _queriesTreeView;
            selectedNode = _queriesTreeView.SelectedNode;
        }
        else if (_dataTreeView.SelectedNode != null && _dataTreeView.Focused)
        {
            activeTreeView = _dataTreeView;
            selectedNode = _dataTreeView.SelectedNode;
        }
        else
        {
            // Fallback: check which tree has a selected node
            if (_queriesTreeView.SelectedNode != null)
            {
                activeTreeView = _queriesTreeView;
                selectedNode = _queriesTreeView.SelectedNode;
            }
            else if (_dataTreeView.SelectedNode != null)
            {
                activeTreeView = _dataTreeView;
                selectedNode = _dataTreeView.SelectedNode;
            }
        }

        if (activeTreeView == null || selectedNode == null)
        {
            MessageBox.Show("No node selected.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Check if node has children
        if (selectedNode.Nodes.Count > 0)
        {
            var result = MessageBox.Show(
                $"'{selectedNode.Text}' has {selectedNode.Nodes.Count} child item(s). Are you sure you want to delete it and all its children?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;
        }

        // Handle deletion based on node type
        if (activeTreeView == _queriesTreeView)
        {
            DeleteQueryNode(selectedNode);
        }
        else if (activeTreeView == _dataTreeView)
        {
            DeleteDataNode(selectedNode);
        }
    }

    private void DeleteQueryNode(TreeNode node)
    {
        // Don't delete root nodes
        if (node.Parent == null)
        {
            MessageBox.Show("Cannot delete root node.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // If it's a query node
        if (node.Tag is QueryDefinition query)
        {
            // Remove from config queries list
            var existingQuery = _config.Queries.FirstOrDefault(q => q.Name == query.Name);
            if (existingQuery != null)
            {
                _config.Queries.Remove(existingQuery);
            }

            // Save configuration
            DatabaseLibraryManager.SaveConfig(_config);
        }

        // Remove from tree
        node.Remove();
        MessageBox.Show("Item deleted successfully.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DeleteDataNode(TreeNode node)
    {
        // Don't delete root nodes (Company_Data)
        if (node.Parent == null)
        {
            MessageBox.Show("Cannot delete root node.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // If it's a table node, remove it from IsInLibrary
        if (node.Tag is TableMetadata table)
        {
            table.IsInLibrary = false;
            DatabaseLibraryManager.SaveConfig(_config);
            node.Remove();
            MessageBox.Show("Table removed from library.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // If it's a datasource node, remove the entire database from config
        if (node.Tag is DatabaseConfig dbConfig)
        {
            _config.Databases.Remove(dbConfig);
            DatabaseLibraryManager.SaveConfig(_config);
            node.Remove();
            MessageBox.Show($"Data source '{node.Text}' deleted successfully.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // For other nodes, just remove from tree
        node.Remove();
        MessageBox.Show("Item deleted successfully.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateTablesInvolvedLabel()
    {
        var tables = GetInvolvedTables();
        if (tables.Count == 0)
        {
            _tablesInvolvedLabel.Text = "Tables: (none)";
        }
        else
        {
            _tablesInvolvedLabel.Text = $"Tables: {string.Join(", ", tables)}";
        }
        
        // Update available tables in all existing join controls
        foreach (var control in _joinsPanel.Controls)
        {
            if (control is JoinControl joinControl)
            {
                joinControl.UpdateAvailableTables(tables);
            }
        }
    }

    private List<string> GetInvolvedTables()
    {
        var tables = new HashSet<string>();
        
        // Get tables from Criteria fields
        foreach (Control control in _criteriaPanel.Controls)
        {
            if (control is CriteriaFieldControl cfc)
            {
                var field = cfc.ToQueryCriteriaField();
                if (!string.IsNullOrWhiteSpace(field.TableName))
                {
                    tables.Add(field.TableName);
                }
            }
            else if (control is RoundedPanel panel && panel.Tag != null)
            {
                var tag = panel.Tag;
                var tableProperty = tag.GetType().GetProperty("Table");
                if (tableProperty != null)
                {
                    var tableMetadata = tableProperty.GetValue(tag) as TableMetadata;
                    if (tableMetadata != null)
                    {
                        tables.Add(tableMetadata.TableName);
                    }
                }
            }
        }
        
        // Get tables from Display fields
        foreach (Control control in _displayPanel.Controls)
        {
            if (control is RoundedPanel panel && panel.Tag != null)
            {
                var tag = panel.Tag;
                var tableProperty = tag.GetType().GetProperty("Table");
                if (tableProperty != null)
                {
                    var tableMetadata = tableProperty.GetValue(tag) as TableMetadata;
                    if (tableMetadata != null)
                    {
                        tables.Add(tableMetadata.TableName);
                    }
                }
            }
        }
        
        return tables.OrderBy(t => t).ToList();
    }

    private void TreeView_ItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (e.Item is TreeNode node)
        {
            // Only allow dragging field nodes (nodes with FieldMetadata tag)
            if (node.Tag is FieldMetadata)
            {
                if (sender is TreeView tv)
                {
                    tv.DoDragDrop(node, DragDropEffects.Copy);
                }
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
            
            // Update tables involved label
            UpdateTablesInvolvedLabel();
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
            
            // Update tables involved label
            UpdateTablesInvolvedLabel();
        }
    }

    private void CreateCriteriaFieldPanel(TableMetadata table, FieldMetadata field)
    {
        var fieldControl = new CriteriaFieldControl(_criteriaPanel, table, field);
        fieldControl.RemoveRequested += (s, e) =>
        {
            _criteriaPanel.Controls.Remove(fieldControl);
            UpdateTablesInvolvedLabel(); // Update tables when field removed
            AutoSaveQuery(); // Auto-save when field removed
        };
        fieldControl.UserResizeCompleted += (s, e) => SnapshotQueryToSessionCache();
        fieldControl.ValueChanged += (s, e) => AutoSaveQuery(); // Auto-save when value changed
        _criteriaPanel.Controls.Add(fieldControl);
        AutoSaveQuery(); // Auto-save when field added
    }

    private RoundedPanel CreateCriteriaFieldPanelFromSaved(TableMetadata table, FieldMetadata field, QueryCriteriaField savedField)
    {
        var fieldControl = new CriteriaFieldControl(_criteriaPanel, table, field, savedField);
        fieldControl.RemoveRequested += (s, e) =>
        {
            _criteriaPanel.Controls.Remove(fieldControl);
            AutoSaveQuery(); // Auto-save when field removed
        };
        fieldControl.UserResizeCompleted += (s, e) => SnapshotQueryToSessionCache();
        fieldControl.ValueChanged += (s, e) => AutoSaveQuery(); // Auto-save when value changed
        return fieldControl;
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
        removeButton.Click += (s, e) =>
        {
            _displayPanel.Controls.Remove(fieldPanel);
            UpdateTablesInvolvedLabel(); // Update tables when field removed
            AutoSaveQuery(); // Auto-save when field removed
        };
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
        AutoSaveQuery(); // Auto-save when field added
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

    private void QueriesHeaderLabel_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Label label) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Draw rounded brass background (like Snap button)
        const int cornerRadius = 14;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            var rect = label.ClientRectangle;
            path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }
        
        // Draw text in blue centered
        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var size = g.MeasureString(label.Text, font);
            var x = (label.Width - size.Width) / 2;
            var y = (label.Height - size.Height) / 2;
            g.DrawString(label.Text, font, brush, x, y);
        }
    }

    private void DataHeaderLabel_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Label label) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Draw rounded brass background (like Snap button)
        const int cornerRadius = 14;
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            var rect = label.ClientRectangle;
            path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            using (var brush = new SolidBrush(_currentTheme.Accent))
            {
                g.FillPath(brush, path);
            }
        }
        
        // Draw text in blue centered
        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (var brush = new SolidBrush(_currentTheme.Primary))
        {
            var size = g.MeasureString(label.Text, font);
            var x = (label.Width - size.Width) / 2;
            var y = (label.Height - size.Height) / 2;
            g.DrawString(label.Text, font, brush, x, y);
        }
    }
    
    private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        
        // Top left arc
        path.AddArc(arc, 180, 90);
        
        // Top right arc
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        
        // Bottom right arc
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        
        // Bottom left arc
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        
        path.CloseFigure();
        return path;
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

    #region Excel-Style Filtering for ListViews

    private void TablesListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        ShowTablesFilterMenu(e.Column);
    }

    private void FieldsListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        ShowFieldsFilterMenu(e.Column);
    }

    private void ShowTablesFilterMenu(int columnIndex)
    {
        string columnName = columnIndex switch
        {
            0 => "Table Name",
            1 => "Last Scanned",
            2 => "Fields",
            _ => ""
        };

        if (string.IsNullOrEmpty(columnName)) return;

        // Get all unique values for this column from ALL items
        var allValues = new HashSet<string>();
        foreach (ListViewItem item in _allTableItems)
        {
            string value = item.SubItems[columnIndex].Text;
            if (!string.IsNullOrEmpty(value))
            {
                allValues.Add(value);
            }
        }

        if (allValues.Count == 0) return;

        // Get current filter selection for this column
        HashSet<string>? currentSelection = _tablesActiveFilters.ContainsKey(columnName)
            ? _tablesActiveFilters[columnName]
            : null;

        // Create filter window
        var filterContent = new FilterWindowContentForm(_currentTheme, columnName, allValues, currentSelection);

        filterContent.FilterApplied += (s, e) =>
        {
            if (e.SelectedValues == null)
            {
                _tablesActiveFilters.Remove(e.ColumnName);
            }
            else
            {
                _tablesActiveFilters[e.ColumnName] = e.SelectedValues;
            }
            ApplyTablesFilters();
        };

        var filterWindow = new BorderedWindowForm(
            _currentTheme,
            initialSize: new Size(350, 450),
            minimumSize: new Size(250, 300)
        );
        filterWindow.Text = $"Filter: {columnName}";
        filterWindow.SetContentForm(filterContent);

        // Position window near the header
        var screenPoint = _tablesListView.PointToScreen(new Point(0, 0));
        filterWindow.StartPosition = FormStartPosition.Manual;
        filterWindow.Location = new Point(screenPoint.X + 50, screenPoint.Y + 50);

        filterWindow.Show();
    }

    private void ShowFieldsFilterMenu(int columnIndex)
    {
        string columnName = columnIndex switch
        {
            0 => "Field Name",
            1 => "Data Type",
            2 => "Unique Values Scanned",
            3 => "Unique Values",
            _ => ""
        };

        if (string.IsNullOrEmpty(columnName)) return;

        // Get all unique values for this column from ALL items
        var allValues = new HashSet<string>();
        foreach (ListViewItem item in _allFieldItems)
        {
            string value = item.SubItems[columnIndex].Text;
            if (!string.IsNullOrEmpty(value))
            {
                allValues.Add(value);
            }
        }

        if (allValues.Count == 0) return;

        // Get current filter selection for this column
        HashSet<string>? currentSelection = _fieldsActiveFilters.ContainsKey(columnName)
            ? _fieldsActiveFilters[columnName]
            : null;

        // Create filter window
        var filterContent = new FilterWindowContentForm(_currentTheme, columnName, allValues, currentSelection);

        filterContent.FilterApplied += (s, e) =>
        {
            if (e.SelectedValues == null)
            {
                _fieldsActiveFilters.Remove(e.ColumnName);
            }
            else
            {
                _fieldsActiveFilters[e.ColumnName] = e.SelectedValues;
            }
            ApplyFieldsFilters();
        };

        var filterWindow = new BorderedWindowForm(
            _currentTheme,
            initialSize: new Size(350, 450),
            minimumSize: new Size(250, 300)
        );
        filterWindow.Text = $"Filter: {columnName}";
        filterWindow.SetContentForm(filterContent);

        // Position window near the header
        var screenPoint = _fieldsListView.PointToScreen(new Point(0, 0));
        filterWindow.StartPosition = FormStartPosition.Manual;
        filterWindow.Location = new Point(screenPoint.X + 50, screenPoint.Y + 50);

        filterWindow.Show();
    }

    private void ApplyTablesFilters()
    {
        _tablesListView.Items.Clear();

        foreach (ListViewItem item in _allTableItems)
        {
            bool matchesAllFilters = true;

            foreach (var filter in _tablesActiveFilters)
            {
                int columnIndex = filter.Key switch
                {
                    "Table Name" => 0,
                    "Last Scanned" => 1,
                    "Fields" => 2,
                    _ => -1
                };

                if (columnIndex == -1) continue;

                string itemValue = item.SubItems[columnIndex].Text;
                if (!filter.Value.Contains(itemValue))
                {
                    matchesAllFilters = false;
                    break;
                }
            }

            if (matchesAllFilters)
            {
                _tablesListView.Items.Add((ListViewItem)item.Clone());
            }
        }
    }

    private void ApplyFieldsFilters()
    {
        _fieldsListView.Items.Clear();

        foreach (ListViewItem item in _allFieldItems)
        {
            bool matchesAllFilters = true;

            foreach (var filter in _fieldsActiveFilters)
            {
                int columnIndex = filter.Key switch
                {
                    "Field Name" => 0,
                    "Data Type" => 1,
                    "Unique Values Scanned" => 2,
                    "Unique Values" => 3,
                    _ => -1
                };

                if (columnIndex == -1) continue;

                string itemValue = item.SubItems[columnIndex].Text;
                if (!filter.Value.Contains(itemValue))
                {
                    matchesAllFilters = false;
                    break;
                }
            }

            if (matchesAllFilters)
            {
                _fieldsListView.Items.Add((ListViewItem)item.Clone());
            }
        }
    }

    #endregion
}
