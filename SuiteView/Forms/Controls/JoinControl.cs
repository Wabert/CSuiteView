using SuiteView.Models;

namespace SuiteView.Forms.Controls;

/// <summary>
/// Custom control for defining table joins with multiple conditions
/// </summary>
public class JoinControl : Panel
{
    private readonly ColorTheme _theme;
    private readonly List<string> _availableTables;
    private readonly DatabaseLibraryConfig _config;
    private ComboBox _leftTableCombo = null!;
    private ComboBox _joinTypeCombo = null!;
    private ComboBox _rightTableCombo = null!;
    private FlowLayoutPanel _conditionsPanel = null!;
    private Button _removeButton = null!;
    private readonly List<JoinConditionRow> _conditionRows = new();

    public event EventHandler? RemoveRequested;
    
    public string LeftTable => _leftTableCombo.SelectedItem?.ToString() ?? string.Empty;
    public string JoinType => _joinTypeCombo.SelectedItem?.ToString() ?? "INNER JOIN";
    public string RightTable => _rightTableCombo.SelectedItem?.ToString() ?? string.Empty;
    
    public List<(string leftField, string rightField)> Conditions
    {
        get => _conditionRows
            .Where(r => !string.IsNullOrWhiteSpace(r.LeftField) && !string.IsNullOrWhiteSpace(r.RightField))
            .Select(r => (r.LeftField, r.RightField))
            .ToList();
    }

    public JoinControl(ColorTheme theme, List<string> availableTables, DatabaseLibraryConfig config)
    {
        _theme = theme;
        _availableTables = availableTables;
        _config = config;
        
        this.BorderStyle = BorderStyle.FixedSingle;
        this.BackColor = Color.FromArgb(250, 250, 250);
        this.Padding = new Padding(10);
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.Margin = new Padding(0, 0, 0, 10);
        
        InitializeControls();
        AddConditionRow(); // Start with one condition row
    }

    private void InitializeControls()
    {
        // Remove button (top-right)
        _removeButton = new Button
        {
            Text = "✕",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.DarkRed,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(25, 25),
            Location = new Point(this.Width - 35, 5)
        };
        _removeButton.FlatAppearance.BorderSize = 0;
        _removeButton.Click += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        _removeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        
        // Left table combo
        _leftTableCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9f),
            Width = 150,
            Location = new Point(10, 10)
        };
        _leftTableCombo.Items.AddRange(_availableTables.ToArray());
        _leftTableCombo.SelectedIndexChanged += LeftTable_Changed;
        
        // Join type combo
        _joinTypeCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9f),
            Width = 150,
            Location = new Point(170, 10)
        };
        _joinTypeCombo.Items.AddRange(new[] { "INNER JOIN", "LEFT OUTER JOIN", "RIGHT OUTER JOIN" });
        _joinTypeCombo.SelectedIndex = 0;
        
        // Right table combo
        _rightTableCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9f),
            Width = 150,
            Location = new Point(330, 10)
        };
        _rightTableCombo.Items.AddRange(_availableTables.ToArray());
        _rightTableCombo.SelectedIndexChanged += RightTable_Changed;
        
        // Conditions panel (for ON clauses)
        _conditionsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Location = new Point(20, 45),
            Width = 600
        };
        
        this.Controls.Add(_removeButton);
        this.Controls.Add(_leftTableCombo);
        this.Controls.Add(_joinTypeCombo);
        this.Controls.Add(_rightTableCombo);
        this.Controls.Add(_conditionsPanel);
    }

    private void LeftTable_Changed(object? sender, EventArgs e)
    {
        UpdateConditionRowFields();
    }

    private void RightTable_Changed(object? sender, EventArgs e)
    {
        UpdateConditionRowFields();
    }

    private void UpdateConditionRowFields()
    {
        string leftTable = _leftTableCombo.SelectedItem?.ToString() ?? string.Empty;
        string rightTable = _rightTableCombo.SelectedItem?.ToString() ?? string.Empty;
        
        var leftFields = GetFieldsForTable(leftTable);
        var rightFields = GetFieldsForTable(rightTable);
        
        foreach (var row in _conditionRows)
        {
            row.UpdateAvailableFields(leftFields, rightFields);
        }
    }

    private void AddConditionRow()
    {
        string leftTable = _leftTableCombo.SelectedItem?.ToString() ?? string.Empty;
        string rightTable = _rightTableCombo.SelectedItem?.ToString() ?? string.Empty;
        
        var leftFields = GetFieldsForTable(leftTable);
        var rightFields = GetFieldsForTable(rightTable);
        
        var row = new JoinConditionRow(_theme, leftFields, rightFields);
        row.AddRequested += (s, e) => AddConditionRow();
        row.RemoveRequested += (s, e) => RemoveConditionRow(row);
        
        _conditionRows.Add(row);
        _conditionsPanel.Controls.Add(row);
        
        // Only show [+] button on the last row
        UpdateAddRemoveButtons();
    }

    private List<string> GetFieldsForTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return new List<string>();

        // Search through all databases for the table
        foreach (var db in _config.Databases)
        {
            var table = db.Tables.FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (table != null && table.Fields.Count > 0)
            {
                return table.Fields.Select(f => f.FieldName).OrderBy(f => f).ToList();
            }
        }

        return new List<string>();
    }

    private void RemoveConditionRow(JoinConditionRow row)
    {
        if (_conditionRows.Count <= 1)
            return; // Keep at least one condition
            
        _conditionRows.Remove(row);
        _conditionsPanel.Controls.Remove(row);
        row.Dispose();
        
        UpdateAddRemoveButtons();
    }

    private void UpdateAddRemoveButtons()
    {
        for (int i = 0; i < _conditionRows.Count; i++)
        {
            _conditionRows[i].ShowAddButton = (i == _conditionRows.Count - 1);
            _conditionRows[i].ShowRemoveButton = (_conditionRows.Count > 1);
        }
    }

    public void UpdateAvailableTables(List<string> tables)
    {
        _availableTables.Clear();
        _availableTables.AddRange(tables);
        
        string selectedLeft = _leftTableCombo.SelectedItem?.ToString() ?? string.Empty;
        string selectedRight = _rightTableCombo.SelectedItem?.ToString() ?? string.Empty;
        
        _leftTableCombo.Items.Clear();
        _leftTableCombo.Items.AddRange(tables.ToArray());
        
        _rightTableCombo.Items.Clear();
        _rightTableCombo.Items.AddRange(tables.ToArray());
        
        // Restore selections if still valid
        if (tables.Contains(selectedLeft))
            _leftTableCombo.SelectedItem = selectedLeft;
        if (tables.Contains(selectedRight))
            _rightTableCombo.SelectedItem = selectedRight;
    }
}

