using System;
using System.Collections.Generic;
using System.Text;

namespace SelectPdf.Api
{
    /// <summary>
    /// Represents the mapping of a HTML element in the PDF document as collection of PDF 
    /// rectangles. A HTML element can span on many pages in the generated PDF document 
    /// and therefore, in general, many PDF rectangles are necessary to completely describe 
    /// the mapping of a HTML element in PDF.
    /// </summary>
    public class WebElement
    {
        /// <summary>
        /// The ID in HTML of the HTML element.
        /// </summary>
        public string HtmlElementId { get; set; }

        /// <summary>
        /// The rectangles occupied by the HTML element in the generated PDF document.
        /// A HTML element can span on many pages in the generated PDF document 
        /// and therefore, in general, many PDF rectangles are necessary to completely describe 
        /// the mapping of a HTML element in PDF.
        /// </summary>
        public WebElementPdfRectangle[] PdfRectangles { get; set; }

        /// <summary>
        /// The HTML tag name of the HTML element. 
        /// </summary>
        public string HtmlElementTagName { get; set; }

        /// <summary>
        /// The CSS class name of the HTML element. 
        /// </summary>
        public string HtmlElementCssClassName { get; set; }
    }

    /// <summary>
    /// Represents the rectangle occupied by a HTML element in a page of the generated PDF document.
    /// </summary>
    public class WebElementPdfRectangle
    {
        /// <summary>
        /// The zero based index of the PDF page containing this rectangle. 
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// The rectangle position inside the PDF page drawing area. The drawing area of the PDF page 
        /// does not include the page margins, header or footer. The rectangle dimensions are expressed 
        /// in points (1 point is 1/72 inch).
        /// </summary>
        public System.Drawing.RectangleF Rectangle { get; set; }
    }
}
