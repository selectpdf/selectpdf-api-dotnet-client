using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SelectPdf.Api
{
    /// <summary>
    /// Pdf To Text Conversion with SelectPdf Online API.
    /// </summary>

    public class PdfToTextClient : ApiClient
    {
        /// <summary>
        /// Construct the Pdf To Text Client.
        /// </summary>
        /// <param name="apiKey">API key.</param>
        public PdfToTextClient(string apiKey)
        {
            apiEndpoint = "https://selectpdf.com/api2/pdftotext/";
            parameters["key"] = apiKey;
        }

        /// <summary>
        /// Get the text from the specified pdf.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <returns>Extracted text.</returns>
        public string getTextFromFile(string inputPdf)
        {
            parameters["async"] = "False";
            parameters["action"] = "Convert";
            parameters["url"] = string.Empty;

            files.Clear();
            files["inputPdf"] = inputPdf;

            byte[] result = PerformPostAsMultipartFormData(null);
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Get the text from the specified pdf and write it to the specified text file.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="outputFilePath">The output file where the resulted text will be written.</param>
        public void getTextFromFileToFile(string inputPdf, string outputFilePath)
        {
            string result = getTextFromFile(inputPdf);
            File.WriteAllText(outputFilePath, result);
        }

        /// <summary>
        /// Get the text from the specified pdf and write it to the specified stream.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void getTextFromFileToStream(string inputPdf, Stream stream)
        {
            string result = getTextFromFile(inputPdf);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(result);
        }

        /// <summary>
        /// Get the text from the specified pdf with an asynchronous call.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <returns>Extracted text.</returns>
        public string getTextFromFileAsync(string inputPdf)
        {
            parameters["action"] = "Convert";
            parameters["url"] = string.Empty;

            files.Clear();
            files["inputPdf"] = inputPdf;

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

                if (result != null)
                {
                    numberOfPages = asyncJobClient.getNumberOfPage();

                    return Encoding.UTF8.GetString(result);
                }

            } while (noPings <= AsyncCallsMaxPings);

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Get the text from the specified pdf with an asynchronous call and write it to the specified text file.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="outputFilePath">The output file where the resulted text will be written.</param>
        public void getTextFromFileToFileAsync(string inputPdf, string outputFilePath)
        {
            string result = getTextFromFileAsync(inputPdf);
            File.WriteAllText(outputFilePath, result);
        }

        /// <summary>
        /// Get the text from the specified pdf with an asynchronous call and write it to the specified stream.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void getTextFromFileToStreamAsync(string inputPdf, Stream stream)
        {
            string result = getTextFromFileAsync(inputPdf);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(result);
        }

        /// <summary>
        /// Search for a specific text in a PDF document. The search is case insensitive and returns partial words also.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchFile(string inputPdf, string textToSearch)
        {
            return searchFile(inputPdf, textToSearch, false, false);
        }

        /// <summary>
        /// Search for a specific text in a PDF document.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <param name="caseSensitive">If the search is case sensitive or not.</param>
        /// <param name="wholeWordsOnly">If the search works on whole words or not.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchFile(string inputPdf, string textToSearch, bool caseSensitive, bool wholeWordsOnly)
        {
            if (string.IsNullOrEmpty(textToSearch))
            {
                throw new ApiException("Search text cannot be empty.");
            }

            parameters["async"] = "False";
            parameters["action"] = "Search";
            parameters["url"] = string.Empty;
            parameters["search_text"] = textToSearch;
            parameters["case_sensitive"] = caseSensitive.ToString();
            parameters["whole_words_only"] = wholeWordsOnly.ToString();

            files.Clear();
            files["inputPdf"] = inputPdf;

            headers["Accept"] = "application/json";
            MemoryStream outStream = new MemoryStream();
            PerformPostAsMultipartFormData(outStream);

            try
            {
                // get json from stream
                string textPositionsJson = new StreamReader(outStream).ReadToEnd();
                outStream.Close();

                // deserialize json
                IList<TextPosition> textPositions = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<TextPosition>>(textPositionsJson);

                return textPositions;
            }
            catch (Exception ex)
            {
                throw new ApiException("Could not get search results.", ex.InnerException);
            }
        }

        /// <summary>
        /// Search for a specific text in a PDF document with an asynchronous call. The search is case insensitive and returns partial words also.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchFileAsync(string inputPdf, string textToSearch)
        {
            return searchFileAsync(inputPdf, textToSearch, false, false);
        }

        /// <summary>
        /// Search for a specific text in a PDF document with an asynchronous call.
        /// </summary>
        /// <param name="inputPdf">Path to a local PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <param name="caseSensitive">If the search is case sensitive or not.</param>
        /// <param name="wholeWordsOnly">If the search works on whole words or not.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchFileAsync(string inputPdf, string textToSearch, bool caseSensitive, bool wholeWordsOnly)
        {
            if (string.IsNullOrEmpty(textToSearch))
            {
                throw new ApiException("Search text cannot be empty.");
            }

            parameters["action"] = "Search";
            parameters["url"] = string.Empty;
            parameters["search_text"] = textToSearch;
            parameters["case_sensitive"] = caseSensitive.ToString();
            parameters["whole_words_only"] = wholeWordsOnly.ToString();

            files.Clear();
            files["inputPdf"] = inputPdf;

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

                if (result != null)
                {
                    numberOfPages = asyncJobClient.getNumberOfPage();

                    try
                    {
                        // get json from stream
                        string textPositionsJson = Encoding.UTF8.GetString(result);

                        // deserialize json
                        IList<TextPosition> textPositions = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<TextPosition>>(textPositionsJson);

                        return textPositions;
                    }
                    catch (Exception ex)
                    {
                        throw new ApiException("Could not get search results.", ex.InnerException);
                    }
                }

            } while (noPings <= AsyncCallsMaxPings);

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Search for a specific text in a PDF document. The search is case insensitive and returns partial words also.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchUrl(string url, string textToSearch)
        {
            return searchUrl(url, textToSearch, false, false);
        }

        /// <summary>
        /// Search for a specific text in a PDF document.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <param name="caseSensitive">If the search is case sensitive or not.</param>
        /// <param name="wholeWordsOnly">If the search works on whole words or not.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchUrl(string url, string textToSearch, bool caseSensitive, bool wholeWordsOnly)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the PDFs available online are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls via this method. Use getTextFromFile instead.");
            }

            if (string.IsNullOrEmpty(textToSearch))
            {
                throw new ApiException("Search text cannot be empty.");
            }

            parameters["async"] = "False";
            parameters["action"] = "Search";
            parameters["search_text"] = textToSearch;
            parameters["case_sensitive"] = caseSensitive.ToString();
            parameters["whole_words_only"] = wholeWordsOnly.ToString();

            files.Clear();
            parameters["url"] = url;

            headers["Accept"] = "application/json";
            MemoryStream outStream = new MemoryStream();
            PerformPostAsMultipartFormData(outStream);

            try
            {
                // get json from stream
                string textPositionsJson = new StreamReader(outStream).ReadToEnd();
                outStream.Close();

                // deserialize json
                IList<TextPosition> textPositions = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<TextPosition>>(textPositionsJson);

                return textPositions;
            }
            catch (Exception ex)
            {
                throw new ApiException("Could not get search results.", ex.InnerException);
            }
        }

        /// <summary>
        /// Search for a specific text in a PDF document with an asynchronous call. The search is case insensitive and returns partial words also.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchUrlAsync(string url, string textToSearch)
        {
            return searchUrlAsync(url, textToSearch, false, false);
        }

        /// <summary>
        /// Search for a specific text in a PDF document with an asynchronous call.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="textToSearch">Text to search.</param>
        /// <param name="caseSensitive">If the search is case sensitive or not.</param>
        /// <param name="wholeWordsOnly">If the search works on whole words or not.</param>
        /// <returns>List with text positions in the current PDF document.</returns>
        /// <remarks>
        /// Pages that participate to this operation are specified by <see cref="setStartPage"/> and <see cref="setEndPage"/> methods.
        /// </remarks>
        public IList<TextPosition> searchUrlAsync(string url, string textToSearch, bool caseSensitive, bool wholeWordsOnly)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the PDFs available online are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls via this method. Use getTextFromFile instead.");
            }

            if (string.IsNullOrEmpty(textToSearch))
            {
                throw new ApiException("Search text cannot be empty.");
            }

            parameters["action"] = "Search";
            parameters["search_text"] = textToSearch;
            parameters["case_sensitive"] = caseSensitive.ToString();
            parameters["whole_words_only"] = wholeWordsOnly.ToString();

            files.Clear();
            parameters["url"] = url;

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

                if (result != null)
                {
                    numberOfPages = asyncJobClient.getNumberOfPage();

                    try
                    {
                        // get json from stream
                        string textPositionsJson = Encoding.UTF8.GetString(result);

                        // deserialize json
                        IList<TextPosition> textPositions = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<TextPosition>>(textPositionsJson);

                        return textPositions;
                    }
                    catch (Exception ex)
                    {
                        throw new ApiException("Could not get search results.", ex.InnerException);
                    }
                }

            } while (noPings <= AsyncCallsMaxPings);

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Get the text from the specified pdf.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <returns>Extracted text.</returns>
        public string getTextFromUrl(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the PDFs available online are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls via this method. Use getTextFromFile instead.");
            }

            parameters["async"] = "False";
            parameters["action"] = "Convert";

            files.Clear();
            parameters["url"] = url;

            byte[] result = PerformPostAsMultipartFormData(null);
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Get the text from the specified pdf and write it to the specified text file.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="outputFilePath">The output file where the resulted text will be written.</param>
        public void getTextFromUrlToFile(string url, string outputFilePath)
        {
            string result = getTextFromUrl(url);
            File.WriteAllText(outputFilePath, result);
        }

        /// <summary>
        /// Get the text from the specified pdf and write it to the specified stream.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void getTextFromUrlToStream(string url, Stream stream)
        {
            string result = getTextFromUrl(url);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(result);
        }

        /// <summary>
        /// Get the text from the specified pdf with an asynchronous call.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <returns>Extracted text.</returns>
        public string getTextFromUrlAsync(string url)
        {
            if (!url.StartsWith("http://", true, null) && !url.StartsWith("https://", true, null))
            {
                throw new ApiException("The supported protocols for the PDFs available online are http:// and https://.");
            }
            if (url.StartsWith("http://localhost", true, null))
            {
                throw new ApiException("Cannot convert local urls via this method. Use getTextFromFile instead.");
            }

            parameters["action"] = "Convert";

            files.Clear();
            parameters["url"] = url;

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

                if (result != null)
                {
                    numberOfPages = asyncJobClient.getNumberOfPage();

                    return Encoding.UTF8.GetString(result);
                }

            } while (noPings <= AsyncCallsMaxPings);

            throw new ApiException("Asynchronous call did not finish in expected timeframe.");
        }

        /// <summary>
        /// Get the text from the specified pdf with an asynchronous call and write it to the specified text file.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="outputFilePath">The output file where the resulted text will be written.</param>
        public void getTextFromUrlToFileAsync(string url, string outputFilePath)
        {
            string result = getTextFromUrlAsync(url);
            File.WriteAllText(outputFilePath, result);
        }

        /// <summary>
        /// Get the text from the specified pdf with an asynchronous call and write it to the specified stream.
        /// </summary>
        /// <param name="url">Address of the PDF file.</param>
        /// <param name="stream">The output stream where the resulted PDF will be written.</param>
        public void getTextFromUrlToStreamAsync(string url, Stream stream)
        {
            string result = getTextFromUrlAsync(url);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(result);
        }

        /// <summary>
        /// Set Start Page number. Default value is 1 (first page of the document).
        /// </summary>
        /// <param name="startPage">Start page number (1-based).</param>
        /// <returns>Reference to the current object.</returns>
        public PdfToTextClient setStartPage(int startPage)
        {
            parameters["start_page"] = startPage.ToString();
            return this;
        }

        /// <summary>
        /// Set End Page number. Default value is 0 (process till the last page of the document).
        /// </summary>
        /// <param name="endPage">End page number (1-based).</param>
        /// <returns>Reference to the current object.</returns>
        public PdfToTextClient setEndPage(int endPage)
        {
            parameters["end_page"] = endPage.ToString();
            return this;
        }

        /// <summary>
        /// Set PDF user password.
        /// </summary>
        /// <param name="userPassword">PDF user password.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfToTextClient setUserPassword(string userPassword)
        {
            parameters["user_password"] = userPassword;
            return this;
        }

        /// <summary>
        /// Set the text layout. The default value is TextLayout.Original.
        /// </summary>
        /// <param name="textLayout">The text layout.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfToTextClient setTextLayout(TextLayout textLayout)
        {
            parameters["text_layout"] = ((int)textLayout).ToString();
            return this;
        }

        /// <summary>
        /// Set the output format. The default value is OutputFormat.Text.
        /// </summary>
        /// <param name="outputFormat">The output format.</param>
        /// <returns>Reference to the current object.</returns>
        public PdfToTextClient setOutputFormat(OutputFormat outputFormat)
        {
            parameters["output_format"] = ((int)outputFormat).ToString();
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
        public PdfToTextClient setTimeout(int timeout)
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
        public PdfToTextClient setCustomParameter(string parameterName, string parameterValue)
        {
            parameters[parameterName] = parameterValue;
            return this;
        }

        /// <summary>
        /// Get the number of pages processed from the PDF document.
        /// </summary>
        /// <returns>Number of pages processed from the PDF document.</returns>
        public int getNumberOfPage()
        {
            return numberOfPages;
        }

    }
}
