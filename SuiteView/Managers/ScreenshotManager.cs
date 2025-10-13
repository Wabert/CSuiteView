using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SuiteView.Managers;

/// <summary>
/// Manages screenshot capture functionality
/// </summary>
public class ScreenshotManager
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

    /// <summary>
    /// Captures a screenshot of the primary monitor
    /// </summary>
    /// <returns>Bitmap of the screen capture</returns>
    public static Bitmap CaptureScreen()
    {
        var bounds = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            var hdcDest = graphics.GetHdc();
            var hdcSrc = GetDC(IntPtr.Zero);

            try
            {
                BitBlt(hdcDest, 0, 0, bounds.Width, bounds.Height,
                    hdcSrc, bounds.X, bounds.Y, CopyPixelOperation.SourceCopy);
            }
            finally
            {
                graphics.ReleaseHdc(hdcDest);
                ReleaseDC(IntPtr.Zero, hdcSrc);
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Saves a bitmap to a temporary file and returns the path
    /// </summary>
    /// <param name="bitmap">The bitmap to save</param>
    /// <returns>Path to the temporary file</returns>
    public static string SaveToTempFile(Bitmap bitmap)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"SuiteView_Screenshot_{Guid.NewGuid()}.png");
        bitmap.Save(tempPath, ImageFormat.Png);
        return tempPath;
    }
}
