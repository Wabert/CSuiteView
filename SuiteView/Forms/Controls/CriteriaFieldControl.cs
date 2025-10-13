using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SuiteView.Forms; // RoundedPanel
using SuiteView.Models; // TableMetadata, FieldMetadata, QueryCriteriaField

namespace SuiteView.Forms.Controls
{
    public class CriteriaFieldControl : RoundedPanel
    {
        // Public data context
        public TableMetadata Table { get; }
        public FieldMetadata Field { get; }

        // State
        public bool UsesListBox { get; private set; }
        public CheckedListBox? ValuesListBox { get; private set; }
        public TextBox? ValueTextBox { get; private set; }
        public ComboBox? OperatorCombo { get; private set; }

    private readonly FlowLayoutPanel _parentFlow;
    private Panel? _resizeStrip;
    private bool _isResizing;
    private int _startY;
    private int _startHeight;
    private bool _parentAutoScrollPrev;
    private const int GripThreshold = 10;

    public event EventHandler? RemoveRequested;
    public event EventHandler? UserResizeCompleted;

        public CriteriaFieldControl(FlowLayoutPanel parentFlow, TableMetadata table, FieldMetadata field, QueryCriteriaField? saved = null)
        {
            _parentFlow = parentFlow;
            Table = table;
            Field = field;

            Width = parentFlow.ClientSize.Width - 20;
            Height = saved?.PanelHeight ?? 35;
            BackColor = Color.FromArgb(250, 250, 250);
            Margin = new Padding(5, 2, 5, 2);
            BorderColor = Color.FromArgb(220, 220, 220);
            BorderRadius = 8;

            BuildUI(saved);
        }

        private void BuildUI(QueryCriteriaField? saved)
        {
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
            removeButton.Click += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);
            Controls.Add(removeButton);

            // Field label
            var fieldLabel = new Label
            {
                Text = $"{Table.TableName}.{Field.FieldName}",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(35, 8),
                AutoSize = true
            };
            Controls.Add(fieldLabel);

            int contentLeft = 250;

            // Operator combo for string fields
            if (string.Equals(Field.DataType, "VARCHAR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Field.DataType, "NVARCHAR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Field.DataType, "CHAR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Field.DataType, "NCHAR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Field.DataType, "TEXT", StringComparison.OrdinalIgnoreCase))
            {
                OperatorCombo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(contentLeft, 6),
                    Size = new Size(160, 24),
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Standard
                };
                OperatorCombo.Items.AddRange(new object[] { "none", "equals", "contains", "begins with", "ends with" });
                OperatorCombo.SelectedIndex = 0;
                Controls.Add(OperatorCombo);
                contentLeft += 170;
            }

            // Decide between list box or text
            bool canUseListBox = Field.UniqueValues != null && Field.UniqueValues.Count > 0 && !Field.HasMoreThan200UniqueValues;
            UsesListBox = (saved?.HasListBox ?? canUseListBox);

