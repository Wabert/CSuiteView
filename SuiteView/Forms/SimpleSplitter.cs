
using System;
using System.Windows.Forms;
using System.Drawing;

public class SimpleSplitter : Form
{
    public SimpleSplitter()
    {
        this.Text = "Simple 3-Panel Splitter";
        this.Width = 900;
        this.Height = 600;

        // First SplitContainer: Left and Right
        SplitContainer splitLeftRight = new SplitContainer();
        splitLeftRight.Dock = DockStyle.Fill;
        splitLeftRight.Orientation = Orientation.Vertical;
        splitLeftRight.SplitterDistance = 300;

        // Left Panel
        Panel leftPanel = new Panel();
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.BackColor = Color.LightBlue;
        splitLeftRight.Panel1.Controls.Add(leftPanel);

        // Second SplitContainer: Middle and Right
        SplitContainer splitMiddleRight = new SplitContainer();
        splitMiddleRight.Dock = DockStyle.Fill;
        splitMiddleRight.Orientation = Orientation.Vertical;
        splitMiddleRight.SplitterDistance = 300;

        // Middle Panel
        Panel middlePanel = new Panel();
        middlePanel.Dock = DockStyle.Fill;
        middlePanel.BackColor = Color.LightGreen;
        splitMiddleRight.Panel1.Controls.Add(middlePanel);

        // Right Panel
        Panel rightPanel = new Panel();
        rightPanel.Dock = DockStyle.Fill;
        rightPanel.BackColor = Color.LightCoral;
        splitMiddleRight.Panel2.Controls.Add(rightPanel);

        // Add middle-right splitter to right panel of left-right splitter
        splitLeftRight.Panel2.Controls.Add(splitMiddleRight);

        // Add the outer splitter to the form
        this.Controls.Add(splitLeftRight);
    }
}
