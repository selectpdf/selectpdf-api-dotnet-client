using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SelectPdf.Api
{
    /// <summary>
    /// Create ZUGFeRD / Factur-X hybrid electronic invoices with SelectPdf Online API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A hybrid electronic invoice is one PDF/A-3 file carrying both halves of the
    /// invoice: the page a human reads, and the XML a recipient's accounting system
    /// reads. This client converts a URL or an HTML string into the visible invoice
    /// and embeds the XML into it as an associated file, with the metadata invoice
    /// software looks for.
    /// </para>
    /// <para>
    /// It derives from <see cref="HtmlToPdfClient"/>, so every conversion setting -
    /// page size, margins, headers, footers, rendering engine - applies here too.
    /// Use the createFrom* methods rather than the inherited convert* methods: the
    /// invoice endpoint takes a multipart request, because the XML is uploaded as a
    /// file part.
    /// </para>
    /// <para>
    /// The carrier document must be PDF/A-3. The default is
    /// <see cref="PdfStandard.PdfA3A"/>, the accessible level, which the standards
    /// recommend because it makes the visible invoice readable by assistive
    /// technology as well as archivable. Because PdfA3A is a tagged standard, a
    /// request that does not set a rendering engine is promoted to Chromium by the
    /// API, which reports the engine used in the X-SelectPdf-Engine response header.
    /// </para>
    /// <para>
    /// There is no way to attach an invoice XML to an existing PDF you already have:
    /// the XML can only be embedded into a document created as PDF/A-3.
    /// </para>
    /// </remarks>
    /// <example>
    /// Create a hybrid electronic invoice in .NET with SelectPdf online REST API:
    /// <code language="cs">
    /// using System;
    /// using SelectPdf.Api;
    ///
    /// namespace SelectPdf.Api.Tests
    /// {
    ///     class Program
    ///     {
    ///         static void Main(string[] args)
    ///         {
    ///             string apiKey = "Your API key here";
    ///             string invoiceHtml = "&lt;html&gt;&lt;body&gt;&lt;h1&gt;Invoice INV-2026-001&lt;/h1&gt;&lt;/body&gt;&lt;/html&gt;";
    ///             string invoiceXml = "factur-x.xml";
    ///             string localFile = "Invoice.pdf";
    ///
    ///             try
    ///             {
    ///                 InvoiceClient client = new InvoiceClient(apiKey);
    ///
    ///                 client
    ///                     .setInvoiceXmlFile(invoiceXml)
    ///                     .setZugferdProfile(ZugferdProfile.En16931)
    ///                     .setDocTitle("Invoice INV-2026-001");
    ///
    ///                 client.createFromHtmlStringToFile(invoiceHtml, localFile);
    ///
    ///                 Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages());
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 Console.WriteLine("An error occurred: " + ex.Message);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class InvoiceClient : HtmlToPdfClient
    {
        /// <summary>
        /// The production endpoint for hybrid electronic invoices.
        /// </summary>
        public const string InvoiceEndpoint = "https://selectpdf.com/api2/invoice/";

        /// <summary>
        /// Construct the Invoice Client.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="HtmlToPdfClient"/>, this client has no demo mode - the
        /// keyless demo endpoint does not produce electronic invoices - so an API
        /// key is required.
        /// </remarks>
        /// <param name="apiKey">API key.</param>
        /// <exception cref="ApiException">Thrown when no API key is supplied.</exception>
        public InvoiceClient(string apiKey)
            : base(apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Trim().ToLower() == "demo")
            {
                throw new ApiException(
                    "An API key is required to create electronic invoices. The keyless demo endpoint does not support them.");
            }

            apiEndpoint = InvoiceEndpoint;

            // The carrier has to be PDF/A-3; default to the accessible level, which
            // the standards recommend. Overridable with setPdfStandard.
            parameters["pdf_standard"] = PdfStandard.PdfA3A.ToString();
        }

        /// <summary>
        /// Set the invoice XML from a local file.
        /// </summary>
        /// <remarks>
        /// Only the content of the file is used - the name recorded inside the PDF
        /// is the one the standard prescribes ("factur-x.xml", or "xrechnung.xml"
        /// for the XRECHNUNG profile), because recipients look it up by name.
        /// </remarks>
        /// <param name="invoiceXmlFile">Path to the local invoice XML file.</param>
        /// <returns>Reference to the current object.</returns>
        public InvoiceClient setInvoiceXmlFile(string invoiceXmlFile)
        {
            binaryData.Remove("zugferd_xml");
            files["zugferd_xml"] = invoiceXmlFile;
            return this;
        }

        /// <summary>
        /// Set the invoice XML from memory.
        /// </summary>
        /// <param name="invoiceXml">The invoice XML bytes.</param>
        /// <returns>Reference to the current object.</returns>
        public InvoiceClient setInvoiceXml(byte[] invoiceXml)
        {
            files.Remove("zugferd_xml");
            binaryData["zugferd_xml"] = invoiceXml;
            return this;
        }

        /// <summary>
        /// Set the invoice XML from a string.
        /// </summary>
        /// <param name="invoiceXml">The invoice XML.</param>
        /// <returns>Reference to the current object.</returns>
        public InvoiceClient setInvoiceXml(string invoiceXml)
        {
            return setInvoiceXml(new UTF8Encoding(false).GetBytes(invoiceXml));
        }

        /// <summary>
        /// Set the data profile of the invoice XML. Required.
        /// </summary>
        /// <param name="profile">The invoice data profile.</param>
        /// <returns>Reference to the current object.</returns>
        public InvoiceClient setZugferdProfile(ZugferdProfile profile)
        {
            parameters["zugferd_profile"] = profile.ToString();
            return this;
        }

        /// <summary>
        /// Set how the embedded XML relates to the visible invoice page.
        /// </summary>
        /// <remarks>
        /// Optional. When not set, the API derives it from the profile:
        /// <see cref="ZugferdRelationship.Alternative"/> for
        /// <see cref="ZugferdProfile.Minimum"/> and
        /// <see cref="ZugferdProfile.Basic_WL"/>, which do not carry a complete
        /// invoice, and <see cref="ZugferdRelationship.Data"/> for the rest.
        /// Those two profiles combined with Data are rejected by the API.
        /// </remarks>
        /// <param name="relationship">The relationship between XML and page.</param>
        /// <returns>Reference to the current object.</returns>
        public InvoiceClient setZugferdRelationship(ZugferdRelationship relationship)
        {
            parameters["zugferd_relationship"] = relationship.ToString();
            return this;
        }

        /// <summary>
        /// Set the metadata schema identifying the invoice. Defaults to
        /// <see cref="ZugferdSchema.FacturX10"/>.
        /// </summary>
        /// <param name="schema">The invoice metadata schema.</param>
        /// <returns>Reference to the current object.</returns>
        public InvoiceClient setZugferdSchema(ZugferdSchema schema)
        {
            parameters["zugferd_schema"] = schema.ToString();
            return this;
        }

        // --- synchronous -----------------------------------------------------

        /// <summary>
        /// Create a hybrid electronic invoice from the invoice page at the specified url.
        /// </summary>
        /// <param name="url">Url of the invoice page.</param>
        /// <returns>Byte array containing the resulted hybrid invoice PDF.</returns>
        public byte[] createFromUrl(string url)
        {
            PrepareUrl(url);
            parameters["async"] = "False";
            return PerformPostAsMultipartFormData(null);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from the invoice page at the specified
        /// url and write it to a stream.
        /// </summary>
        /// <param name="url">Url of the invoice page.</param>
        /// <param name="stream">The output stream.</param>
        public void createFromUrlToStream(string url, Stream stream)
        {
            PrepareUrl(url);
            parameters["async"] = "False";
            PerformPostAsMultipartFormData(stream);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from the invoice page at the specified
        /// url and write it to a local file.
        /// </summary>
        /// <param name="url">Url of the invoice page.</param>
        /// <param name="filePath">Path of the output file.</param>
        public void createFromUrlToFile(string url, string filePath)
        {
            WriteToFile(filePath, delegate (Stream s) { createFromUrlToStream(url, s); });
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <returns>Byte array containing the resulted hybrid invoice PDF.</returns>
        public byte[] createFromHtmlString(string htmlString)
        {
            return createFromHtmlString(htmlString, null);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths in the HTML.</param>
        /// <returns>Byte array containing the resulted hybrid invoice PDF.</returns>
        public byte[] createFromHtmlString(string htmlString, string baseUrl)
        {
            PrepareHtml(htmlString, baseUrl);
            parameters["async"] = "False";
            return PerformPostAsMultipartFormData(null);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string and write it to a stream.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="stream">The output stream.</param>
        public void createFromHtmlStringToStream(string htmlString, Stream stream)
        {
            createFromHtmlStringToStream(htmlString, null, stream);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string and write it to a stream.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths in the HTML.</param>
        /// <param name="stream">The output stream.</param>
        public void createFromHtmlStringToStream(string htmlString, string baseUrl, Stream stream)
        {
            PrepareHtml(htmlString, baseUrl);
            parameters["async"] = "False";
            PerformPostAsMultipartFormData(stream);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string and write it to a local file.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="filePath">Path of the output file.</param>
        public void createFromHtmlStringToFile(string htmlString, string filePath)
        {
            createFromHtmlStringToFile(htmlString, null, filePath);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string and write it to a local file.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths in the HTML.</param>
        /// <param name="filePath">Path of the output file.</param>
        public void createFromHtmlStringToFile(string htmlString, string baseUrl, string filePath)
        {
            WriteToFile(filePath, delegate (Stream s) { createFromHtmlStringToStream(htmlString, baseUrl, s); });
        }

        // --- asynchronous ----------------------------------------------------

        /// <summary>
        /// Create a hybrid electronic invoice from the invoice page at the specified
        /// url, asynchronously.
        /// </summary>
        /// <remarks>
        /// Recommended for long invoice pages or callers that cannot hold an HTTP
        /// connection open for the whole conversion.
        /// </remarks>
        /// <param name="url">Url of the invoice page.</param>
        /// <returns>Byte array containing the resulted hybrid invoice PDF.</returns>
        public byte[] createFromUrlAsync(string url)
        {
            PrepareUrl(url);
            return RunAsyncJob();
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string, asynchronously.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <returns>Byte array containing the resulted hybrid invoice PDF.</returns>
        public byte[] createFromHtmlStringAsync(string htmlString)
        {
            return createFromHtmlStringAsync(htmlString, null);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string, asynchronously.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths in the HTML.</param>
        /// <returns>Byte array containing the resulted hybrid invoice PDF.</returns>
        public byte[] createFromHtmlStringAsync(string htmlString, string baseUrl)
        {
            PrepareHtml(htmlString, baseUrl);
            return RunAsyncJob();
        }

        /// <summary>
        /// Create a hybrid electronic invoice from the invoice page at the specified
        /// url, asynchronously, and write it to a local file.
        /// </summary>
        /// <param name="url">Url of the invoice page.</param>
        /// <param name="filePath">Path of the output file.</param>
        public void createFromUrlToFileAsync(string url, string filePath)
        {
            byte[] result = createFromUrlAsync(url);
            WriteBytesToFile(filePath, result);
        }

        /// <summary>
        /// Create a hybrid electronic invoice from a raw HTML string, asynchronously,
        /// and write it to a local file.
        /// </summary>
        /// <param name="htmlString">The invoice HTML.</param>
        /// <param name="filePath">Path of the output file.</param>
        public void createFromHtmlStringToFileAsync(string htmlString, string filePath)
        {
            byte[] result = createFromHtmlStringAsync(htmlString, null);
            WriteBytesToFile(filePath, result);
        }

        // --- internals -------------------------------------------------------

        private void PrepareUrl(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the converted webpage are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }

            RequireInvoiceXml();
            parameters["url"] = url;
            parameters["html"] = string.Empty;
            parameters["base_url"] = string.Empty;
        }

        private void PrepareHtml(string htmlString, string baseUrl)
        {
            RequireInvoiceXml();
            parameters["url"] = string.Empty;
            parameters["html"] = htmlString;
            parameters["base_url"] = baseUrl ?? string.Empty;
        }

        // Fail here rather than spending a round trip on a request the API will
        // reject with the same message.
        private void RequireInvoiceXml()
        {
            if (!files.ContainsKey("zugferd_xml") && !binaryData.ContainsKey("zugferd_xml"))
            {
                throw new ApiException(
                    "The invoice XML was not specified. Call setInvoiceXmlFile or setInvoiceXml before creating the invoice.");
            }
            if (!parameters.ContainsKey("zugferd_profile") || string.IsNullOrEmpty(parameters["zugferd_profile"]))
            {
                throw new ApiException(
                    "The invoice profile was not specified. Call setZugferdProfile before creating the invoice.");
            }
        }

        private byte[] RunAsyncJob()
        {
            parameters["async"] = "True";

            string JobID = StartAsyncJobMultipartFormData();

            if (string.IsNullOrEmpty(JobID))
            {
                throw new ApiException("An error occurred launching the asynchronous call.");
            }

            int noPings = 0;

            while (noPings < AsyncCallsMaxPings)
            {
                noPings++;

#if NET_STANDARD_20
                System.Threading.Tasks.Task.Delay(AsyncCallsPingInterval * 1000).Wait();
#else
                System.Threading.Thread.Sleep(AsyncCallsPingInterval * 1000);
#endif

                AsyncJobClient asyncJobClient = new AsyncJobClient(parameters["key"], JobID);
                asyncJobClient.setApiEndpoint(apiAsyncEndpoint);

                byte[] result = asyncJobClient.getResult();

                if (asyncJobClient.finished())
                {
                    numberOfPages = asyncJobClient.getNumberOfPages();
                    creditsTotal = asyncJobClient.CreditsTotal;
                    creditsRemaining = asyncJobClient.CreditsRemaining;
                    return result;
                }
            }

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        private delegate void WriteToStream(Stream stream);

        private static void WriteToFile(string filePath, WriteToStream write)
        {
            using (FileStream outputFile = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                write(outputFile);
                outputFile.Close();
            }
        }

        private static void WriteBytesToFile(string filePath, byte[] contents)
        {
            using (FileStream outputFile = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                outputFile.Write(contents, 0, contents.Length);
                outputFile.Close();
            }
        }
    }
}