            if (UsesListBox && Field.UniqueValues != null && !Field.HasMoreThan200UniqueValues)
            {
                ValuesListBox = new CheckedListBox
                {
                    Location = new Point(contentLeft, 5),
                    Size = new Size(Width - contentLeft - 10, Math.Max(64, Height - 16)),
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true,
                    IntegralHeight = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                };
                // (none) option
                bool noneChecked = saved?.SelectedValues?.Contains("(none)") == true;
                ValuesListBox.Items.Add("(none)", noneChecked);
                foreach (var v in Field.UniqueValues)
                {
                    bool isChecked = saved?.SelectedValues?.Contains(v) == true;
                    ValuesListBox.Items.Add(v, isChecked);
                }
                Controls.Add(ValuesListBox);

                AddResizeStrip();
            }
            else
            {
                ValueTextBox = new TextBox
                {
                    Location = new Point(contentLeft, 7),
                    Size = new Size(Width - contentLeft - 10, 23),
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 9f),
                    Text = saved?.TextValue ?? string.Empty
                };
                Controls.Add(ValueTextBox);
                // Still add strip to make height adjustable for future
                AddResizeStrip();
            }
        }

        private void AddResizeStrip()
        {
            _resizeStrip = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 12,
                BackColor = Color.FromArgb(242, 242, 242),
                Cursor = Cursors.SizeNS
            };

            // Visible handle glyph for discoverability
            var glyph = new Label
            {
                Dock = DockStyle.Fill,
                Text = "⋮⋮",
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.Gray,
                Padding = new Padding(0, 0, 6, 0),
                Cursor = Cursors.SizeNS,
                BackColor = Color.Transparent
            };
            _resizeStrip.Controls.Add(glyph);

            MouseEventHandler beginResize = (s, e) =>
            {
                _isResizing = true;
                _startY = Cursor.Position.Y;
                _startHeight = Height;
                _parentAutoScrollPrev = _parentFlow.AutoScroll;
                _parentFlow.AutoScroll = false;
                _resizeStrip!.Capture = true;
            };
            _resizeStrip.MouseDown += beginResize;
            glyph.MouseDown += beginResize;

            MouseEventHandler continueResize = (s, e) =>
            {
                if (!_isResizing) return;
                int deltaY = Cursor.Position.Y - _startY;
                int newHeight = Math.Max(80, _startHeight + deltaY);
                Height = newHeight;
                _parentFlow.PerformLayout();
                _parentFlow.Invalidate();
            };
            _resizeStrip.MouseMove += continueResize;
            glyph.MouseMove += continueResize;

            MouseEventHandler endResize = (s, e) =>
            {
                _isResizing = false;
                _resizeStrip!.Capture = false;
                _parentFlow.AutoScroll = _parentAutoScrollPrev;
                UserResizeCompleted?.Invoke(this, EventArgs.Empty);
            };
            _resizeStrip.MouseUp += endResize;
            glyph.MouseUp += endResize;

            Controls.Add(_resizeStrip);
        }

        // Helpers for extracting state for save/snapshot
        public QueryCriteriaField ToQueryCriteriaField()
        {
            var c = new QueryCriteriaField
            {
                TableName = Table.TableName,
                FieldName = Field.FieldName,
                DataType = Field.DataType,
                PanelHeight = Height,
                HasListBox = UsesListBox
            };

            if (UsesListBox && ValuesListBox != null)
            {
                c.SelectedValues = ValuesListBox.CheckedItems.Cast<string>().ToList();
            }
            else if (ValueTextBox != null)
            {
                c.TextValue = ValueTextBox.Text;
            }
            if (OperatorCombo != null)
            {
                c.StringOperator = OperatorCombo.SelectedItem?.ToString() ?? "none";
            }
            return c;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            // Keep list box height in sync with container height, leaving space for strip
            if (ValuesListBox != null)
            {
                ValuesListBox.Height = Math.Max(64, Height - 16);
                ValuesListBox.Width = Math.Max(50, Width - ValuesListBox.Left - 10);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isResizing)
            {
                int deltaY = Cursor.Position.Y - _startY;
                int newHeight = Math.Max(80, _startHeight + deltaY);
                Height = newHeight;
                _parentFlow.PerformLayout();
                _parentFlow.Invalidate();
                return;
            }
            // Change cursor near bottom edge anywhere in the control
            if (Height - e.Y <= GripThreshold)
            {
                Cursor = Cursors.SizeNS;
            }
            else if (!_isResizing)
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && Height - e.Y <= GripThreshold)
            {
                _isResizing = true;
                _startY = Cursor.Position.Y;
                _startHeight = Height;
                _parentAutoScrollPrev = _parentFlow.AutoScroll;
                _parentFlow.AutoScroll = false;
                Capture = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isResizing)
            {
                _isResizing = false;
                Capture = false;
                _parentFlow.AutoScroll = _parentAutoScrollPrev;
                UserResizeCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
