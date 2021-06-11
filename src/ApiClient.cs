using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

#if NET_STANDARD_20
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
#endif

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
        /// API async jobs endpoint
        /// </summary>
        protected string apiAsyncEndpoint = "https://selectpdf.com/api2/asyncjob/";

        /// <summary>
        /// API web elements endpoint
        /// </summary>
        protected string apiWebElementsEndpoint = "https://selectpdf.com/api2/webelements/";

        /// <summary>
        /// Parameters that will be sent to the API.
        /// </summary>
        protected Dictionary<string, string> parameters = new Dictionary<string, string>();

        /// <summary>
        /// HTTP Headers that will be sent to the API.
        /// </summary>
        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        /// <summary>
        /// Files that will be sent to the API.
        /// </summary>
        protected Dictionary<string, string> files = new Dictionary<string, string>();

        /// <summary>
        /// Binary data that will be sent to the API.
        /// </summary>
        protected Dictionary<string, byte[]> binaryData = new Dictionary<string, byte[]>();

        /// <summary>
        /// Number of pages of the pdf document resulted from the conversion.
        /// </summary>
        protected int numberOfPages = 0;

        /// <summary>
        /// Job ID for asynchronous calls or for calls that require a second request.
        /// </summary>
        protected string jobId = string.Empty;

        /// <summary>
        /// Web elements locations. This is retrieved if pdf_web_elements_selectors parameter is set and elements were found to match the selectors.
        /// </summary>
        protected IList<WebElement> webElements = null;

        private const string MULTIPART_FORM_DATA_BOUNDARY = "------------SelectPdf_Api_Boundry_$";
        private const string NEW_LINE = "\r\n";

#if NET_STANDARD_20
        private static readonly HttpClient staticHttpClient;
