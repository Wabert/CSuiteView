using SuiteView.Models;

namespace SuiteView.Managers;

/// <summary>
/// Manages docking logic for the toolbar
/// Handles snapping to screen edges, orientation changes, and multi-monitor support
/// </summary>
public class DockingManager
{
    private const int SnapDistance = 50; // Pixels from edge to trigger snap
    private const int EdgeMargin = 0;    // Margin from screen edge when docked

    /// <summary>
    /// Calculates the docked position based on current window position and size
    /// </summary>
    /// <param name="windowLocation">Current window location</param>
    /// <param name="windowSize">Current window size</param>
    /// <param name="monitorIndex">Monitor index to dock to</param>
    /// <returns>Tuple of (new location, dock position, should dock)</returns>
    public static (Point Location, DockPosition DockPosition, bool ShouldDock) CalculateDockPosition(
        Point windowLocation,
        Size windowSize,
        int monitorIndex)
    {
        var screens = Screen.AllScreens;

        // Validate monitor index
        if (monitorIndex < 0 || monitorIndex >= screens.Length)
        {
            monitorIndex = 0;
        }

        var screen = screens[monitorIndex];
        var workingArea = screen.WorkingArea; // Working area excludes taskbar

        var centerX = windowLocation.X + windowSize.Width / 2;
        var centerY = windowLocation.Y + windowSize.Height / 2;

        // Check if window center is within this screen
        bool isInScreen = centerX >= workingArea.Left && centerX <= workingArea.Right &&
                          centerY >= workingArea.Top && centerY <= workingArea.Bottom;

        if (!isInScreen)
        {
            // Find which screen the window is actually on
            for (int i = 0; i < screens.Length; i++)
            {
                var testArea = screens[i].WorkingArea;
                if (centerX >= testArea.Left && centerX <= testArea.Right &&
                    centerY >= testArea.Top && centerY <= testArea.Bottom)
                {
                    screen = screens[i];
                    workingArea = testArea;
                    monitorIndex = i;
                    break;
                }
            }
        }

        // Calculate distances to each edge
        int distanceToLeft = Math.Abs(windowLocation.X - workingArea.Left);
        int distanceToRight = Math.Abs((windowLocation.X + windowSize.Width) - workingArea.Right);
        int distanceToTop = Math.Abs(windowLocation.Y - workingArea.Top);
        int distanceToBottom = Math.Abs((windowLocation.Y + windowSize.Height) - workingArea.Bottom);

        // Find minimum distance
        int minDistance = Math.Min(Math.Min(distanceToLeft, distanceToRight),
                                   Math.Min(distanceToTop, distanceToBottom));

        // Check if should dock (within snap distance)
        if (minDistance > SnapDistance)
        {
            return (windowLocation, DockPosition.None, false);
        }

        // Determine which edge to dock to
        DockPosition dockPosition;
        Point newLocation;

        if (minDistance == distanceToLeft)
        {
            // Dock to left edge
            newLocation = new Point(workingArea.Left + EdgeMargin, windowLocation.Y);

            // Check if near top or bottom corner
            if (distanceToTop < SnapDistance)
            {
                newLocation.Y = workingArea.Top + EdgeMargin;
                dockPosition = DockPosition.TopLeft;
            }
            else if (distanceToBottom < SnapDistance)
            {
                newLocation.Y = workingArea.Bottom - windowSize.Height - EdgeMargin;
                dockPosition = DockPosition.BottomLeft;
            }
            else
            {
                dockPosition = DockPosition.Left;
            }
        }
        else if (minDistance == distanceToRight)
        {
            // Dock to right edge
            newLocation = new Point(workingArea.Right - windowSize.Width - EdgeMargin, windowLocation.Y);

            // Check if near top or bottom corner
            if (distanceToTop < SnapDistance)
            {
                newLocation.Y = workingArea.Top + EdgeMargin;
                dockPosition = DockPosition.TopRight;
            }
            else if (distanceToBottom < SnapDistance)
            {
                newLocation.Y = workingArea.Bottom - windowSize.Height - EdgeMargin;
                dockPosition = DockPosition.BottomRight;
            }
            else
            {
                dockPosition = DockPosition.Right;
            }
        }
        else if (minDistance == distanceToTop)
        {
            // Dock to top edge
            newLocation = new Point(windowLocation.X, workingArea.Top + EdgeMargin);

            // Check if near left or right corner
            if (distanceToLeft < SnapDistance)
            {
                newLocation.X = workingArea.Left + EdgeMargin;
                dockPosition = DockPosition.TopLeft;
            }
            else if (distanceToRight < SnapDistance)
            {
                newLocation.X = workingArea.Right - windowSize.Width - EdgeMargin;
                dockPosition = DockPosition.TopRight;
            }
            else
            {
                dockPosition = DockPosition.Top;
            }
        }
        else // distanceToBottom
        {
            // Dock to bottom edge
            newLocation = new Point(windowLocation.X, workingArea.Bottom - windowSize.Height - EdgeMargin);

            // Check if near left or right corner
            if (distanceToLeft < SnapDistance)
            {
                newLocation.X = workingArea.Left + EdgeMargin;
                dockPosition = DockPosition.BottomLeft;
            }
            else if (distanceToRight < SnapDistance)
            {
                newLocation.X = workingArea.Right - windowSize.Width - EdgeMargin;
                dockPosition = DockPosition.BottomRight;
            }
            else
            {
                dockPosition = DockPosition.Bottom;
            }
        }

        // Ensure the window stays within screen bounds
        newLocation.X = Math.Max(workingArea.Left, Math.Min(newLocation.X, workingArea.Right - windowSize.Width));
        newLocation.Y = Math.Max(workingArea.Top, Math.Min(newLocation.Y, workingArea.Bottom - windowSize.Height));

        return (newLocation, dockPosition, true);
    }

