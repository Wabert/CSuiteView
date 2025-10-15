using SuiteView.Forms.LayeredForms;
using SuiteView.Models;
using System.Drawing;
using System.Windows.Forms;

namespace SuiteView.Forms;

/// <summary>
/// Form Builder - Interactive tool for creating and testing layered forms
/// Built using TwoLayerForm for consistent architecture
/// </summary>
public class FormBuilderForm : TwoLayerForm
{
    // Form Builder UI controls
    private ComboBox _formTypeComboBox = null!;
    private NumericUpDown _formWidthInput = null!;
    private NumericUpDown _formHeightInput = null!;
    private NumericUpDown _layer1HeaderInput = null!;
    private NumericUpDown _layer1FooterInput = null!;
    private NumericUpDown _layer2HeaderInput = null!;
    private NumericUpDown _layer2FooterInput = null!;
    private NumericUpDown _layer2BorderInput = null!;
    private NumericUpDown _panelCountInput = null!;
    private Button _createFormButton = null!;
    private Button _oldFormButton = null!;

    public FormBuilderForm(ColorTheme? theme = null)
        : base(
            formWidth: 400,
            formHeight: 680,
            layer1HeaderHeight: 30,
            layer1FooterHeight: 30,
            theme: theme ?? Managers.ThemeManager.GetTheme("Royal Classic"))
    {
        InitializeFormBuilderControls();
    }

    private void InitializeFormBuilderControls()
    {
        var contentPanel = this.ContentPanel;
        contentPanel.AutoScroll = true;

        // Title label
        var titleLabel = new Label
        {
            Text = "Layered Form Builder",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = _theme.Accent,
            Location = new Point(20, 15),
            AutoSize = true
        };
        contentPanel.Controls.Add(titleLabel);

        // Description
        var descLabel = new Label
        {
            Text = "Configure and preview layered forms",
            Font = new Font("Segoe UI", 9f),
            ForeColor = _theme.Accent,
            Location = new Point(20, 45),
            AutoSize = true
        };
        contentPanel.Controls.Add(descLabel);

        int currentY = 80;

        // Form Type ComboBox
        var formTypeLabel = new Label
        {
            Text = "Form Type:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold)
        };
        contentPanel.Controls.Add(formTypeLabel);
        
        _formTypeComboBox = new ComboBox
        {
            Location = new Point(20, currentY + 25),
            Width = 340,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary
        };
        _formTypeComboBox.Items.AddRange(new[] { "TwoLayerForm", "ThreeLayerForm" });
        _formTypeComboBox.SelectedIndex = 0;
        _formTypeComboBox.SelectedIndexChanged += FormTypeComboBox_SelectedIndexChanged;
        contentPanel.Controls.Add(_formTypeComboBox);
        currentY += 65;

        // Form Width
        var formWidthLabel = new Label
        {
            Text = "Form Width:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent
        };
        contentPanel.Controls.Add(formWidthLabel);
        
        _formWidthInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 300,
            Maximum = 3000,
            Value = 1000,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary
        };
        contentPanel.Controls.Add(_formWidthInput);
        currentY += 55;

        // Form Height
        var formHeightLabel = new Label
        {
            Text = "Form Height:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent
        };
        contentPanel.Controls.Add(formHeightLabel);
        
