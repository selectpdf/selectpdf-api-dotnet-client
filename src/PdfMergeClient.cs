using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SelectPdf.Api
{
    /// <summary>
    /// Pdf Merge with SelectPdf Online API.
    /// </summary>
    /// <example>
    /// Merge PDF documents in .NET with SelectPdf online REST API:
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
    ///             string testUrl = "https://selectpdf.com/demo/files/selectpdf.pdf";
    ///             string testPdf = "Input.pdf";
    ///             string localFile = "Result.pdf";
    ///             string apiKey = "Your API key here";
    /// 
    ///             Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);
    /// 
    ///             try
    ///             {
    ///                 PdfMergeClient client = new PdfMergeClient(apiKey);
    /// 
    ///                 // set parameters - see full list at https://selectpdf.com/pdf-merge-api/
    ///                 client
    ///                     // specify the pdf files that will be merged (order will be preserved in the final pdf)
    /// 
    ///                     .addFile(testPdf) // add PDF from local file
    ///                     .addUrlFile(testUrl) // add PDF From public url
    ///                     // .addFile(testPdf, "pdf_password") // add PDF (that requires a password) from local file
    ///                     // .addUrlFile(testUrl, "pdf_password") // add PDF (that requires a password) from public url
    ///                 ;
    /// 
    ///                 Console.WriteLine("Starting pdf merge ...");
    /// 
    ///                 // merge pdfs to local file
    ///                 client.saveToFile(localFile);
    /// 
    ///                 // merge pdfs to memory
    ///                 // byte[] pdf = client.save();
    /// 
    ///                 Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages());
    /// 
    ///                 // get API usage
    ///                 UsageClient usageClient = new UsageClient(apiKey);
    ///                 UsageInformation usage = usageClient.getUsage(false);
    ///                 Console.WriteLine("Conversions remained this month: {0}.", usage.Available);
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 Console.WriteLine("An error occurred: " + ex.Message);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language="vb">
    /// Imports SelectPdf.Api
    /// 
    /// Module Program
    ///     Sub Main(args As String())
    ///         Dim testUrl As String = "https://selectpdf.com/demo/files/selectpdf.pdf"
    ///         Dim testPdf As String = "Input.pdf"
    ///         Dim localFile As String = "Result.pdf"
    ///         Dim apiKey As String = "Your API key here"
    /// 
    ///         Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)
    /// 
    ///         Try
    ///             Dim client As PdfMergeClient = New PdfMergeClient(apiKey)
    /// 
    ///             ' set parameters - see full list at https://selectpdf.com/pdf-merge-api/
    /// 
    ///             ' specify the pdf files that will be merged (order will be preserved in the final pdf)
    ///             client.addFile(testPdf) ' add PDF from local file
    ///             client.addUrlFile(testUrl) ' add PDF From Public url
    ///             ' client.addFile(testPdf, "pdf_password") ' add PDF (that requires a password) from local file
    ///             ' client.addUrlFile(testUrl, "pdf_password") ' add PDF (that requires a password) from public url
    /// 
    ///             Console.WriteLine("Starting pdf merge ...")
    /// 
    ///             ' merge pdfs to local file
    ///             client.saveToFile(localFile)
    /// 
    ///             ' merge pdfs to memory
    ///             ' Dim pdf As Byte() = client.save()
    /// 
    ///             Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages())
    /// 
    ///             ' get API usage
    ///             Dim usageClient As UsageClient = New UsageClient(apiKey)
    ///             Dim usage As UsageInformation = usageClient.getUsage(False)
    ///             Console.WriteLine("Conversions remained this month: {0}.", usage.Available)
    /// 
    ///         Catch ex As Exception
    ///             Console.WriteLine("An error occurred: " &amp; ex.Message)
    ///         End Try
    ///     End Sub
    /// End Module
    /// </code>
    /// </example>
    public class PdfMergeClient: ApiClient
    {
        private int fileIdx = 0;

        /// <summary>
        /// Construct the Pdf Merge Client.
        /// </summary>
        /// <param name="apiKey">API key.</param>
        public PdfMergeClient(string apiKey)
        {
            apiEndpoint = "https://selectpdf.com/api2/pdfmerge/";
            parameters["key"] = apiKey;
        }


        /// <summary>
        /// Add local PDF document to the list of input files.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient addFile(string inputPdf)
        {
            fileIdx++;

            files["file_" + fileIdx] = inputPdf;
            parameters.Remove("url_" + fileIdx);
            parameters.Remove("password_" + fileIdx);

            return this;
        }

        /// <summary>
        /// Add local PDF document to the list of input files.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="userPassword">User password for the PDF document.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient addFile(string inputPdf, string userPassword)
        {
            fileIdx++;

            files["file_" + fileIdx] = inputPdf;
            parameters.Remove("url_" + fileIdx);
            parameters["password_" + fileIdx] = userPassword;

            return this;
        }

        /// <summary>
        /// Add remote PDF document to the list of input files.
        /// </summary>
        /// <param name="inputUrl">Url of a remote PDF file.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient addUrlFile(string inputUrl)
        {
            fileIdx++;

            parameters["url_" + fileIdx] = inputUrl;
            parameters.Remove("password_" + fileIdx);

            return this;
        }

        /// <summary>
        /// Add remote PDF document to the list of input files.
        /// </summary>
        /// <param name="inputUrl">Url of a remote PDF file.</param>
        /// <param name="userPassword">User password for the PDF document.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient addUrlFile(string inputUrl, string userPassword)
        {
            fileIdx++;

            parameters["url_" + fileIdx] = inputUrl;
            parameters["password_" + fileIdx] = userPassword;

            return this;
        }

        /// <summary>
        /// Merge all specified input pdfs and return the resulted PDF.
        /// </summary>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] save()
        {
            parameters["async"] = "False";
            parameters["files_no"] = fileIdx.ToString();

            byte[] result = PerformPostAsMultipartFormData(null);

            fileIdx = 0;
            files.Clear();

            return result;
        }

        /// <summary>
        /// Merge all specified input pdfs and writes the resulted PDF to a local file.
        /// </summary>
        /// <param name="filePath">Local output file including path if necessary.</param>
        public void saveToFile(string filePath)
        {
            parameters["async"] = "False";
            parameters["files_no"] = fileIdx.ToString();

            byte[] result = PerformPostAsMultipartFormData(null);
            File.WriteAllBytes(filePath, result);

            fileIdx = 0;
            files.Clear();
        }

        /// <summary>
        /// Merge all specified input pdfs and writes the resulted PDF to a specified stream.
        /// </summary>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void saveToStream(Stream stream)
        {
            parameters["async"] = "False";
            parameters["files_no"] = fileIdx.ToString();

            byte[] result = PerformPostAsMultipartFormData(null);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(result);

            fileIdx = 0;
            files.Clear();
        }

        /// <summary>
        /// Merge all specified input pdfs and return the resulted PDF. An asynchronous call is used.
        /// </summary>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] saveAsync()
        {
            parameters["files_no"] = fileIdx.ToString();

            string JobID = StartAsyncJobMultipartFormData();

            if (string.IsNullOrEmpty(JobID))
            {
                throw new ApiException("An error occurred launching the asynchronous call.");
            }

            int noPings = 0;

            do
            {
                noPings++;

                // sleep for a few seconds before next ping
                System.Threading.Thread.Sleep(AsyncCallsPingInterval * 1000);

                AsyncJobClient asyncJobClient = new AsyncJobClient(parameters["key"], JobID);
                asyncJobClient.setApiEndpoint(apiAsyncEndpoint);

                byte[] result = asyncJobClient.getResult();

                if (asyncJobClient.finished())
                {
                    numberOfPages = asyncJobClient.getNumberOfPages();
                    fileIdx = 0;
                    files.Clear();

                    return result;
                }

            } while (noPings <= AsyncCallsMaxPings);

            fileIdx = 0;
            files.Clear();
            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Merge all specified input pdfs and writes the resulted PDF to a local file. An asynchronous call is used.
        /// </summary>
        /// <param name="filePath">Local output file including path if necessary.</param>
        public void saveToFileAsync(string filePath)
        {
            byte[] result = saveAsync();
            File.WriteAllBytes(filePath, result);
        }

        /// <summary>
        /// Merge all specified input pdfs and writes the resulted PDF to a specified stream. An asynchronous call is used.
        /// </summary>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void saveToStreamAsync(Stream stream)
        {
            byte[] result = saveAsync();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(result);
        }

        /// <summary>
        /// Set the PDF document title.
        /// </summary>
        /// <param name="docTitle">Document title.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setDocTitle(string docTitle)
        {
            parameters["doc_title"] = docTitle;
            return this;
        }

        /// <summary>
        /// Set the subject of the PDF document.
        /// </summary>
        /// <param name="docSubject">Document subject.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setDocSubject(string docSubject)
        {
            parameters["doc_subject"] = docSubject;
            return this;
        }

        /// <summary>
        /// Set the PDF document keywords.
        /// </summary>
        /// <param name="docKeywords">Document keywords.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setDocKeywords(string docKeywords)
        {
            parameters["doc_keywords"] = docKeywords;
            return this;
        }

        /// <summary>
        /// Set the name of the PDF document author.
        /// </summary>
        /// <param name="docAuthor">Document author.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setDocAuthor(string docAuthor)
        {
            parameters["doc_author"] = docAuthor;
            return this;
        }

        /// <summary>
        /// Add the date and time when the PDF document was created to the PDF document information. The default value is False.
        /// </summary>
        /// <param name="docAddCreationDate">Add creation date to the document metadata or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setDocAddCreationDate(bool docAddCreationDate)
        {
            parameters["doc_add_creation_date"] = docAddCreationDate.ToString();
            return this;
        }

        /// <summary>
        /// Set the page layout to be used when the document is opened in a PDF viewer. The default value is PageLayout.OneColumn.
        /// </summary>
        /// <param name="pageLayout">Page layout.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerPageLayout(PageLayout pageLayout)
        {
            parameters["viewer_page_layout"] = ((int)pageLayout).ToString();
            return this;
        }

        /// <summary>
        /// Set the document page mode when the pdf document is opened in a PDF viewer. The default value is PageMode.UseNone.
        /// </summary>
        /// <param name="pageMode">Page mode.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerPageMode(PageMode pageMode)
        {
            parameters["viewer_page_mode"] = ((int)pageMode).ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to position the document's window in the center of the screen. The default value is False.
        /// </summary>
        /// <param name="viewerCenterWindow">Center window or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerCenterWindow(bool viewerCenterWindow)
        {
            parameters["viewer_center_window"] = viewerCenterWindow.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether the window's title bar should display the document title taken from document information. The default value is False.
        /// </summary>
        /// <param name="viewerDisplayDocTitle">Display title or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerDisplayDocTitle(bool viewerDisplayDocTitle)
        {
            parameters["viewer_display_doc_title"] = viewerDisplayDocTitle.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to resize the document's window to fit the size of the first displayed page. The default value is False.
        /// </summary>
        /// <param name="viewerFitWindow">Fit window or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerFitWindow(bool viewerFitWindow)
        {
            parameters["viewer_fit_window"] = viewerFitWindow.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to hide the pdf viewer application's menu bar when the document is active. The default value is False.
        /// </summary>
        /// <param name="viewerHideMenuBar">Hide menu bar or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerHideMenuBar(bool viewerHideMenuBar)
        {
            parameters["viewer_hide_menu_bar"] = viewerHideMenuBar.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to hide the pdf viewer application's tool bars when the document is active. The default value is False.
        /// </summary>
        /// <param name="viewerHideToolbar">Hide tool bars or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerHideToolbar(bool viewerHideToolbar)
        {
            parameters["viewer_hide_toolbar"] = viewerHideToolbar.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to hide user interface elements in the document's window (such as scroll bars and navigation controls), leaving only the document's contents displayed. 
        /// The default value is False.
        /// </summary>
        /// <param name="viewerHideWindowUI">Hide window UI or not.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setViewerHideWindowUI(bool viewerHideWindowUI)
        {
            parameters["viewer_hide_window_ui"] = viewerHideWindowUI.ToString();
            return this;
        }

        /// <summary>
        /// Set PDF user password.
        /// </summary>
        /// <param name="userPassword">PDF user password.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setUserPassword(string userPassword)
        {
            parameters["user_password"] = userPassword;
            return this;
        }

        /// <summary>
        /// Set PDF owner password.
        /// </summary>
        /// <param name="ownerPassword">PDF owner password.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setOwnerPassword(string ownerPassword)
        {
            parameters["owner_password"] = ownerPassword;
            return this;
        }

        /// <summary>
        /// Set the maximum amount of time (in seconds) for this job. 
        /// </summary>
        /// <remarks>
        /// The default value is 30 seconds. Use a larger value (up to 120 seconds allowed) for large documents.
        /// </remarks>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setTimeout(int timeout)
        {
            parameters["timeout"] = timeout.ToString();
            return this;
        }

        /// <summary>
        /// Set a custom parameter. Do not use this method unless advised by SelectPdf.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfMergeClient setCustomParameter(string parameterName, string parameterValue)
        {
            parameters[parameterName] = parameterValue;
            return this;
        }

        /// <summary>
        /// Get the number of pages processed from the PDF document.
        /// </summary>
        /// <returns>Number of pages processed from the PDF document.</returns>
        public int getNumberOfPages()
        {
            return numberOfPages;
        }

    }
}
