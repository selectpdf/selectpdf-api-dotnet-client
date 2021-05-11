using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// SelectPdf Online API .NET Client.
/// </summary>
namespace SelectPdf.Api
{
    /// <summary>
    /// Base class for API clients. Do not use this directly.
    /// </summary>
    public class ApiClient
    {
        /// <summary>
        /// API endpoint
        /// </summary>
        protected string apiEndpoint = "https://selectpdf.com/api2/convert/";

        /// <summary>
        /// Parameters that will be sent to the API.
        /// </summary>
        protected Dictionary<string, string> parameters = new Dictionary<string, string>();

        /// <summary>
        /// HTTP Headers that will be sent to the API.
        /// </summary>
        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        /// <summary>
        /// Number of pages of the pdf document resulted from the conversion.
        /// </summary>
        protected int numberOfPages = 0;

        /// <summary>
        /// Web elements locations. This is retrieved if pdf_web_elements_selectors parameter is set and elements were found to match the selectors.
        /// </summary>
        protected IList<WebElement> webElements = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ApiClient()
        {
            // Up to 8 concurrent connections by default
            System.Net.ServicePointManager.DefaultConnectionLimit = 8;
        }

        /// <summary>
        /// Set the number of concurrent connections (between 1 and 16).
        /// </summary>
        /// <param name="connectionsNumber">Number of concurrent connections.</param>
        public void setConcurrentConnections(int connectionsNumber)
        {
            if (connectionsNumber < 1 || System.Net.ServicePointManager.DefaultConnectionLimit > 16)
            {
                connectionsNumber = 8;
            }
            System.Net.ServicePointManager.DefaultConnectionLimit = connectionsNumber;
        }

        /// <summary>
        /// Set a custom SelectPdf API endpoint. Do not use this method unless advised by SelectPdf.
        /// </summary>
        /// <param name="apiEndpoint">API endpoint.</param>
        public void setApiEndpoint(string apiEndpoint)
        {
            this.apiEndpoint = apiEndpoint;
        }

        /// <summary>
        /// Serialize parameters.
        /// </summary>
        /// <returns>Serialized parameters.</returns>
        protected string SerializeParameters()
        {
            string sParameters = "";

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                sParameters += parameter.Key + "=" + EncodeString(parameter.Value) + "&";
            }

            return sParameters;
        }

        /// <summary>
        /// Serialize dictionary.
        /// </summary>
        /// <returns>Serialized dictionary.</returns>
        protected string SerializeDictionary(Dictionary<string, string> dictionaryToSerialize)
        {
            string sString = "";

            foreach (KeyValuePair<string, string> entry in dictionaryToSerialize)
            {
                sString += entry.Key + "=" + EncodeString(entry.Value) + "&";
            }

            return sString;
        }

