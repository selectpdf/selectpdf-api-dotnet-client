using System;
using System.Collections.Generic;
using System.Text;

namespace SelectPdf.Api
{

    /// <summary>
    /// Exception thrown by SelectPdf API Client.
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Constructor for ApiException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ApiException(string message) : this(message, null)
        { }
        /// <summary>
        /// Constructor for ApiException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception that triggered this exception.</param>
        public ApiException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