    /// <summary>
    /// Gets the default position for a specific dock position
    /// </summary>
    public static Point GetDefaultDockPosition(DockPosition dockPosition, Size windowSize, int monitorIndex)
    {
        var screens = Screen.AllScreens;

        // Validate monitor index
        if (monitorIndex < 0 || monitorIndex >= screens.Length)
        {
            monitorIndex = 0;
        }

        var screen = screens[monitorIndex];
        var workingArea = screen.WorkingArea;

        return dockPosition switch
        {
            DockPosition.Top => new Point(workingArea.Left + (workingArea.Width - windowSize.Width) / 2, workingArea.Top + EdgeMargin),
            DockPosition.Bottom => new Point(workingArea.Left + (workingArea.Width - windowSize.Width) / 2, workingArea.Bottom - windowSize.Height - EdgeMargin),
            DockPosition.Left => new Point(workingArea.Left + EdgeMargin, workingArea.Top + (workingArea.Height - windowSize.Height) / 2),
            DockPosition.Right => new Point(workingArea.Right - windowSize.Width - EdgeMargin, workingArea.Top + (workingArea.Height - windowSize.Height) / 2),
            DockPosition.TopLeft => new Point(workingArea.Left + EdgeMargin, workingArea.Top + EdgeMargin),
            DockPosition.TopRight => new Point(workingArea.Right - windowSize.Width - EdgeMargin, workingArea.Top + EdgeMargin),
            DockPosition.BottomLeft => new Point(workingArea.Left + EdgeMargin, workingArea.Bottom - windowSize.Height - EdgeMargin),
            DockPosition.BottomRight => new Point(workingArea.Right - windowSize.Width - EdgeMargin, workingArea.Bottom - windowSize.Height - EdgeMargin),
            _ => new Point(workingArea.Right - windowSize.Width - EdgeMargin, workingArea.Bottom - windowSize.Height - EdgeMargin)
        };
    }

    /// <summary>
    /// Determines if the window should use horizontal or vertical orientation based on dock position
    /// </summary>
    public static bool IsHorizontalOrientation(DockPosition dockPosition)
    {
        return dockPosition == DockPosition.Top || dockPosition == DockPosition.Bottom;
    }

    /// <summary>
    /// Finds the monitor index that contains the given point
    /// </summary>
    public static int FindMonitorIndex(Point point)
    {
        var screens = Screen.AllScreens;

        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i].WorkingArea.Contains(point))
            {
                return i;
            }
        }

        return 0; // Default to primary monitor
    }

    /// <summary>
    /// Validates and corrects monitor index if the monitor is no longer available
    /// </summary>
    public static int ValidateMonitorIndex(int monitorIndex)
    {
        var screenCount = Screen.AllScreens.Length;

        if (monitorIndex < 0 || monitorIndex >= screenCount)
        {
            return 0; // Return primary monitor
        }

        return monitorIndex;
    }
}
