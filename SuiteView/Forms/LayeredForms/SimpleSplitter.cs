using System;
using System.Windows.Forms;
using System.Drawing;

namespace SuiteView.Forms.LayeredForms;

/// <summary>
/// A simple draggable splitter control for resizing adjacent panels
/// </summary>
public class SimpleSplitter : Panel
{
    private bool _isDragging = false;
    private int _dragStartX = 0;
    private int _initialLeft = 0;
    
    public event EventHandler<SplitterEventArgs>? SplitterMoving;
    
    public class SplitterEventArgs : EventArgs
    {
        public int SplitterX { get; set; }
        public SplitterEventArgs(int x) => SplitterX = x;
    }
    
    public SimpleSplitter()
    {
        this.Cursor = Cursors.SizeWE;
        this.Width = 4;
        
        this.MouseDown += SimpleSplitter_MouseDown;
        this.MouseMove += SimpleSplitter_MouseMove;
        this.MouseUp += SimpleSplitter_MouseUp;
    }
    
    private void SimpleSplitter_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragStartX = Cursor.Position.X;
            _initialLeft = this.Left;
        }
    }
    
    private void SimpleSplitter_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging && e.Button == MouseButtons.Left)
        {
            int deltaX = Cursor.Position.X - _dragStartX;
            int newX = _initialLeft + deltaX;
            
            SplitterMoving?.Invoke(this, new SplitterEventArgs(newX));
        }
    }
    
    private void SimpleSplitter_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
        }
    }
}
