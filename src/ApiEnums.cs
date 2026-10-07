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
        /// A0 page size.
        /// </summary>
        A0,
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
        /// A6 page size.
        /// </summary>
        A6,
        /// <summary>
        /// A7 page size.
        /// </summary>
        A7,
        /// <summary>
        /// A8 page size.
        /// </summary>
        A8,
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
        Blink,
        /// <summary>
        /// Chromium rendering engine.
        /// </summary>
        Chromium
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

    /// <summary>
    /// The output text layout (for pdf to text calls).
    /// </summary>
    public enum TextLayout
    {
        /// <summary>
        /// The original layout of the text from the PDF document is preserved.
        /// </summary>
        Original,
        /// <summary>
        /// The text is produced in reading order.
        /// </summary>
        Reading
    };

    /// <summary>
    /// The output format (for pdf to text calls).
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>
        /// Text.
        /// </summary>
        Text,
        /// <summary>
        /// HTML.
        /// </summary>
        Html
    };

    /// <summary>
    /// PDF conformance target for the generated document.
    /// </summary>
    /// <remarks>
    /// Tagged standards (PdfA3A) require the Blink or Chromium rendering engine.
    /// When no engine is specified the API promotes the request to Chromium and
    /// reports the engine used in the X-SelectPdf-Engine response header.
    /// </remarks>
    public enum PdfStandard
    {
        /// <summary>
        /// The complete PDF feature set. Default.
        /// </summary>
        Full,
        /// <summary>
        /// PDF/A - long term archiving.
        /// </summary>
        PdfA,
        /// <summary>
        /// PDF/A-2B - long term archiving, transparencies allowed.
        /// </summary>
        PdfA2B,
        /// <summary>
        /// PDF/A-3A - the accessible level of PDF/A-3. Implies a tagged document
        /// and can carry a ZUGFeRD / Factur-X electronic invoice.
        /// </summary>
        PdfA3A,
        /// <summary>
        /// PDF/A-3B - long term archiving with arbitrary embedded files. Can
        /// carry a ZUGFeRD / Factur-X electronic invoice.
        /// </summary>
        PdfA3B,
        /// <summary>
        /// PDF/A-3U - PDF/A-3B with Unicode mapping for all text. Can carry a
        /// ZUGFeRD / Factur-X electronic invoice.
        /// </summary>
        PdfA3U,
        /// <summary>
        /// PDF/X - graphics exchange.
        /// </summary>
        PdfX,
        /// <summary>
        /// PDF/SiqQ Level A - suitable for digital signatures, external links disabled.
        /// </summary>
        PdfSiqQ_A,
        /// <summary>
        /// PDF/SiqQ Level B - suitable for digital signatures.
        /// </summary>
        PdfSiqQ_B
    };

    /// <summary>
    /// The data profile of a ZUGFeRD / Factur-X hybrid electronic invoice. The
    /// profile determines how much of the EN 16931 semantic model the embedded
    /// XML carries.
    /// </summary>
    public enum ZugferdProfile
    {
        /// <summary>
        /// MINIMUM - accounting information only. Not a complete invoice.
        /// </summary>
        Minimum,
        /// <summary>
        /// BASIC WL - header and footer data without invoice lines. Not a complete invoice.
        /// </summary>
        Basic_WL,
        /// <summary>
        /// BASIC - a subset of EN 16931 covering simple invoices, with lines.
        /// </summary>
        Basic,
        /// <summary>
        /// EN 16931 (formerly COMFORT) - the full European semantic standard.
        /// </summary>
        En16931,
        /// <summary>
        /// EXTENDED - EN 16931 plus additional business terms.
        /// </summary>
        Extended,
        /// <summary>
        /// XRECHNUNG - the German public-sector reference profile. The embedded
        /// file is named xrechnung.xml instead of factur-x.xml.
        /// </summary>
        XRechnung
    };

    /// <summary>
    /// How the embedded invoice XML relates to the visible invoice page.
    /// </summary>
    /// <remarks>
    /// When not set, the API derives this from the profile: Alternative for
    /// Minimum and Basic_WL, Data for the rest. Minimum and Basic_WL combined
    /// with Data are rejected, because those profiles do not carry a complete
    /// invoice.
    /// </remarks>
    public enum ZugferdRelationship
    {
        /// <summary>
        /// The XML and the visible page carry exactly the same invoice content.
        /// Mandatory in Germany for the Basic, En16931, Extended and XRechnung profiles.
        /// </summary>
        Data,
        /// <summary>
        /// The visible page carries more than the XML does - always the case for
        /// the Minimum and Basic_WL profiles - or the page was generated from the XML.
        /// </summary>
        Alternative,
        /// <summary>
        /// The XML is the source the visible page was produced from.
        /// </summary>
        Source,
        /// <summary>
        /// The XML supplements the visible page.
        /// </summary>
        Supplement
    };

    /// <summary>
    /// The metadata schema used to identify a hybrid invoice inside the PDF.
    /// </summary>
    public enum ZugferdSchema
    {
        /// <summary>
        /// Factur-X 1.0 / ZUGFeRD 2.x - the current schema. Default.
        /// </summary>
        FacturX10,
        /// <summary>
        /// ZUGFeRD 2.0 - the legacy schema, deprecated but still accepted. Use
        /// only for recipients that explicitly require it.
        /// </summary>
        Zugferd20
    };

}