        _formHeightInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 200,
            Maximum = 2000,
            Value = 600,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary
        };
        contentPanel.Controls.Add(_formHeightInput);
        currentY += 55;

        // Layer 1 Header Height
        var layer1HeaderLabel = new Label
        {
            Text = "Layer 1 Header Height:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent
        };
        contentPanel.Controls.Add(layer1HeaderLabel);
        
        _layer1HeaderInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 0,
            Maximum = 100,
            Value = 30,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary
        };
        contentPanel.Controls.Add(_layer1HeaderInput);
        currentY += 55;

        // Layer 1 Footer Height
        var layer1FooterLabel = new Label
        {
            Text = "Layer 1 Footer Height:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent
        };
        contentPanel.Controls.Add(layer1FooterLabel);
        
        _layer1FooterInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 0,
            Maximum = 100,
            Value = 30,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary
        };
        contentPanel.Controls.Add(_layer1FooterInput);
        currentY += 55;

        // Layer 2 Header Height (ThreeLayerForm only)
        var layer2HeaderLabel = new Label
        {
            Text = "Layer 2 Header Height:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(layer2HeaderLabel);
        
        _layer2HeaderInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(_layer2HeaderInput);
        currentY += 55;

        // Layer 2 Footer Height (ThreeLayerForm only)
        var layer2FooterLabel = new Label
        {
            Text = "Layer 2 Footer Height:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(layer2FooterLabel);
        
        _layer2FooterInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(_layer2FooterInput);
        currentY += 55;

        // Layer 2 Border Width (ThreeLayerForm only)
        var layer2BorderLabel = new Label
        {
            Text = "Layer 2 Border Width:",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(layer2BorderLabel);
        
        _layer2BorderInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 0,
            Maximum = 50,
            Value = 15,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(_layer2BorderInput);
        currentY += 55;

        // Panel Count (ThreeLayerForm only)
        var panelCountLabel = new Label
        {
            Text = "Panel Count (1-4):",
            Location = new Point(20, currentY),
            AutoSize = true,
            ForeColor = _theme.Accent,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(panelCountLabel);
        
        _panelCountInput = new NumericUpDown
        {
            Location = new Point(20, currentY + 20),
            Width = 340,
            Minimum = 1,
            Maximum = 4,
            Value = 3,
            BackColor = _theme.LightBlue,
            ForeColor = _theme.Primary,
            Tag = "ThreeLayer",
            Visible = false
        };
        contentPanel.Controls.Add(_panelCountInput);
        currentY += 70;

        // Separator line
        var separator = new Label
        {
            Location = new Point(20, currentY),
            Size = new Size(340, 2),
            BackColor = _theme.Accent,
            BorderStyle = BorderStyle.None
        };
        contentPanel.Controls.Add(separator);
        currentY += 20;

        // Create Form Button
        _createFormButton = new Button
        {
            Text = "Create Form Preview",
            Location = new Point(20, currentY),
            Size = new Size(340, 40),
            BackColor = _theme.Accent,
            ForeColor = _theme.Primary,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _createFormButton.FlatAppearance.BorderSize = 0;
        _createFormButton.Click += CreateFormButton_Click;
        contentPanel.Controls.Add(_createFormButton);
        currentY += 50;

        // Old Form (Compare) Button
        _oldFormButton = new Button
        {
            Text = "Old Form (Compare)",
            Location = new Point(20, currentY),
            Size = new Size(340, 35),
            BackColor = ControlPaint.Dark(_theme.Secondary, 0.1f),
            ForeColor = _theme.Accent,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _oldFormButton.FlatAppearance.BorderColor = _theme.Accent;
        _oldFormButton.FlatAppearance.BorderSize = 1;
        _oldFormButton.Click += OldFormButton_Click;
        contentPanel.Controls.Add(_oldFormButton);
    }

    private void FormTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        bool isThreeLayer = _formTypeComboBox.SelectedIndex == 1;

        // Show/hide ThreeLayer-specific controls
        foreach (Control control in this.ContentPanel.Controls)
        {
            if (control.Tag?.ToString() == "ThreeLayer")
            {
                control.Visible = isThreeLayer;
            }
        }
    }

    private void CreateFormButton_Click(object? sender, EventArgs e)
    {
        BorderedWindowForm? form = null;

        if (_formTypeComboBox.SelectedIndex == 0) // TwoLayerForm
        {
            form = LayeredFormFactory.CreateTwoLayerForm(
                formWidth: (int)_formWidthInput.Value,
                formHeight: (int)_formHeightInput.Value,
                layer1HeaderHeight: (int)_layer1HeaderInput.Value,
                layer1FooterHeight: (int)_layer1FooterInput.Value,
                minimumSize: new Size(300, 200),
                theme: _theme);

            form.Text = "TwoLayerForm Preview";
        }
        else // ThreeLayerForm
        {
            form = LayeredFormFactory.CreateThreeLayerForm(
                formWidth: (int)_formWidthInput.Value,
                formHeight: (int)_formHeightInput.Value,
                layer1HeaderHeight: (int)_layer1HeaderInput.Value,
                layer1FooterHeight: (int)_layer1FooterInput.Value,
                layer2HeaderHeight: (int)_layer2HeaderInput.Value,
                layer2FooterHeight: (int)_layer2FooterInput.Value,
                layer2BorderWidth: (int)_layer2BorderInput.Value,
                panelCount: (int)_panelCountInput.Value,
                minimumSize: new Size(300, 200),
                theme: _theme);

            form.Text = $"ThreeLayerForm Preview ({_panelCountInput.Value} Panels)";
        }

        form?.Show();
    }

    private void OldFormButton_Click(object? sender, EventArgs e)
    {
        var contentForm = new IndependentThreePanelForm(
            panelCount: (int)_panelCountInput.Value,
            showHeader: true,
            showFooter: true,
            theme: _theme);

        var borderedWindow = new BorderedWindowForm(
            theme: _theme,
            initialSize: new Size((int)_formWidthInput.Value, (int)_formHeightInput.Value),
            minimumSize: new Size(300, 200));

        borderedWindow.SetContentForm(contentForm);
        borderedWindow.Text = $"OLD {_panelCountInput.Value}-Panel Form (IndependentThreePanelForm)";
        borderedWindow.Show();
    }
}
