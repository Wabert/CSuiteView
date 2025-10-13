using System.Runtime.InteropServices;

namespace SuiteView.Managers;

/// <summary>
/// Manages Word document operations for screenshot insertion using late binding
/// </summary>
public class WordDocumentManager : IDisposable
{
    private dynamic? _wordApp;
    private dynamic? _document;
    private int _screenshotCount = 0;
    private bool _disposed = false;

    // Constants for Word enumerations
    private const int WdCollapseEnd = 0; // wdCollapseEnd

    /// <summary>
    /// Gets whether a Word document is currently open
    /// </summary>
    public bool IsDocumentOpen => _document != null;

    /// <summary>
    /// Creates or retrieves the Word document for screenshots
    /// </summary>
    public void EnsureDocumentOpen()
    {
        if (_document != null)
            return;

        try
        {
            // Create Word application using late binding
            Type? wordType = Type.GetTypeFromProgID("Word.Application");
            if (wordType == null)
            {
                throw new InvalidOperationException("Microsoft Word is not installed on this system.");
            }

            _wordApp = Activator.CreateInstance(wordType);
            _wordApp.Visible = true;

            // Create new document
            _document = _wordApp.Documents.Add();

            // Set minimum margins (0.5 inches = 36 points)
            _document.PageSetup.TopMargin = 36f;
            _document.PageSetup.BottomMargin = 36f;
            _document.PageSetup.LeftMargin = 36f;
            _document.PageSetup.RightMargin = 36f;

            _screenshotCount = 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create Word document. Please ensure Microsoft Word is installed.", ex);
        }
    }

    /// <summary>
    /// Adds a screenshot to the Word document
    /// </summary>
    /// <param name="imagePath">Path to the screenshot image</param>
    public void AddScreenshot(string imagePath)
    {
        if (_document == null || _wordApp == null)
            throw new InvalidOperationException("Document is not open");

        try
        {
            // Calculate if we need a page break (2 screenshots per page)
            if (_screenshotCount > 0 && _screenshotCount % 2 == 0)
            {
                // Add page break after every 2 screenshots
                _document.Content.InsertAfter("\f"); // \f is page break
                var range = _document.Range();
                range.Collapse(WdCollapseEnd);
                range.Select();
            }

            // Move to end of document
            var endRange = _document.Content;
            endRange.Collapse(WdCollapseEnd);

            // Insert the image - using named parameters for clarity
            var inlineShape = _document.InlineShapes.AddPicture(
                imagePath,  // FileName
                false,      // LinkToFile
                true,       // SaveWithDocument
                endRange    // Range
            );

            // Calculate width to fit 2 images per page
            // Page width = 8.5 inches - 1 inch margins = 7.5 inches = 540 points
            const float pageWidth = 540f;
            const float imageWidth = pageWidth; // Full width for each screenshot

            // Scale the image proportionally
            float aspectRatio = (float)inlineShape.Height / (float)inlineShape.Width;
            inlineShape.Width = imageWidth;
            inlineShape.Height = imageWidth * aspectRatio;

            // Add a line break after each image for spacing
            _document.Content.InsertAfter("\n");

            _screenshotCount++;

            // Clean up temp file
            try
            {
                File.Delete(imagePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add screenshot to Word document: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if the Word document is still open
    /// </summary>
    /// <returns>True if document is still open, false otherwise</returns>
    public bool CheckDocumentOpen()
    {
        if (_document == null || _wordApp == null)
            return false;

        try
        {
            // Try to access document property - will throw if closed
            var _ = _document.Saved;
            return true;
        }
        catch (COMException)
        {
            // Document was closed
            _document = null;
            _wordApp = null;
            _screenshotCount = 0;
            return false;
        }
    }

    /// <summary>
    /// Releases COM objects
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_document != null)
        {
            Marshal.ReleaseComObject(_document);
            _document = null;
        }

        if (_wordApp != null)
        {
            Marshal.ReleaseComObject(_wordApp);
            _wordApp = null;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~WordDocumentManager()
    {
        Dispose();
    }
}
