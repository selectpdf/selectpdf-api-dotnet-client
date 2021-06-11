using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SelectPdf.Api
{
    /// <summary>
    /// Html To Pdf Conversion with SelectPdf Online API.
    /// </summary>
    public class HtmlToPdfClient : ApiClient
    {
        /// <summary>
        /// Construct the Html To Pdf Client.
        /// </summary>
        /// <param name="apiKey">API key.</param>
        public HtmlToPdfClient(string apiKey)
        {
            apiEndpoint = "https://selectpdf.com/api2/convert/";
            parameters["key"] = apiKey;
        }

        /// <summary>
        /// Convert the specified url to PDF.
        /// </summary>
        /// <remarks>SelectPdf online API can convert http:// and https:// publicly available urls.</remarks>
        /// <param name="url">Address of the web page being converted.</param>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] convertUrl(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the converted webpage are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }
            parameters["url"] = url;
            parameters["html"] = string.Empty;
            parameters["base_url"] = string.Empty;
            parameters["async"] = "False";

            return PerformPost(null);
        }

        /// <summary>
        /// Convert the specified url to PDF and writes the resulted PDF to an output stream.
        /// </summary>
        /// <remarks>SelectPdf online API can convert http:// and https:// publicly available urls.</remarks>
        /// <param name="url">Address of the web page being converted.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void convertUrlToStream(string url, Stream stream)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the converted webpage are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }
            parameters["url"] = url;
            parameters["html"] = string.Empty;
            parameters["base_url"] = string.Empty;
            parameters["async"] = "False";

            PerformPost(stream);
        }

        /// <summary>
        /// Convert the specified url to PDF and writes the resulted PDF to a local file.
        /// </summary>
        /// <remarks>SelectPdf online API can convert http:// and https:// publicly available urls.</remarks>
        /// <param name="url">Address of the web page being converted.</param>
        /// <param name="filePath">Local file including path if necessary.</param>
        public void convertUrlToFile(string url, string filePath)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the converted webpage are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }
            parameters["url"] = url;
            parameters["html"] = string.Empty;
            parameters["base_url"] = string.Empty;
            parameters["async"] = "False";

            FileStream outputFile = new FileStream(filePath, FileMode.Create);

            try
            {
                PerformPost(outputFile);
                outputFile.Close();
            }
            catch 
            {
                outputFile.Close();
                File.Delete(filePath);
                throw;
            }

        }

        /// <summary>
        /// Convert the specified url to PDF using an asynchronous call.
        /// </summary>
        /// <remarks>SelectPdf online API can convert http:// and https:// publicly available urls.</remarks>
        /// <param name="url">Address of the web page being converted.</param>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] convertUrlAsync(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the converted webpage are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }
            parameters["url"] = url;
            parameters["html"] = string.Empty;
            parameters["base_url"] = string.Empty;

            string JobID = StartAsyncJob();

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

                if (result != null)
                {
                    numberOfPages = asyncJobClient.getNumberOfPage();
                    webElements = asyncJobClient.getWebElements();

                    return result;
                }

            } while (noPings <= AsyncCallsMaxPings);

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Convert the specified url to PDF with an asynchronous call and writes the resulted PDF to an output stream.
        /// </summary>
        /// <remarks>SelectPdf online API can convert http:// and https:// publicly available urls.</remarks>
        /// <param name="url">Address of the web page being converted.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void convertUrlToStreamAsync(string url, Stream stream)
        {
            byte[] result = convertUrlAsync(url);

            BinaryCopyStream(new MemoryStream(result), stream);
        }

        /// <summary>
        /// Convert the specified url to PDF with an asynchronous call and writes the resulted PDF to a local file.
        /// </summary>
        /// <remarks>SelectPdf online API can convert http:// and https:// publicly available urls.</remarks>
        /// <param name="url">Address of the web page being converted.</param>
        /// <param name="filePath">Local file including path if necessary.</param>
        public void convertUrlToFileAsync(string url, string filePath)
        {
            byte[] result = convertUrlAsync(url);

            using (FileStream outputFile = new FileStream(filePath, FileMode.Create))
            {
                BinaryCopyStream(new MemoryStream(result), outputFile);
                outputFile.Close();
            }
        }

        /// <summary>
        /// Convert the specified HTML string to PDF.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] convertHtmlString(string htmlString)
        {
            return convertHtmlString(htmlString, string.Empty);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF. Use a base url to resolve relative paths to resources.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths to resources (css, images, javascript, etc). Must be a http:// or https:// publicly available url.</param>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] convertHtmlString(string htmlString, string baseUrl)
        {
            parameters["url"] = string.Empty;
            parameters["html"] = htmlString;

            if (!string.IsNullOrEmpty(baseUrl))
            {
                parameters["base_url"] = baseUrl;
            }

            return PerformPost(null);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF and writes the resulted PDF to an output stream.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void convertHtmlStringToStream(string htmlString, Stream stream)
        {
            convertHtmlStringToStream(htmlString, string.Empty, stream);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF and writes the resulted PDF to an output stream. Use a base url to resolve relative paths to resources.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths to resources (css, images, javascript, etc). Must be a http:// or https:// publicly available url.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void convertHtmlStringToStream(string htmlString, string baseUrl, Stream stream)
        {
            parameters["url"] = string.Empty;
            parameters["html"] = htmlString;

            if (!string.IsNullOrEmpty(baseUrl))
            {
                parameters["base_url"] = baseUrl;
            }

            PerformPost(stream);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF and writes the resulted PDF to a local file.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="filePath">Local file including path if necessary.</param>
        public void convertHtmlStringToFile(string htmlString, string filePath)
        {
            convertHtmlStringToFile(htmlString, string.Empty, filePath);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF and writes the resulted PDF to a local file. Use a base url to resolve relative paths to resources.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths to resources (css, images, javascript, etc). Must be a http:// or https:// publicly available url.</param>
        /// <param name="filePath">Local file including path if necessary.</param>
        public void convertHtmlStringToFile(string htmlString, string baseUrl, string filePath)
        {
            parameters["url"] = string.Empty;
            parameters["html"] = htmlString;

            if (!string.IsNullOrEmpty(baseUrl))
            {
                parameters["base_url"] = baseUrl;
            }

            FileStream outputFile = new FileStream(filePath, FileMode.Create);

            try
            {
                PerformPost(outputFile);
                outputFile.Close();
            }
            catch
            {
                outputFile.Close();
                File.Delete(filePath);
                throw;
            }
        }

        /// <summary>
        /// Convert the specified HTML string to PDF with an asynchronous call.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] convertHtmlStringAsync(string htmlString)
        {
            return convertHtmlStringAsync(htmlString, string.Empty);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF with an asynchronous call. Use a base url to resolve relative paths to resources.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths to resources (css, images, javascript, etc). Must be a http:// or https:// publicly available url.</param>
        /// <returns>Byte array containing the resulted PDF.</returns>
        public byte[] convertHtmlStringAsync(string htmlString, string baseUrl)
        {
            parameters["url"] = string.Empty;
            parameters["html"] = htmlString;

            if (!string.IsNullOrEmpty(baseUrl))
            {
                parameters["base_url"] = baseUrl;
            }

            string JobID = StartAsyncJob();

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

                if (result != null)
                {
                    numberOfPages = asyncJobClient.getNumberOfPage();
                    webElements = asyncJobClient.getWebElements();

                    return result;
                }

            } while (noPings <= AsyncCallsMaxPings);

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Convert the specified HTML string to PDF with an asynchronous call and writes the resulted PDF to an output stream.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void convertHtmlStringToStreamAsync(string htmlString, Stream stream)
        {
            convertHtmlStringToStreamAsync(htmlString, string.Empty, stream);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF with an asynchronous call and writes the resulted PDF to an output stream. Use a base url to resolve relative paths to resources.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths to resources (css, images, javascript, etc). Must be a http:// or https:// publicly available url.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void convertHtmlStringToStreamAsync(string htmlString, string baseUrl, Stream stream)
        {
            byte[] result = convertHtmlStringAsync(htmlString, baseUrl);

            BinaryCopyStream(new MemoryStream(result), stream);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF with an asynchronous call and writes the resulted PDF to a local file.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="filePath">Local file including path if necessary.</param>
        public void convertHtmlStringToFileAsync(string htmlString, string filePath)
        {
            convertHtmlStringToFileAsync(htmlString, string.Empty, filePath);
        }

        /// <summary>
        /// Convert the specified HTML string to PDF with an asynchronous call and writes the resulted PDF to a local file. Use a base url to resolve relative paths to resources.
        /// </summary>
        /// <param name="htmlString">HTML string with the content being converted.</param>
        /// <param name="baseUrl">Base url used to resolve relative paths to resources (css, images, javascript, etc). Must be a http:// or https:// publicly available url.</param>
        /// <param name="filePath">Local file including path if necessary.</param>
        public void convertHtmlStringToFileAsync(string htmlString, string baseUrl, string filePath)
        {
            byte[] result = convertHtmlStringAsync(htmlString, baseUrl);

            using (FileStream outputFile = new FileStream(filePath, FileMode.Create))
            {
                BinaryCopyStream(new MemoryStream(result), outputFile);
                outputFile.Close();
            }
        }

        /// <summary>
        /// Set PDF page size. Default value is A4.
        /// </summary>
        /// <remarks>If page size is set to Custom, use setPageWidth and setPageHeight methods to set the custom width/height of the PDF pages.</remarks>
        /// <param name="pageSize">PDF page size.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageSize(PageSize pageSize)
        {
            parameters["page_size"] = pageSize.ToString();
            return this;
        }

        /// <summary>
        /// Set PDF page width in points. Default value is 595pt (A4 page width in points). 1pt = 1/72 inch.
        /// </summary>
        /// <remarks>This is taken into account only if page size is set to Custom using setPageSize method.</remarks>
        /// <param name="pageWidth">Page width in points.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageWidth(int pageWidth)
        {
            parameters["page_width"] = pageWidth.ToString();
            return this;
        }

        /// <summary>
        /// Set PDF page height in points. Default value is 842pt (A4 page height in points). 1pt = 1/72 inch.
        /// </summary>
        /// <remarks>This is taken into account only if page size is set to Custom using setPageSize method.</remarks>
        /// <param name="pageHeight">Page height in points.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageHeight(int pageHeight)
        {
            parameters["page_height"] = pageHeight.ToString();
            return this;
        }

        /// <summary>
        /// Set PDF page orientation. Default value is Portrait.
        /// </summary>
        /// <param name="pageOrientation">PDF page orientation.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageOrientation(PageOrientation pageOrientation)
        {
            parameters["page_orientation"] = pageOrientation.ToString();
            return this;
        }

        /// <summary>
        /// Set top margin of the PDF pages. Default value is 5pt.
        /// </summary>
        /// <param name="marginTop">Margin value in points. 1pt = 1/72 inch.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMarginTop(int marginTop)
        {
            parameters["margin_top"] = marginTop.ToString();
            return this;
        }

        /// <summary>
        /// Set right margin of the PDF pages. Default value is 5pt.
        /// </summary>
        /// <param name="marginRight">Margin value in points. 1pt = 1/72 inch.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMarginRight(int marginRight)
        {
            parameters["margin_right"] = marginRight.ToString();
            return this;
        }

        /// <summary>
        /// Set bottom margin of the PDF pages. Default value is 5pt.
        /// </summary>
        /// <param name="marginBottom">Margin value in points. 1pt = 1/72 inch.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMarginBottom(int marginBottom)
        {
            parameters["margin_bottom"] = marginBottom.ToString();
            return this;
        }

        /// <summary>
        /// Set left margin of the PDF pages. Default value is 5pt.
        /// </summary>
        /// <param name="marginLeft">Margin value in points. 1pt = 1/72 inch.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMarginLeft(int marginLeft)
        {
            parameters["margin_left"] = marginLeft.ToString();
            return this;
        }

        /// <summary>
        /// Set all margins of the PDF pages to the same value. Default value is 5pt.
        /// </summary>
        /// <param name="margin">Margin value in points. 1pt = 1/72 inch.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMargins(int margin)
        {
            return setMarginTop(margin).setMarginRight(margin).setMarginBottom(margin).setMarginLeft(margin);
        }

        /// <summary>
        /// Specify the name of the pdf document that will be created. The default value is Document.pdf.
        /// </summary>
        /// <param name="pdfName">Name of the generated PDF document.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPdfName(string pdfName)
        {
            parameters["pdf_name"] = pdfName;
            return this;
        }

        /// <summary>
        /// Set the rendering engine used for the HTML to PDF conversion. Default value is WebKit.
        /// </summary>
        /// <param name="renderingEngine">HTML rendering engine.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setRenderingEngine(RenderingEngine renderingEngine)
        {
            parameters["engine"] = renderingEngine.ToString();
            return this;
        }

        /// <summary>
        /// Set PDF user password.
        /// </summary>
        /// <param name="userPassword">PDF user password.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setUserPassword(string userPassword)
        {
            parameters["user_password"] = userPassword;
            return this;
        }

        /// <summary>
        /// Set PDF owner password.
        /// </summary>
        /// <param name="ownerPassword">PDF owner password.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setOwnerPassword(string ownerPassword)
        {
            parameters["owner_password"] = ownerPassword;
            return this;
        }

        /// <summary>
        /// Set the width used by the converter's internal browser window in pixels. The default value is 1024px.
        /// </summary>
        /// <param name="webPageWidth">Browser window width in pixels.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setWebPageWidth(int webPageWidth)
        {
            parameters["web_page_width"] = webPageWidth.ToString();
            return this;
        }

        /// <summary>
        /// Set the height used by the converter's internal browser window in pixels. The default value is 0px and it means that the page height is automatically calculated by the converter.
        /// </summary>
        /// <param name="webPageHeight">Browser window height in pixels. Set it to 0px to automatically calculate page height.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setWebPageHeight(int webPageHeight)
        {
            parameters["web_page_height"] = webPageHeight.ToString();
            return this;
        }

        /// <summary>
        /// Introduce a delay (in seconds) before the actual conversion to allow the web page to fully load. 
        /// </summary>
        /// <remarks>
        /// This method is an alias for setConversionDelay. The default value is 1 second. Use a larger value if the web page has content that takes time to render when it is displayed in the browser.
        /// </remarks>
        /// <param name="minLoadTime">Delay in seconds.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMinLoadTime(int minLoadTime)
        {
            parameters["min_load_time"] = minLoadTime.ToString();
            return this;
        }

        /// <summary>
        /// Introduce a delay (in seconds) before the actual conversion to allow the web page to fully load. 
        /// </summary>
        /// <remarks>
        /// This method is an alias for setMinLoadTime. The default value is 1 second. 
        /// Use a larger value if the web page has content that takes time to render when it is displayed in the browser.
        /// </remarks>
        /// <param name="delay">Delay in seconds.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setConversionDelay(int delay)
        {
            return setMinLoadTime(delay);
        }

        /// <summary>
        /// Set the maximum amount of time (in seconds) that the converter will wait for the page to load. 
        /// </summary>
        /// <remarks>
        /// This method is an alias for setNavigationTimeout. A timeout error is displayed when this time elapses. 
        /// The default value is 30 seconds. Use a larger value (up to 120 seconds allowed) for pages that take a long time to load.
        /// </remarks>
        /// <param name="maxLoadTime">Timeout in seconds.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setMaxLoadTime(int maxLoadTime)
        {
            parameters["max_load_time"] = maxLoadTime.ToString();
            return this;
        }

        /// <summary>
        /// Set the maximum amount of time (in seconds) that the converter will wait for the page to load. 
        /// </summary>
        /// <remarks>
        /// This method is an alias for setMaxLoadTime. A timeout error is displayed when this time elapses. 
        /// The default value is 30 seconds. Use a larger value (up to 120 seconds allowed) for pages that take a long time to load.
        /// </remarks>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setNavigationTimeout(int timeout)
        {
            return setMaxLoadTime(timeout);
        }

        /// <summary>
        /// Set the protocol used for secure (HTTPS) connections.
        /// </summary>
        /// <remarks>Set this only if you have an older server that only works with older SSL connections.</remarks>
        /// <param name="secureProtocol">Secure protocol.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setSecureProtocol(SecureProtocol secureProtocol)
        {
            parameters["protocol"] = ((int)secureProtocol).ToString();
            return this;
        }

        /// <summary>
        /// Specify if the CSS Print media type is used instead of the Screen media type. The default value is False.
        /// </summary>
        /// <param name="useCssPrint">Use CSS Print media or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setUseCssPrint(bool useCssPrint)
        {
            parameters["use_css_print"] = useCssPrint.ToString();
            return this;
        }

        /// <summary>
        /// Specify the background color of the PDF page in RGB html format. The default is #FFFFFF.
        /// </summary>
        /// <param name="backgroundColor">Background color in #RRGGBB format.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setBackgroundColor(string backgroundColor)
        {
            if (!Regex.Match(backgroundColor, "^#?[0-9a-fA-F]{6}$").Success)
                throw new ApiException("Color value must be in #RRGGBB format.");

            parameters["background_color"] = backgroundColor;
            return this;
        }

        /// <summary>
        /// Set a flag indicating if the web page background is rendered in PDF. The default value is True.
        /// </summary>
        /// <param name="drawHtmlBackground">Draw the HTML background or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDrawHtmlBackground(bool drawHtmlBackground)
        {
            parameters["draw_html_background"] = drawHtmlBackground.ToString();
            return this;
        }

        /// <summary>
        /// Do not run JavaScript in web pages. The default value is False and javascript is executed.
        /// </summary>
        /// <param name="disableJavascript">Disable javascript or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDisableJavascript(bool disableJavascript)
        {
            parameters["disable_javascript"] = disableJavascript.ToString();
            return this;
        }

        /// <summary>
        /// Do not create internal links in the PDF. The default value is False and internal links are created.
        /// </summary>
        /// <param name="disableInternalLinks">Disable internal links or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDisableInternalLinks(bool disableInternalLinks)
        {
            parameters["disable_internal_links"] = disableInternalLinks.ToString();
            return this;
        }

        /// <summary>
        /// Do not create external links in the PDF. The default value is False and external links are created.
        /// </summary>
        /// <param name="disableExternalLinks">Disable external links or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDisableExternalLinks(bool disableExternalLinks)
        {
            parameters["disable_external_links"] = disableExternalLinks.ToString();
            return this;
        }

        /// <summary>
        /// Try to render the PDF even in case of the web page loading timeout. The default value is False and an exception is raised in case of web page navigation timeout.
        /// </summary>
        /// <param name="renderOnTimeout">Render in case of timeout or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setRenderOnTimeout(bool renderOnTimeout)
        {
            parameters["render_on_timeout"] = renderOnTimeout.ToString();
            return this;
        }

        /// <summary>
        /// Avoid breaking images between PDF pages. The default value is False and images are split between pages if larger.
        /// </summary>
        /// <param name="keepImagesTogether">Try to keep images on same page or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setKeepImagesTogether(bool keepImagesTogether)
        {
            parameters["keep_images_together"] = keepImagesTogether.ToString();
            return this;
        }

        /// <summary>
        /// Set the PDF document title.
        /// </summary>
        /// <param name="docTitle">Document title.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDocTitle(string docTitle)
        {
            parameters["doc_title"] = docTitle;
            return this;
        }

        /// <summary>
        /// Set the subject of the PDF document.
        /// </summary>
        /// <param name="docSubject">Document subject.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDocSubject(string docSubject)
        {
            parameters["doc_subject"] = docSubject;
            return this;
        }

        /// <summary>
        /// Set the PDF document keywords.
        /// </summary>
        /// <param name="docKeywords">Document keywords.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDocKeywords(string docKeywords)
        {
            parameters["doc_keywords"] = docKeywords;
            return this;
        }

        /// <summary>
        /// Set the name of the PDF document author.
        /// </summary>
        /// <param name="docAuthor">Document author.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDocAuthor(string docAuthor)
        {
            parameters["doc_author"] = docAuthor;
            return this;
        }

        /// <summary>
        /// Add the date and time when the PDF document was created to the PDF document information. The default value is False.
        /// </summary>
        /// <param name="docAddCreationDate">Add creation date to the document metadata or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setDocAddCreationDate(bool docAddCreationDate)
        {
            parameters["doc_add_creation_date"] = docAddCreationDate.ToString();
            return this;
        }

        /// <summary>
        /// Set the page layout to be used when the document is opened in a PDF viewer. The default value is PageLayout.OneColumn.
        /// </summary>
        /// <param name="pageLayout">Page layout.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerPageLayout(PageLayout pageLayout)
        {
            parameters["viewer_page_layout"] = ((int)pageLayout).ToString();
            return this;
        }

        /// <summary>
        /// Set the document page mode when the pdf document is opened in a PDF viewer. The default value is PageMode.UseNone.
        /// </summary>
        /// <param name="pageMode">Page mode.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerPageMode(PageMode pageMode)
        {
            parameters["viewer_page_mode"] = ((int)pageMode).ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to position the document's window in the center of the screen. The default value is False.
        /// </summary>
        /// <param name="viewerCenterWindow">Center window or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerCenterWindow(bool viewerCenterWindow)
        {
            parameters["viewer_center_window"] = viewerCenterWindow.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether the window's title bar should display the document title taken from document information. The default value is False.
        /// </summary>
        /// <param name="viewerDisplayDocTitle">Display title or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerDisplayDocTitle(bool viewerDisplayDocTitle)
        {
            parameters["viewer_display_doc_title"] = viewerDisplayDocTitle.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to resize the document's window to fit the size of the first displayed page. The default value is False.
        /// </summary>
        /// <param name="viewerFitWindow">Fit window or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerFitWindow(bool viewerFitWindow)
        {
            parameters["viewer_fit_window"] = viewerFitWindow.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to hide the pdf viewer application's menu bar when the document is active. The default value is False.
        /// </summary>
        /// <param name="viewerHideMenuBar">Hide menu bar or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerHideMenuBar(bool viewerHideMenuBar)
        {
            parameters["viewer_hide_menu_bar"] = viewerHideMenuBar.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag specifying whether to hide the pdf viewer application's tool bars when the document is active. The default value is False.
        /// </summary>
        /// <param name="viewerHideToolbar">Hide tool bars or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setViewerHideToolbar(bool viewerHideToolbar)
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
        public HtmlToPdfClient setViewerHideWindowUI(bool viewerHideWindowUI)
        {
            parameters["viewer_hide_window_ui"] = viewerHideWindowUI.ToString();
            return this;
        }

        /// <summary>
        /// Control if a custom header is displayed in the generated PDF document. The default value is False.
        /// </summary>
        /// <param name="showHeader">Show header or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setShowHeader(bool showHeader)
        {
            parameters["show_header"] = showHeader.ToString();
            return this;
        }

        /// <summary>
        /// The height of the pdf document header. This height is specified in points. 1 point is 1/72 inch. The default value is 50.
        /// </summary>
        /// <param name="height">Header height.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderHeight(int height)
        {
            parameters["header_height"] = height.ToString();
            return this;
        }

        /// <summary>
        /// Set the url of the web page that is converted and rendered in the PDF document header.
        /// </summary>
        /// <param name="url">The url of the web page that is converted and rendered in the pdf document header.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderUrl(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the url are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }

            parameters["header_url"] = url;
            return this;
        }

        /// <summary>
        /// Set the raw html that is converted and rendered in the pdf document header.
        /// </summary>
        /// <param name="html">The raw html that is converted and rendered in the pdf document header.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderHtml(string html)
        {
            parameters["header_html"] = html;
            return this;
        }

        /// <summary>
        /// Set an optional base url parameter can be used together with the header HTML to resolve relative paths from the html string.
        /// </summary>
        /// <param name="baseUrl">Header base url.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderBaseUrl(string baseUrl)
        {
            if (!baseUrl.StartsWith("http://", true, null) && !baseUrl.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the base url are http:// and https://.");
            }
            if (baseUrl.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }

            parameters["header_base_url"] = baseUrl;
            return this;
        }

        /// <summary>
        /// Control the visibility of the header on the first page of the generated pdf document. The default value is True.
        /// </summary>
        /// <param name="displayOnFirstPage">Display header on the first page or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderDisplayOnFirstPage(bool displayOnFirstPage)
        {
            parameters["header_display_on_first_page"] = displayOnFirstPage.ToString();
            return this;
        }

        /// <summary>
        /// Control the visibility of the header on the odd numbered pages of the generated pdf document. The default value is True.
        /// </summary>
        /// <param name="displayOnOddPages">Display header on odd pages or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderDisplayOnOddPages(bool displayOnOddPages)
        {
            parameters["header_display_on_odd_pages"] = displayOnOddPages.ToString();
            return this;
        }

        /// <summary>
        /// Control the visibility of the header on the even numbered pages of the generated pdf document. The default value is True.
        /// </summary>
        /// <param name="displayOnEvenPages">Display header on even pages or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderDisplayOnEvenPages(bool displayOnEvenPages)
        {
            parameters["header_display_on_even_pages"] = displayOnEvenPages.ToString();
            return this;
        }

        /// <summary>
        /// Set the width in pixels used by the converter's internal browser window during the conversion of the header content. The default value is 1024px.
        /// </summary>
        /// <param name="headerWebPageWidth">Browser window width in pixels.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderWebPageWidth(int headerWebPageWidth)
        {
            parameters["header_web_page_width"] = headerWebPageWidth.ToString();
            return this;
        }

        /// <summary>
        /// Set the height in pixels used by the converter's internal browser window during the conversion of the header content. 
        /// The default value is 0px and it means that the page height is automatically calculated by the converter.
        /// </summary>
        /// <param name="headerWebPageHeight">Browser window height in pixels. Set it to 0px to automatically calculate page height.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setHeaderWebPageHeight(int headerWebPageHeight)
        {
            parameters["header_web_page_height"] = headerWebPageHeight.ToString();
            return this;
        }


        /// <summary>
        /// Control if a custom footer is displayed in the generated PDF document. The default value is False.
        /// </summary>
        /// <param name="showFooter">Show footer or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setShowFooter(bool showFooter)
        {
            parameters["show_footer"] = showFooter.ToString();
            return this;
        }

        /// <summary>
        /// The height of the pdf document footer. This height is specified in points. 1 point is 1/72 inch. The default value is 50.
        /// </summary>
        /// <param name="height">Footer height.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterHeight(int height)
        {
            parameters["footer_height"] = height.ToString();
            return this;
        }

        /// <summary>
        /// Set the url of the web page that is converted and rendered in the PDF document footer.
        /// </summary>
        /// <param name="url">The url of the web page that is converted and rendered in the pdf document footer.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterUrl(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the url are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }

            parameters["footer_url"] = url;
            return this;
        }

        /// <summary>
        /// Set the raw html that is converted and rendered in the pdf document footer.
        /// </summary>
        /// <param name="html">The raw html that is converted and rendered in the pdf document footer.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterHtml(string html)
        {
            parameters["footer_html"] = html;
            return this;
        }

        /// <summary>
        /// Set an optional base url parameter can be used together with the footer HTML to resolve relative paths from the html string.
        /// </summary>
        /// <param name="baseUrl">Footer base url.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterBaseUrl(string baseUrl)
        {
            if (!baseUrl.StartsWith("http://", true, null) && !baseUrl.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the base url are http:// and https://.");
            }
            if (baseUrl.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls. SelectPdf online API can only convert publicly available urls.");
            }

            parameters["footer_base_url"] = baseUrl;
            return this;
        }

        /// <summary>
        /// Control the visibility of the footer on the first page of the generated pdf document. The default value is True.
        /// </summary>
        /// <param name="displayOnFirstPage">Display footer on the first page or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterDisplayOnFirstPage(bool displayOnFirstPage)
        {
            parameters["footer_display_on_first_page"] = displayOnFirstPage.ToString();
            return this;
        }

        /// <summary>
        /// Control the visibility of the footer on the odd numbered pages of the generated pdf document. The default value is True.
        /// </summary>
        /// <param name="displayOnOddPages">Display footer on odd pages or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterDisplayOnOddPages(bool displayOnOddPages)
        {
            parameters["footer_display_on_odd_pages"] = displayOnOddPages.ToString();
            return this;
        }

        /// <summary>
        /// Control the visibility of the footer on the even numbered pages of the generated pdf document. The default value is True.
        /// </summary>
        /// <param name="displayOnEvenPages">Display footer on even pages or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterDisplayOnEvenPages(bool displayOnEvenPages)
        {
            parameters["footer_display_on_even_pages"] = displayOnEvenPages.ToString();
            return this;
        }

        /// <summary>
        /// Add a special footer on the last page of the generated pdf document only. The default value is False.
        /// </summary>
        /// <remarks>
        /// Use <see cref="setFooterUrl"/> or <see cref="setFooterHtml"/> and <see cref="setFooterBaseUrl"/> to specify the content of the last page footer.
        /// Use <see cref="setFooterHeight"/> to specify the height of the special last page footer.
        /// </remarks>
        /// <param name="displayOnLastPage">Display special footer on the last page or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterDisplayOnLastPage(bool displayOnLastPage)
        {
            parameters["footer_display_on_last_page"] = displayOnLastPage.ToString();
            return this;
        }

        /// <summary>
        /// Set the width in pixels used by the converter's internal browser window during the conversion of the footer content. The default value is 1024px.
        /// </summary>
        /// <param name="footerWebPageWidth">Browser window width in pixels.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterWebPageWidth(int footerWebPageWidth)
        {
            parameters["footer_web_page_width"] = footerWebPageWidth.ToString();
            return this;
        }

        /// <summary>
        /// Set the height in pixels used by the converter's internal browser window during the conversion of the footer content. 
        /// The default value is 0px and it means that the page height is automatically calculated by the converter.
        /// </summary>
        /// <param name="footerWebPageHeight">Browser window height in pixels. Set it to 0px to automatically calculate page height.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setFooterWebPageHeight(int footerWebPageHeight)
        {
            parameters["footer_web_page_height"] = footerWebPageHeight.ToString();
            return this;
        }


        /// <summary>
        /// Show page numbers. Default value is True.
        /// </summary>
        /// <remarks>Page numbers will be displayed in the footer of the PDF document.</remarks>
        /// <param name="showPageNumbers">Show page numbers or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setShowPageNumbers(bool showPageNumbers)
        {
            parameters["page_numbers"] = showPageNumbers.ToString();
            return this;
        }

        /// <summary>
        /// Control the page number for the first page being rendered. The default value is 1.
        /// </summary>
        /// <param name="firstPageNumber">First page number.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersFirst(int firstPageNumber)
        {
            parameters["page_numbers_first"] = firstPageNumber.ToString();
            return this;
        }

        /// <summary>
        /// Control the total number of pages offset in the generated pdf document. The default value is 0.
        /// </summary>
        /// <param name="totalPagesOffset">Offset for the total number of pages in the generated pdf document.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersOffset(int totalPagesOffset)
        {
            parameters["page_numbers_offset"] = totalPagesOffset.ToString();
            return this;
        }

        /// <summary>
        /// Set the text that is used to display the page numbers. 
        /// It can contain the placeholder {page_number} for the current page number and {total_pages} for the total number of pages. 
        /// The default value is "Page: {page_number} of {total_pages}".
        /// </summary>
        /// <param name="template">Page numbers template.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersTemplate(string template)
        {
            parameters["page_numbers_template"] = template;
            return this;
        }

        /// <summary>
        /// Set the font used to display the page numbers text. The default value is "Helvetica".
        /// </summary>
        /// <param name="fontName">The font used to display the page numbers text.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersFontName(string fontName)
        {
            parameters["page_numbers_font_name"] = fontName;
            return this;
        }

        /// <summary>
        /// Set the size of the font used to display the page numbers. The default value is 10 points.
        /// </summary>
        /// <param name="fontSize">The size in points of the font used to display the page numbers.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersFontSize(int fontSize)
        {
            parameters["page_numbers_font_size"] = fontSize.ToString();
            return this;
        }

        /// <summary>
        /// Set the alignment of the page numbers text. The default value is PageNumbersAlignment.Right.
        /// </summary>
        /// <param name="alignment">The alignment of the page numbers text.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersAlignment(PageNumbersAlignment alignment)
        {
            parameters["page_numbers_alignment"] = ((int)alignment).ToString();
            return this;
        }

        /// <summary>
        /// Specify the color of the page numbers text in #RRGGBB html format. The default value is #333333.
        /// </summary>
        /// <param name="color">Page numbers color.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersColor(string color)
        {
            if (!Regex.Match(color, "^#?[0-9a-fA-F]{6}$").Success)
                throw new ApiException("Color value must be in #RRGGBB format.");

            parameters["page_numbers_color"] = color;
            return this;
        }

        /// <summary>
        /// Specify the position in points on the vertical where the page numbers text is displayed in the footer. The default value is 10 points.
        /// </summary>
        /// <param name="position">Page numbers Y position in points.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageNumbersVerticalPosition(int position)
        {
            parameters["page_numbers_pos_y"] = position.ToString();
            return this;
        }

        /// <summary>
        /// Generate automatic bookmarks in pdf. The elements that will be bookmarked are defined using CSS selectors. 
        /// For example, the selector for all the H1 elements is "H1", the selector for all the elements with the CSS class name 'myclass' is "*.myclass" and 
        /// the selector for the elements with the id 'myid' is "*#myid". Read more about CSS selectors <a href="http://www.w3schools.com/cssref/css_selectors.asp" target="_blank">here</a>.
        /// </summary>
        /// <param name="selectors">CSS selectors used to identify HTML elements, comma separated.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPdfBookmarksSelectors(string selectors)
        {
            parameters["pdf_bookmarks_selectors"] = selectors;
            return this;
        }

        /// <summary>
        /// Exclude page elements from the conversion. The elements that will be excluded are defined using CSS selectors. 
        /// For example, the selector for all the H1 elements is "H1", the selector for all the elements with the CSS class name 'myclass' is "*.myclass" and 
        /// the selector for the elements with the id 'myid' is "*#myid". Read more about CSS selectors <a href="http://www.w3schools.com/cssref/css_selectors.asp" target="_blank">here</a>.
        /// </summary>
        /// <param name="selectors">CSS selectors used to identify HTML elements, comma separated.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPdfHideElements(string selectors)
        {
            parameters["pdf_hide_elements"] = selectors;
            return this;
        }

        /// <summary>
        /// Convert only a specific section of the web page to pdf. 
        /// The section that will be converted to pdf is specified by the html element ID. 
        /// The element can be anything (image, table, table row, div, text, etc).
        /// </summary>
        /// <param name="elementID">HTML element ID.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPdfShowOnlyElementID(string elementID)
        {
            parameters["pdf_show_only_element_id"] = elementID;
            return this;
        }

        /// <summary>
        /// Get the locations of page elements from the conversion. The elements that will have their locations retrieved are defined using CSS selectors. 
        /// For example, the selector for all the H1 elements is "H1", the selector for all the elements with the CSS class name 'myclass' is "*.myclass" and 
        /// the selector for the elements with the id 'myid' is "*#myid". Read more about CSS selectors <a href="http://www.w3schools.com/cssref/css_selectors.asp" target="_blank">here</a>.
        /// </summary>
        /// <param name="selectors">CSS selectors used to identify HTML elements, comma separated.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPdfWebElementsSelectors(string selectors)
        {
            parameters["pdf_web_elements_selectors"] = selectors;
            return this;
        }

        /// <summary>
        /// Set converter startup mode. The default value is StartupMode.Automatic and the conversion is started immediately.
        /// </summary>
        /// <remarks>
        /// By default this is set to <see cref="StartupMode.Automatic"/> and the conversion is started as soon as the page loads (and conversion delay set with <see cref="setConversionDelay"/> elapses). 
        /// If set to <see cref="StartupMode.Manual"/>, the conversion is started only by a javascript call to <c>SelectPdf.startConversion()</c> from within the web page.
        /// </remarks>
        /// <param name="startupMode">Converter startup mode.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setStartupMode(StartupMode startupMode)
        {
            parameters["startup_mode"] = startupMode.ToString();
            return this;
        }

        /// <summary>
        /// Internal use only.
        /// </summary>
        /// <param name="skipDecoding">The default value is True.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setSkipDecoding(bool skipDecoding)
        {
            parameters["skip_decoding"] = skipDecoding.ToString();
            return this;
        }

        /// <summary>
        /// Set a flag indicating if the images from the page are scaled during the conversion process. The default value is False and images are not scaled.
        /// </summary>
        /// <param name="scaleImages">Scale images or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setScaleImages(bool scaleImages)
        {
            parameters["scale_images"] = scaleImages.ToString();
            return this;
        }

        /// <summary>
        /// Generate a single page PDF. The converter will automatically resize the PDF page to fit all the content in a single page.
        /// The default value of this property is False and the PDF will contain several pages if the content is large.
        /// </summary>
        /// <param name="generateSinglePagePdf">Generate a single page PDF or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setSinglePagePdf(bool generateSinglePagePdf)
        {
            parameters["single_page_pdf"] = generateSinglePagePdf.ToString();
            return this;
        }

        /// <summary>
        /// Get or set a flag indicating if an enhanced custom page breaks algorithm is used. 
        /// The enhanced algorithm is a little bit slower but it will prevent the appearance of hidden text in the PDF when custom page breaks are used.
        /// The default value for this property is False.
        /// </summary>
        /// <param name="enableEnhancedPageBreaksAlgorithm">Enable enhanced page breaks algorithm or not.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setPageBreaksEnhancedAlgorithm(bool enableEnhancedPageBreaksAlgorithm)
        {
            parameters["page_breaks_enhanced_algorithm"] = enableEnhancedPageBreaksAlgorithm.ToString();
            return this;
        }

        /// <summary>
        /// Set HTTP cookies for the web page being converted.
        /// </summary>
        /// <param name="cookies">HTTP cookies that will be sent to the page being converted.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setCookies(Dictionary<string, string> cookies)
        {
            parameters["cookies_string"] = SerializeDictionary(cookies);
            return this;
        }

        /// <summary>
        /// Set a custom parameter. Do not use this method unless advised by SelectPdf.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <returns>Reference to the current object.</returns>
        public HtmlToPdfClient setCustomParameter(string parameterName, string parameterValue)
        {
            parameters[parameterName] = parameterValue;
            return this;
        }

        /// <summary>
        /// Get the number of pages of the PDF document resulted from the API call.
        /// </summary>
        /// <returns>Number of pages of the PDF document.</returns>
        public int getNumberOfPage()
        {
            return numberOfPages;
        }

        /// <summary>
        /// Get the locations of certain web elements. This is retrieved if pdf_web_elements_selectors parameter is set and elements were found to match the selectors.
        /// </summary>
        /// <returns>List of web elements locations.</returns>
        public IList<WebElement> getWebElements()
        {
            WebElementsClient webElementsClient = new WebElementsClient(parameters["key"], jobId);
            webElementsClient.setApiEndpoint(apiWebElementsEndpoint);

            webElements = webElementsClient.getWebElements();
            return webElements;
        }
    }
}