#endif

        /// <summary>
        /// Ping interval in seconds for asynchronous calls. Default value is 3 seconds.
        /// </summary>
        public int AsyncCallsPingInterval
        {
            get; set;
        }

        /// <summary>
        /// Maximum number of pings for asynchronous calls. Default value is 1,000 pings.
        /// </summary>
        public int AsyncCallsMaxPings
        {
            get; set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected ApiClient()
        {
            // Up to 8 concurrent connections by default
            System.Net.ServicePointManager.DefaultConnectionLimit = 8;

            // Async calls ping internal
            AsyncCallsPingInterval = 3;

            // Maximum number of pings for async calls
            AsyncCallsMaxPings = 1000;
        }

#if NET_STANDARD_20
        static ApiClient()
        {
            staticHttpClient = new HttpClient();
            staticHttpClient.Timeout = TimeSpan.FromMinutes(100); // 100 minutes

            staticHttpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            staticHttpClient.DefaultRequestHeaders.ExpectContinue = true; //.Add("Expect", "100-continue");
            staticHttpClient.DefaultRequestHeaders.Expect.Add(new System.Net.Http.Headers.NameValueWithParametersHeaderValue("100-continue"));
            //staticHttpClient.DefaultRequestHeaders.ConnectionClose = true;

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.Expect100Continue = true;


        }
#endif

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
        /// Set a custom SelectPdf API endpoint for async jobs. Do not use this method unless advised by SelectPdf.
        /// </summary>
        /// <param name="apiAsyncEndpoint">API async jobs endpoint.</param>
        public void setApiAsyncEndpoint(string apiAsyncEndpoint)
        {
            this.apiAsyncEndpoint = apiAsyncEndpoint;
        }

        /// <summary>
        /// Set a custom SelectPdf API endpoint for web elements. Do not use this method unless advised by SelectPdf.
        /// </summary>
        /// <param name="apiWebElementsEndpoint">API web elements endpoint.</param>
        public void setApiWebElementsEndpoint(string apiWebElementsEndpoint)
        {
            this.apiWebElementsEndpoint = apiWebElementsEndpoint;
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
            // reset results
            numberOfPages = 0;
            jobId = string.Empty;
            webElements = null;

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

                    // get job id from the header
                    try
                    {
                        if (!string.IsNullOrEmpty(response.Headers["selectpdf-api-jobid"]))
                        {
                            jobId = response.Headers["selectpdf-api-jobid"];
                        }
                    }
                    catch { }

                    // Get the response stream with the content returned by the server
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (outStream != null)
                        {
                            BinaryCopyStream(responseStream, outStream);
                            outStream.Position = 0;
                            response.Close();
                            return null;
                        }

                        using (MemoryStream output = new MemoryStream())
                        {
                            BinaryCopyStream(responseStream, output);
                            response.Close();
                            return output.ToArray();
                        }
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    // request accepted (for asynchronous jobs)

                    // get job id from the header
                    try
                    {
                        if (!string.IsNullOrEmpty(response.Headers["selectpdf-api-jobid"]))
                        {
                            jobId = response.Headers["selectpdf-api-jobid"];
                        }
                    }
                    catch { }

                    response.Close();
                    return null;
                }
                else
                {
                    // error - get error message
                    int statusCode = (int)response.StatusCode;
                    string message = response.StatusDescription;
                    response.Close();

                    throw new ApiException(string.Format("({0}) {1}", statusCode, message));
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

                    int statusCode = (int)response.StatusCode;

                    response.Close();

                    //throw new ApiException(string.Format("({0}) {1} - {2}", response.StatusCode, response.StatusDescription, message), webEx);
                    throw new ApiException(string.Format("({0}) {1}", statusCode, message), webEx);
                }
            }
        }

        /// <summary>
        /// Create a multipart/form-data POST request (that can handle file uploads).
        /// </summary>
        /// <param name="outStream">Output response to this stream, if specified.</param>
        /// <returns>If output stream is not specified, return response as byte array.</returns>
        protected byte[] PerformPostAsMultipartFormData(Stream outStream)
        {
            numberOfPages = 0;
            jobId = string.Empty;
            webElements = null;

            // serialize parameters
            byte[] byteData = EncodeMultipartFormData();

            // create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            if (request == null)
            {
                throw new ApiException("Could not establish a connection with the API endpoint: " + apiEndpoint);
            }

            request.ContentType = "multipart/form-data; boundary=" + MULTIPART_FORM_DATA_BOUNDARY;
            request.ContentLength = byteData.Length;
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
                    }
                    catch { }

                    // get job id from the header
                    try
                    {
                        if (!string.IsNullOrEmpty(response.Headers["selectpdf-api-jobid"]))
                        {
                            jobId = response.Headers["selectpdf-api-jobid"];
                        }
                    }
                    catch { }

                    // Get the response stream with the content returned by the server
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (outStream != null)
                        {
                            BinaryCopyStream(responseStream, outStream);
                            outStream.Position = 0;
                            response.Close();
                            return null;
                        }

                        using (MemoryStream output = new MemoryStream())
                        {
                            BinaryCopyStream(responseStream, output);
                            response.Close();
                            return output.ToArray();
                        }
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    // request accepted (for asynchronous jobs)

                    // get job id from the header
                    try
                    {
                        if (!string.IsNullOrEmpty(response.Headers["selectpdf-api-jobid"]))
                        {
                            jobId = response.Headers["selectpdf-api-jobid"];
                        }
                    }
                    catch { }

                    response.Close();
                    return null;
                }
                else
                {
                    // error - get error message
                    int statusCode = (int)response.StatusCode;
                    string message = response.StatusDescription;
                    response.Close();

                    throw new ApiException(string.Format("({0}) {1}", statusCode, message));
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

                    int statusCode = (int)response.StatusCode;

                    response.Close();

                    //throw new ApiException(string.Format("({0}) {1} - {2}", response.StatusCode, response.StatusDescription, message), webEx);
                    throw new ApiException(string.Format("({0}) {1}", statusCode, message), webEx);
                }
            }
        }