        /// <summary>
        /// Custom encoding method, to overcome the size limitation of Uri.EscapeDataString.
        /// </summary>
        /// <param name="str">String to encode.</param>
        /// <returns>Encoded string.</returns>
        private string EncodeString(string str)
        {
            //maxLengthAllowed .NET < 4.5 = 32765;
            //maxLengthAllowed .NET >= 4.5 = 65519;
            int maxLengthAllowed = 32765;
            StringBuilder sb = new StringBuilder();
            int loops = str.Length / maxLengthAllowed;

            for (int i = 0; i <= loops; i++)
            {
                sb.Append(Uri.EscapeDataString(i < loops
                    ? str.Substring(maxLengthAllowed * i, maxLengthAllowed)
                    : str.Substring(maxLengthAllowed * i)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a POST request.
        /// </summary>
        /// <param name="outStream">Output response to this stream, if specified.</param>
        /// <returns>If output stream is not specified, return response as byte array.</returns>
        protected byte[] PerformPost(Stream outStream)
        {
            numberOfPages = 0;

            // serialize parameters
            string serializedParameters = SerializeParameters();
            byte[] byteData = Encoding.UTF8.GetBytes(serializedParameters);

            // create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            if (request == null)
            {
                throw new ApiException("Could not establish a connection with the API endpoint: " + apiEndpoint);
            }

            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 6000000; //6,000,000ms=6,000s=100min
            request.ReadWriteTimeout = 6000000; //6,000,000ms=6,000s=100min
            request.MaximumResponseHeadersLength = 102400; // size in Kilobytes = 100Mb = 100 * 1024 kb. cannot use -1 (unlimited) because it does not work on .NET Core and .NET 5.

            // send headers
            foreach (KeyValuePair<string, string> header in headers)
            {
                if (WebHeaderCollection.IsRestricted(header.Key))
                {
                    if (header.Key.ToLower() == "accept")
                    {
                        request.Accept = header.Value;
                    }
                }
                else
                {
                    string headerName = header.Key;
                    string headerValue = header.Value;
                    string encodedHeaderName;
                    string encodedHeaderValue;

                    HeaderNameValueEncode(headerName, headerValue, out encodedHeaderName, out encodedHeaderValue);
                    request.Headers.Add(encodedHeaderName, encodedHeaderValue);
                }
            }

            // POST parameters
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteData, 0, byteData.Length);
            dataStream.Flush();
            dataStream.Close();

            // GET response (if response code is not 200 OK, a WebException is raised)
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // get number of pages from the header
                    try
                    {
                        if (!string.IsNullOrEmpty(response.Headers["selectpdf-api-pages"]))
                        {
                            numberOfPages = Convert.ToInt32(response.Headers["selectpdf-api-pages"]);
                        }
                    } catch { }

                    // get web elements
                    string base64EncodedWebElementsHeadersCount = response.Headers["selectpdf-api-web-elements-headers-count"];
                    if (!string.IsNullOrEmpty(base64EncodedWebElementsHeadersCount))
                    {
                        StringBuilder allHeaders = new StringBuilder();
                        int headersCount = Convert.ToInt32(base64EncodedWebElementsHeadersCount);

                        for (int i=1; i<=headersCount; i++)
                        {
                            allHeaders.Append(response.Headers["selectpdf-api-web-elements-header-" + i]);
                        }

                        string base64EncodedWebElements = allHeaders.ToString(); // this is a base64 encoded serialized json

                        // decode from base64
                        byte[] byteArray = Convert.FromBase64String(base64EncodedWebElements);
                        string webElementsJson = Encoding.UTF8.GetString(byteArray);

                        // deserialize json
                        webElements = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<WebElement>>(webElementsJson);
                    }


                    // Get the response stream with the content returned by the server
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (outStream != null)
                        {
                            BinaryCopyStream(responseStream, outStream);
                            outStream.Position = 0;
                            return null;
                        }

                        using (MemoryStream output = new MemoryStream())
                        {
                            BinaryCopyStream(responseStream, output);
                            return output.ToArray();
                        }
                    }
                }
                else
                {
                    // error - get error message
                    throw new ApiException(string.Format("({0}) {1}", (int)response.StatusCode, response.StatusDescription));
                }
            }
            catch (WebException webEx)
            {
                // an error occurred
                HttpWebResponse response = (HttpWebResponse)webEx.Response;

                if (response == null)
                {
                    throw new ApiException(string.Format("Could not get a response from the API endpoint: {0}. Web Exception: {1}.", apiEndpoint, webEx.Message), webEx);
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();

                    // get details of the error message if available (text read!!!)
                    StreamReader readStream = new StreamReader(responseStream);
                    string message = readStream.ReadToEnd();
                    responseStream.Close();

                    //throw new ApiException(string.Format("({0}) {1} - {2}", response.StatusCode, response.StatusDescription, message), webEx);
                    throw new ApiException(string.Format("({0}) {1}", (int)response.StatusCode, message), webEx);
                }
            }
        }


        /// <summary>
        /// Binary read from Stream into a MemoryStream.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <returns>Output memory stream.</returns>
        protected MemoryStream BinaryReadStream(Stream input)
        {
            int bytesNumber = 0;
            byte[] bytes = new byte[8192];
            MemoryStream stream = new MemoryStream();
            BinaryReader reader = new BinaryReader(input);

            do
            {
                bytesNumber = reader.Read(bytes, 0, bytes.Length);
                if (bytesNumber > 0)
                    stream.Write(bytes, 0, bytesNumber);
            } while (bytesNumber > 0);

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Copy from one stream into another.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream.</param>
        protected void BinaryCopyStream(Stream input, Stream output)
        {
            byte[] bytes = new byte[8192];
            while (true)
            {
                int bytesNumber = input.Read(bytes, 0, bytes.Length);
                if (bytesNumber <= 0)
                    return;
                output.Write(bytes, 0, bytesNumber);
            }
        }

        private void HeaderNameValueEncode(string headerName, string headerValue, out string encodedHeaderName, out string encodedHeaderValue)
        {
            if (String.IsNullOrEmpty(headerName))
                encodedHeaderName = headerName;
            else
                encodedHeaderName = EncodeHeaderString(headerName);

            if (String.IsNullOrEmpty(headerValue))
                encodedHeaderValue = headerValue;
            else
                encodedHeaderValue = EncodeHeaderString(headerValue);
        }

        private void StringBuilderAppend(string s, ref StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder(s);
            else
                sb.Append(s);
        }

        private string EncodeHeaderString(string input)
        {
            StringBuilder sb = null;

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if ((ch < 32 && ch != 9) || ch == 127)
                    StringBuilderAppend(String.Format("%{0:x2}", (int)ch), ref sb);
            }

            if (sb != null)
                return sb.ToString();

            return input;
        }

    }
}
