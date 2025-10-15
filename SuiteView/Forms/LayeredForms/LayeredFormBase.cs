using System;
using System.Windows.Forms;
using System.Drawing;
using SuiteView.Models;

namespace SuiteView.Forms.LayeredForms;

/// <summary>
/// Abstract base class for all layered forms in SuiteView
/// Provides common functionality: brass borders, close button, form dragging
/// All layered forms are wrapped in BorderedWindowForm and have consistent styling
/// </summary>
public abstract class LayeredFormBase : Form, IContentForm
{
    protected readonly ColorTheme _theme;
    protected Form? _parentBorderForm;
    protected BorderedWindowForm? _borderForm;  // The wrapper form we create internally
    protected Button? _closeButton;
    
    // Form dragging state
    protected bool _isFormDragging = false;
    protected Point _formDragStartPoint;
    protected Point _formDragParentStart;
    
    // Layer 1 dimensions (Form Container)
    protected readonly int _layer1HeaderHeight;
    protected readonly int _layer1FooterHeight;
    
    protected LayeredFormBase(
        int formWidth = 1000,
        int formHeight = 600,
        int layer1HeaderHeight = 30,
        int layer1FooterHeight = 30,
        ColorTheme? theme = null,
        Size? minimumSize = null,
        bool createBorderForm = true)  // Allow derived classes to opt out if needed
    {
        _theme = theme ?? Managers.ThemeManager.GetTheme("Royal Classic");
        _layer1HeaderHeight = layer1HeaderHeight;
        _layer1FooterHeight = layer1FooterHeight;
        
        // Form setup (this is the content form)
        this.Width = formWidth;
        this.Height = formHeight;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = _theme.Primary;  // Layer 1 = Royal blue
        
        // Enable double buffering
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                      ControlStyles.AllPaintingInWmPaint | 
                      ControlStyles.UserPaint | 
                      ControlStyles.ResizeRedraw, true);
        this.UpdateStyles();
        
        // Wire up common events
        this.MouseDown += Form_MouseDown;
        this.MouseMove += Form_MouseMove;
        this.MouseUp += Form_MouseUp;
        
        // Create the border form wrapper internally
        if (createBorderForm)
        {
            _borderForm = new BorderedWindowForm(
                theme: _theme,
                initialSize: new Size(formWidth, formHeight),
                minimumSize: minimumSize ?? new Size(100, 100));
            
            // Set up the parent-child relationship
            _borderForm.SetContentForm(this);
            _parentBorderForm = _borderForm;
        }
    }
    
    /// <summary>
    /// Set the parent BorderedWindowForm (called by wrapper)
    /// </summary>
    public void SetParentBorderForm(Form parent)
    {
        _parentBorderForm = parent;
    }
    
    /// <summary>
    /// Initialize the close button in the center of Layer 1 header
    /// </summary>
    protected void InitializeCloseButton()
    {
        _closeButton = new Button
        {
            Text = "",
            Size = new Size(24, 24),
            Location = new Point(this.ClientSize.Width - 34, (_layer1HeaderHeight / 2) - 12),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _closeButton.FlatAppearance.BorderSize = 0;
        _closeButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        _closeButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        _closeButton.Click += CloseButton_Click;
        _closeButton.Paint += CloseButton_Paint;
        this.Controls.Add(_closeButton);
        _closeButton.BringToFront();
    }
    
    #region Form Dragging
    
    protected virtual void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _parentBorderForm != null)
        {
            // Don't drag if clicking on close button
            Point clientPos = this.PointToClient(Cursor.Position);
            if (_closeButton != null && _closeButton.Bounds.Contains(clientPos))
            {
                return;
            }
            
            // Allow derived classes to prevent dragging in specific areas
            if (ShouldPreventDragging(clientPos))
            {
                return;
            }

            _isFormDragging = true;
            _formDragStartPoint = Cursor.Position;
            _formDragParentStart = _parentBorderForm.Location;
        }
    }
    
    protected virtual void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isFormDragging && e.Button == MouseButtons.Left && _parentBorderForm != null)
        {
            int deltaX = Cursor.Position.X - _formDragStartPoint.X;
            int deltaY = Cursor.Position.Y - _formDragStartPoint.Y;
            _parentBorderForm.Location = new Point(_formDragParentStart.X + deltaX, _formDragParentStart.Y + deltaY);
        }
    }
    
    protected virtual void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isFormDragging)
        {
            _isFormDragging = false;
        }
    }
    
    /// <summary>
    /// Override in derived classes to prevent dragging in specific areas (e.g., splitters)
    /// </summary>
    protected virtual bool ShouldPreventDragging(Point clientPos)
    {
        return false;
    }
    
    #endregion
    
    #region Close Button
    
    protected virtual void CloseButton_Click(object? sender, EventArgs e)
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
    
    protected virtual void CloseButton_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(2, 2, 20, 20);
        
        // Draw brass circle
        using (var brush = new SolidBrush(_theme.Accent))
        {
            g.FillEllipse(brush, rect);
        }

        // Draw X in royal blue
        using (var pen = new Pen(_theme.Primary, 2))
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int size = 6;
            g.DrawLine(pen, centerX - size, centerY - size, centerX + size, centerY + size);
            g.DrawLine(pen, centerX + size, centerY - size, centerX - size, centerY + size);
        }
    }
    
    #endregion
    
    #region Public Methods for BorderedWindowForm Access
    
    /// <summary>
    /// Show the form (shows the border form wrapper)
    /// </summary>
    public new void Show()
    {
        if (_borderForm != null)
        {
            _borderForm.Show();
        }
        else
        {
            base.Show();
        }
    }
    
    /// <summary>
    /// Hide the form (hides the border form wrapper)
    /// </summary>
    public new void Hide()
    {
        if (_borderForm != null)
        {
            _borderForm.Hide();
        }
        else
        {
            base.Hide();
        }
    }
    
    /// <summary>
    /// Close the form (closes the border form wrapper)
    /// </summary>
    public new void Close()
    {
        if (_borderForm != null)
        {
            _borderForm.Close();
        }
        else
        {
            base.Close();
        }
    }
    
    /// <summary>
    /// Get whether the form is visible (checks border form wrapper)
    /// </summary>
    public new bool Visible
    {
        get => _borderForm?.Visible ?? base.Visible;
        set
        {
            if (_borderForm != null)
            {
                _borderForm.Visible = value;
            }
            else
            {
                base.Visible = value;
            }
        }
    }
    
    /// <summary>
    /// Activate the form (activates the border form wrapper)
    /// </summary>
    public new void Activate()
    {
        if (_borderForm != null)
        {
            _borderForm.Activate();
        }
        else
        {
            base.Activate();
        }
    }
    
    /// <summary>
    /// Set the window state (delegates to border form wrapper)
    /// </summary>
    public new FormWindowState WindowState
    {
        get => _borderForm?.WindowState ?? base.WindowState;
        set
        {
            if (_borderForm != null)
            {
                _borderForm.WindowState = value;
            }
            else
            {
                base.WindowState = value;
            }
        }
    }
    
    /// <summary>
    /// Get the border form (for advanced operations if needed)
    /// </summary>
    public BorderedWindowForm? BorderForm => _borderForm;
    
    #endregion
    
    /// <summary>
    /// Enable double buffering on a control
    /// </summary>
    protected void SetDoubleBuffered(Control control)
    {
        typeof(Control).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic,
            null, control, new object[] { true });
    }
}