#if NET_STANDARD_20
        /// <summary>
        /// Create a multipart/form-data POST request (that can handle file uploads) using HttpClient.
        /// </summary>
        /// <param name="outStream">Output response to this stream, if specified.</param>
        /// <returns>If output stream is not specified, return response as byte array.</returns>
        protected async Task<byte[]> PerformPostAsMultipartFormDataAsync(Stream outStream)
        {
            numberOfPages = 0;
            jobId = string.Empty;
            webElements = null;

            // serialize parameters
            MultipartFormDataContent multiForm = EncodeMultipartFormDataContent();

            // create request
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(apiEndpoint);
            request.Method = HttpMethod.Post;
            request.Content = multiForm;
            //request.Version = HttpVersion.Version10;

            // add headers to the request
            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerName = header.Key;
                string headerValue = header.Value;
                string encodedHeaderName;
                string encodedHeaderValue;

                HeaderNameValueEncode(headerName, headerValue, out encodedHeaderName, out encodedHeaderValue);
                request.Headers.Add(encodedHeaderName, encodedHeaderValue);
            }

            // GET response (if response code is not 200 OK or 202 Accepted, no Exception is raised when HttpClient is used)
            using (HttpResponseMessage response = await staticHttpClient.SendAsync(request))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    IEnumerable<string> values;

                    // get number of pages from the header
                    if (response.Headers.TryGetValues("selectpdf-api-pages", out values))
                    {
                        numberOfPages = int.Parse(values.First());
                    }

                    // get job id from the header
                    if (response.Headers.TryGetValues("selectpdf-api-jobid", out values))
                    {
                        jobId = values.First();
                    }

                    if (outStream != null)
                    {
                        await response.Content.CopyToAsync(outStream);
                        outStream.Position = 0;
                        return null;
                    }

                    return await response.Content.ReadAsByteArrayAsync();
                }
                else if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    // request accepted (for asynchronous jobs)

                    IEnumerable<string> values;

                    // get job id from the header
                    if (response.Headers.TryGetValues("selectpdf-api-jobid", out values))
                    {
                        jobId = values.First();
                    }

                    return null;
                }
                else
                {
                    // error - get error message
                    int statusCode = (int)response.StatusCode;
                    string message = response.ReasonPhrase;

                    string responseBody = string.Empty;
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                    }
                    catch { }
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        message = responseBody;
                    }

                    throw new ApiException(string.Format("({0}) {1}", statusCode, message));
                }
            }
        }

        private MultipartFormDataContent EncodeMultipartFormDataContent()
        {
            var multiForm = new MultipartFormDataContent();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                multiForm.Add(new StringContent(parameter.Value), parameter.Key);
            }

            // encode files
            foreach (KeyValuePair<string, string> fileDataEntry in files)
            {
                multiForm.Add(new ByteArrayContent(File.ReadAllBytes(fileDataEntry.Value)), fileDataEntry.Key, fileDataEntry.Value);
            }

            // encode additional binary data
            foreach (KeyValuePair<string, byte[]> binaryDataEntry in binaryData)
            {
                multiForm.Add(new ByteArrayContent(binaryDataEntry.Value), binaryDataEntry.Key, binaryDataEntry.Key);
            }

            return multiForm;
        }

        /// <summary>
        /// Start an asynchronous job that requires multipart forma data.
        /// </summary>
        /// <returns>Asynchronous job ID.</returns>
        public string StartAsyncJobMultipartFormDataAsync()
        {
            parameters["async"] = "True";
            PerformPostAsMultipartFormDataAsync(null).Wait();
            return jobId;
        }

#endif

        /// <summary>
        /// Start an asynchronous job.
        /// </summary>
        /// <returns>Asynchronous job ID.</returns>
        public string StartAsyncJob()
        {
            parameters["async"] = "True";
            PerformPost(null);
            return jobId;
        }

        /// <summary>
        /// Start an asynchronous job that requires multipart forma data.
        /// </summary>
        /// <returns>Asynchronous job ID.</returns>
        public string StartAsyncJobMultipartFormData()
        {
            parameters["async"] = "True";
            PerformPostAsMultipartFormData(null);
            return jobId;
        }

        private byte[] EncodeMultipartFormData()
        {
            MemoryStream data = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(data);

            // encode regular parameters
            string sParameters = "";

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                sParameters += "--" + MULTIPART_FORM_DATA_BOUNDARY + NEW_LINE;
                sParameters += string.Format("Content-Disposition: form-data; name=\"{0}\"", parameter.Key) + NEW_LINE;
                sParameters += NEW_LINE;
                sParameters += parameter.Value + NEW_LINE;
            }

            writer.Write(Encoding.UTF8.GetBytes(sParameters));

            // encode files
            foreach (KeyValuePair<string, string> fileDataEntry in files)
            {
                string sFileEncoding = "--" + MULTIPART_FORM_DATA_BOUNDARY + NEW_LINE;
                sFileEncoding += String.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", fileDataEntry.Key, fileDataEntry.Value) + NEW_LINE;
                sFileEncoding += "Content-Type: application/octet-stream" + NEW_LINE;
                sFileEncoding += NEW_LINE;
                writer.Write(Encoding.UTF8.GetBytes(sFileEncoding));

                // file content
                using (FileStream fin = File.OpenRead(fileDataEntry.Value))
                {
                    BinaryCopyStream(fin, writer.BaseStream);
                }
                writer.Write(Encoding.UTF8.GetBytes(NEW_LINE));
            }

            // encode additional binary data
            foreach (KeyValuePair<string, byte[]> binaryDataEntry in binaryData)
            {
                string sFileEncoding = "--" + MULTIPART_FORM_DATA_BOUNDARY + NEW_LINE;
                sFileEncoding += String.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", binaryDataEntry.Key, binaryDataEntry.Key) + NEW_LINE;
                sFileEncoding += "Content-Type: application/octet-stream" + NEW_LINE;
                sFileEncoding += NEW_LINE;
                writer.Write(Encoding.UTF8.GetBytes(sFileEncoding));

                // binary content
                writer.Write(binaryDataEntry.Value);
                writer.Write(Encoding.UTF8.GetBytes(NEW_LINE));
            }

            // final boundary
            sParameters = "--" + MULTIPART_FORM_DATA_BOUNDARY + "--" + NEW_LINE;
            sParameters += NEW_LINE;

            writer.Write(Encoding.UTF8.GetBytes(sParameters));
            writer.Flush();
            return data.ToArray();
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
