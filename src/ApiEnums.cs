using System;
using System.Collections.Generic;
using System.Text;

namespace SelectPdf.Api
{
    /// <summary>
    /// PDF page size.
    /// </summary>
    public enum PageSize
    {
        /// <summary>
        /// Custom page size.
        /// </summary>
        Custom,
        /// <summary>
        /// A1 page size.
        /// </summary>
        A1,
        /// <summary>
        /// A2 page size.
        /// </summary>
        A2,
        /// <summary>
        /// A3 page size.
        /// </summary>
        A3,
        /// <summary>
        /// A4 page size.
        /// </summary>
        A4,
        /// <summary>
        /// A5 page size.
        /// </summary>
        A5,
        /// <summary>
        /// Letter page size.
        /// </summary>
        Letter,
        /// <summary>
        /// Half Letter page size.
        /// </summary>
        HalfLetter,
        /// <summary>
        /// Ledger page size.
        /// </summary>
        Ledger,
        /// <summary>
        /// Legal page size.
        /// </summary>
        Legal
    }

    /// <summary>
    /// PDF page orientation.
    /// </summary>
    public enum PageOrientation
    {
        /// <summary>
        /// Portrait page orientation.
        /// </summary>
        Portrait,
        /// <summary>
        /// Landscape page orientation.
        /// </summary>
        Landscape
    }

    /// <summary>
    /// Rendering engine used for HTML to PDF conversion.
    /// </summary>
    public enum RenderingEngine
    {
        /// <summary>
        /// WebKit rendering engine.
        /// </summary>
        WebKit,
        /// <summary>
        /// WebKit Restricted rendering engine.
        /// </summary>
        Restricted,
        /// <summary>
        /// Blink rendering engine.
        /// </summary>
        Blink
    }

    /// <summary>
    /// Protocol used for secure (HTTPS) connections.
    /// </summary>
    public enum SecureProtocol
    {
        /// <summary>
        /// TLS 1.1 or newer. Recommended value.
        /// </summary>
        Tls11OrNewer = 0,
        /// <summary>
        /// TLS 1.0 only.
        /// </summary>
        Tls10 = 1,
        /// <summary>
        /// SSL v3 only.
        /// </summary>
        Ssl3 = 2
    }

    /// <summary>
    /// The page layout to be used when the pdf document is opened in a viewer. 
    /// </summary>
    public enum PageLayout
    {
        /// <summary>
        /// Displays one page at a time.
        /// </summary>
        SinglePage = 0,
        /// <summary>
        /// Displays the pages in one column.
        /// </summary>
        OneColumn = 1,
        /// <summary>
        /// Displays the pages in two columns, with odd-numbered pages on the left.
        /// </summary>
        TwoColumnLeft = 2,
        /// <summary>
        /// Displays the pages in two columns, with odd-numbered pages on the right.
        /// </summary>
        TwoColumnRight = 3
    }

    /// <summary>
    /// The PDF document's page mode. 
    /// </summary>
    public enum PageMode
    {
        /// <summary>
        /// Neither document outline (bookmarks) nor thumbnail images are visible. 
        /// </summary>
        UseNone = 0,
        /// <summary>
        /// Document outline (bookmarks) are visible.
        /// </summary>
        UseOutlines = 1,
        /// <summary>
        /// Thumbnail images are visible.
        /// </summary>
        UseThumbs = 2,
        /// <summary>
        /// Full-screen mode, with no menu bar, window controls or any other window visible.
        /// </summary>
        FullScreen = 3,
        /// <summary>
        /// Optional content group panel is visible.
        /// </summary>
        UseOC = 4,
        /// <summary>
        /// Document attachments are visible.
        /// </summary>
        UseAttachments = 5
    }

    /// <summary>
    /// Alignment for page numbers.
    /// </summary>
    public enum PageNumbersAlignment
    {
        /// <summary>
        /// Align left.
        /// </summary>
        Left = 1,
        /// <summary>
        /// Align center.
        /// </summary>
        Center = 2,
        /// <summary>
        /// Align right.
        /// </summary>
        Right = 3
    }

    /// <summary>
    /// Specifies the converter startup mode. 
    /// </summary>
    public enum StartupMode
    {
        /// <summary>
        /// The conversion starts right after the page loads.
        /// </summary>
        Automatic,
        /// <summary>
        /// The conversion starts only when called from JavaScript.
        /// </summary>
        Manual
    }


}