/// <summary>
/// Represents a single join condition row (field ON field)
/// </summary>
public class JoinConditionRow : Panel
{
    private readonly ColorTheme _theme;
    private ComboBox _leftFieldCombo = null!;
    private Label _onLabel = null!;
    private ComboBox _rightFieldCombo = null!;
    private Button _addButton = null!;
    private Button _removeButton = null!;
    
    public event EventHandler? AddRequested;
    public event EventHandler? RemoveRequested;
    
    public string LeftField => _leftFieldCombo.SelectedItem?.ToString() ?? string.Empty;
    public string RightField => _rightFieldCombo.SelectedItem?.ToString() ?? string.Empty;
    
    public bool ShowAddButton
    {
        get => _addButton.Visible;
        set => _addButton.Visible = value;
    }
    
    public bool ShowRemoveButton
    {
        get => _removeButton.Visible;
        set => _removeButton.Visible = value;
    }

    public JoinConditionRow(ColorTheme theme, List<string> leftFields, List<string> rightFields)
    {
        _theme = theme;
        
        this.Height = 35;
        this.Width = 600;
        this.Margin = new Padding(0, 2, 0, 2);
        
        InitializeControls();
        UpdateAvailableFields(leftFields, rightFields);
    }

    private void InitializeControls()
    {
        // Left field combo with auto-complete
        _leftFieldCombo = new ComboBox
        {
            Font = new Font("Segoe UI", 9f),
            Width = 180,
            Location = new Point(0, 5),
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems
        };
        
        // ON label
        _onLabel = new Label
        {
            Text = "ON",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = _theme.Primary,
            AutoSize = true,
            Location = new Point(190, 8)
        };
        
        // Right field combo with auto-complete
        _rightFieldCombo = new ComboBox
        {
            Font = new Font("Segoe UI", 9f),
            Width = 180,
            Location = new Point(220, 5),
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems
        };
        
        // Add button [+]
        _addButton = new Button
        {
            Text = "+",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(30, 25),
            Location = new Point(410, 5)
        };
        _addButton.FlatAppearance.BorderSize = 1;
        _addButton.FlatAppearance.BorderColor = Color.DarkGreen;
        _addButton.Click += (s, e) => AddRequested?.Invoke(this, EventArgs.Empty);
        
        // Remove button [-]
        _removeButton = new Button
        {
            Text = "−",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.DarkRed,
            BackColor = Color.LightCoral,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Size = new Size(30, 25),
            Location = new Point(445, 5)
        };
        _removeButton.FlatAppearance.BorderSize = 1;
        _removeButton.FlatAppearance.BorderColor = Color.DarkRed;
        _removeButton.Click += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        
        this.Controls.Add(_leftFieldCombo);
        this.Controls.Add(_onLabel);
        this.Controls.Add(_rightFieldCombo);
        this.Controls.Add(_addButton);
        this.Controls.Add(_removeButton);
    }

    public void UpdateAvailableFields(List<string> leftFields, List<string> rightFields)
    {
        // Save current selections
        string selectedLeft = _leftFieldCombo.Text;
        string selectedRight = _rightFieldCombo.Text;
        
        // Update left field combo
        _leftFieldCombo.Items.Clear();
        if (leftFields.Count > 0)
        {
            _leftFieldCombo.Items.AddRange(leftFields.ToArray());
            
            // Setup auto-complete
            var autoCompleteLeft = new AutoCompleteStringCollection();
            autoCompleteLeft.AddRange(leftFields.ToArray());
            _leftFieldCombo.AutoCompleteCustomSource = autoCompleteLeft;
            
            // Restore selection if still valid
            if (leftFields.Contains(selectedLeft))
                _leftFieldCombo.Text = selectedLeft;
        }
        
        // Update right field combo
        _rightFieldCombo.Items.Clear();
        if (rightFields.Count > 0)
        {
            _rightFieldCombo.Items.AddRange(rightFields.ToArray());
            
            // Setup auto-complete
            var autoCompleteRight = new AutoCompleteStringCollection();
            autoCompleteRight.AddRange(rightFields.ToArray());
            _rightFieldCombo.AutoCompleteCustomSource = autoCompleteRight;
            
            // Restore selection if still valid
            if (rightFields.Contains(selectedRight))
                _rightFieldCombo.Text = selectedRight;
        }
    }
}
