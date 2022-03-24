using System;
using System.Collections.Generic;
using System.Text;

namespace SelectPdf.Api
{
	/// <summary>
	/// The text position in a PDF document.
	/// </summary>
	public class TextPosition
	{

		/// <summary>
		/// Creates a TextPosition object.
		/// </summary>
		public TextPosition()
		{
			PageNumber = 0;
			X = 0;
			Y = 0;
			Width = 0;
			Height = 0;
		}

		/// <summary>
		/// Number of the page where the text is located (1-based).
		/// </summary>
		public int PageNumber { get; set; }

		/// <summary>
		/// X coordinate of the text rectangle in the PDF page.
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// Y coordinate of the text rectangle in the PDF page.
		/// </summary>
		public float Y { get; set; }

		/// <summary>
		/// Width of the text rectangle in the PDF page.
		/// </summary>
		public float Width { get; set; }

		/// <summary>
		/// Height of the text rectangle in the PDF page.
		/// </summary>
		public float Height { get; set; }

		/// <summary>
		/// ToString override.
		/// </summary>
		/// <returns>String representation of the object.</returns>
		public override string ToString()
        {
			return string.Format("Page: {0} - [X: {1}, Y: {2}, Width: {3}, Height: {4}]", PageNumber, X, Y, Width, Height);
        }
	}
}
