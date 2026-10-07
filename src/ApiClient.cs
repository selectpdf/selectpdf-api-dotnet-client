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
        /// Last HTTP Code
        /// </summary>
        protected int lastHTTPCode = 0;

        /// <summary>
        /// Subscription monthly conversion limit, parsed from X-SelectPdf-Credits-Total.
        /// -1 indicates unlimited (Dedicated tier). Null when the header is absent
        /// (e.g. demo endpoint, error responses, older server versions).
        /// </summary>
        protected int? creditsTotal = null;

        /// <summary>
        /// Conversions remaining in the current month, parsed from
        /// X-SelectPdf-Credits-Remaining. -1 for unlimited subscriptions.
        /// Null when the header is absent.
        /// </summary>
        protected int? creditsRemaining = null;

        /// <summary>
        /// Subscription monthly conversion limit reported by the server.
        /// -1 = unlimited (Dedicated tier). Null = the most recent response did
        /// not include credit info (e.g. demo endpoint, error response).
        /// </summary>
        public int? CreditsTotal { get { return creditsTotal; } }

        /// <summary>
        /// Conversions remaining in the current month reported by the server.
        /// -1 = unlimited (Dedicated tier). Null = the most recent response did
        /// not include credit info.
        /// </summary>
        public int? CreditsRemaining { get { return creditsRemaining; } }

        /// <summary>
        /// Endpoint mode of the most recent response, parsed from X-SelectPdf-Mode.
        /// "production" or "demo". Empty string when the header is absent (older server).
        /// </summary>
        protected string mode = string.Empty;

        /// <summary>
        /// Server-side execution path of the most recent response, parsed from
        /// X-SelectPdf-Execution. "in-process" or "worker". Empty string when absent.
        /// </summary>
        protected string executionMode = string.Empty;

        /// <summary>
        /// Endpoint mode of the most recent response: "production" or "demo".
        /// Empty when the response did not include the X-SelectPdf-Mode header
        /// (older server, or non-conversion endpoint).
        /// </summary>
        public string Mode { get { return mode; } }

        /// <summary>
        /// Server-side execution path of the most recent conversion: "in-process"
        /// (engine ran inside the IIS worker process) or "worker" (engine ran in
        /// a separate SelectPdf.Worker.Local.exe / Worker.Remote.exe process).
        /// Empty for endpoints that don't perform a conversion (e.g. Usage, WebElements).
        /// </summary>
        public string ExecutionMode { get { return executionMode; } }

        /// <summary>
        /// Web elements locations. This is retrieved if pdf_web_elements_selectors parameter is set and elements were found to match the selectors.
        /// </summary>
        protected IList<WebElement> webElements = null;

        private const string MULTIPART_FORM_DATA_BOUNDARY = "------------SelectPdf_Api_Boundry_$";
        private const string NEW_LINE = "\r\n";

        /// <summary>
        /// Library version
        /// </summary>
        public static string CLIENT_VERSION = "1.6.0";

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
        /// Hook called after a successful response, with raw response headers.
        /// Subclasses can override to capture endpoint-specific headers (e.g. demo headers).
        /// Default implementation is a no-op.
        /// Used on the WebRequest code path (legacy and most TFMs).
        /// </summary>
        /// <param name="headers">Raw HTTP response headers.</param>
        protected virtual void OnResponseHeadersReceived(WebHeaderCollection headers)
        {
        }

#if NET_STANDARD_20
        /// <summary>
        /// HttpClient variant of <see cref="OnResponseHeadersReceived(WebHeaderCollection)"/>.
        /// Both the response message headers and the content headers are passed
        /// in because HttpClient splits them across two collections (and the
        /// SelectPdf demo headers land on the content-headers side). Subclasses
        /// that need demo-style headers should override this when targeting
        /// .NET Standard 2.0 / .NET Core in addition to the WebRequest hook.
        /// Default implementation is a no-op.
        /// </summary>
        protected virtual void OnResponseHeadersReceivedHttpClient(
            System.Net.Http.Headers.HttpResponseHeaders headers,
            System.Net.Http.Headers.HttpContentHeaders contentHeaders)
        {
        }
#endif

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
        public string SerializeDictionary(Dictionary<string, string> dictionaryToSerialize)
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
            string platform = Environment.Version.ToString();
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            headers["selectpdf-api-client"] = string.Format("net-{0}-{1}", platform, version);

            // reset results
            numberOfPages = 0;
            jobId = string.Empty;
            webElements = null;
            lastHTTPCode = 0;
            creditsTotal = null;
            creditsRemaining = null;
            mode = string.Empty;
            executionMode = string.Empty;

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
                int statusCode = (int)response.StatusCode;
                lastHTTPCode = statusCode;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ReadStandardResponseHeaders(response.Headers);

                    try { OnResponseHeadersReceived(response.Headers); } catch { }

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
                    ReadStandardResponseHeaders(response.Headers);
                    try { OnResponseHeadersReceived(response.Headers); } catch { }

                    response.Close();
                    return null;
                }
                else
                {
                    // error - get error message
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
                    lastHTTPCode = statusCode;

                    string contentType = response.ContentType ?? string.Empty;
                    var responseHeaders = response.Headers;

                    response.Close();

                    // Demo endpoint returns JSON error bodies for 400 / 413 / 429 / 503.
                    // Parse those into typed exceptions so callers can react programmatically.
                    if (contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ApiException demoEx = TryBuildDemoException(statusCode, message, responseHeaders, webEx);
                        if (demoEx != null) throw demoEx;
                    }

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
            string platform = Environment.Version.ToString();
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            headers["selectpdf-api-client"] = string.Format("net-{0}-{1}", platform, version);

            // reset results
            numberOfPages = 0;
            jobId = string.Empty;
            webElements = null;
            lastHTTPCode = 0;
            creditsTotal = null;
            creditsRemaining = null;
            mode = string.Empty;
            executionMode = string.Empty;

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
                int statusCode = (int)response.StatusCode;
                lastHTTPCode = statusCode;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ReadStandardResponseHeaders(response.Headers);
                    try { OnResponseHeadersReceived(response.Headers); } catch { }

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
                    ReadStandardResponseHeaders(response.Headers);
                    try { OnResponseHeadersReceived(response.Headers); } catch { }

                    response.Close();
                    return null;
                }
                else
                {
                    // error - get error message
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
                    lastHTTPCode = statusCode;

                    string contentType = response.ContentType ?? string.Empty;
                    var responseHeaders = response.Headers;

                    response.Close();

                    // Demo endpoint returns JSON error bodies for 400 / 413 / 429 / 503.
                    // Parse those into typed exceptions so callers can react programmatically.
                    if (contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ApiException demoEx = TryBuildDemoException(statusCode, message, responseHeaders, webEx);
                        if (demoEx != null) throw demoEx;
                    }

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
                    var contentHeaders = response.Content != null ? response.Content.Headers : null;
                    ReadStandardResponseHeadersHttpClient(response.Headers, contentHeaders);
                    try { OnResponseHeadersReceivedHttpClient(response.Headers, contentHeaders); } catch { }

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
                    var contentHeaders = response.Content != null ? response.Content.Headers : null;
                    ReadStandardResponseHeadersHttpClient(response.Headers, contentHeaders);
                    try { OnResponseHeadersReceivedHttpClient(response.Headers, contentHeaders); } catch { }
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

                    string contentType = response.Content.Headers.ContentType != null
                        ? response.Content.Headers.ContentType.MediaType
                        : string.Empty;
                    if (contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ApiException demoEx = TryBuildDemoExceptionFromHttpClient(statusCode, message, response.Headers, response.Content.Headers);
                        if (demoEx != null) throw demoEx;
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
        /// Start an asynchronous job that requires multipart form data.
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

        // --- standard response header readers ---------------------------------------
        // The SDK reads only the new X-SelectPdf-* header form. Older clients that
        // depend on the legacy lowercase form continue to work because the server
        // still emits both, but new SDK code should never read the legacy form.

        private void ReadStandardResponseHeaders(System.Net.WebHeaderCollection headers)
        {
            if (headers == null) return;

            try
            {
                string pages = headers["X-SelectPdf-Pages"];
                if (!string.IsNullOrEmpty(pages))
                    numberOfPages = Convert.ToInt32(pages);
            }
            catch { }

            try
            {
                string jid = headers["X-SelectPdf-Job-Id"];
                if (!string.IsNullOrEmpty(jid)) jobId = jid;
            }
            catch { }

            try
            {
                string total = headers["X-SelectPdf-Credits-Total"];
                int v;
                if (!string.IsNullOrEmpty(total) && int.TryParse(total, out v)) creditsTotal = v;
            }
            catch { }

            try
            {
                string remaining = headers["X-SelectPdf-Credits-Remaining"];
                int v;
                if (!string.IsNullOrEmpty(remaining) && int.TryParse(remaining, out v)) creditsRemaining = v;
            }
            catch { }

            try
            {
                string m = headers["X-SelectPdf-Mode"];
                if (!string.IsNullOrEmpty(m)) mode = m;
            }
            catch { }

            try
            {
                string e = headers["X-SelectPdf-Execution"];
                if (!string.IsNullOrEmpty(e)) executionMode = e;
            }
            catch { }
        }

#if NET_STANDARD_20
        // The SelectPdf server adds all X-SelectPdf-* headers to response.Content.Headers
        // (see SelectPdf/AppCode/ApiResponseHeaders.cs). HttpClient exposes these on a
        // separate HttpContentHeaders collection, distinct from response.Headers — so
        // both must be checked. Earlier revisions of this method only consulted
        // response.Headers and missed every standard header on the HttpClient path.
        private void ReadStandardResponseHeadersHttpClient(
            System.Net.Http.Headers.HttpResponseHeaders headers,
            System.Net.Http.Headers.HttpContentHeaders contentHeaders)
        {
            string s;

            s = TryGetHeader(headers, contentHeaders, "X-SelectPdf-Pages");
            int pages;
            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out pages)) numberOfPages = pages;

            s = TryGetHeader(headers, contentHeaders, "X-SelectPdf-Job-Id");
            if (!string.IsNullOrEmpty(s)) jobId = s;

            s = TryGetHeader(headers, contentHeaders, "X-SelectPdf-Credits-Total");
            int total;
            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out total)) creditsTotal = total;

            s = TryGetHeader(headers, contentHeaders, "X-SelectPdf-Credits-Remaining");
            int remaining;
            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out remaining)) creditsRemaining = remaining;

            s = TryGetHeader(headers, contentHeaders, "X-SelectPdf-Mode");
            if (!string.IsNullOrEmpty(s)) mode = s;

            s = TryGetHeader(headers, contentHeaders, "X-SelectPdf-Execution");
            if (!string.IsNullOrEmpty(s)) executionMode = s;
        }

        private static string TryGetHeader(
            System.Net.Http.Headers.HttpResponseHeaders headers,
            System.Net.Http.Headers.HttpContentHeaders contentHeaders,
            string name)
        {
            IEnumerable<string> values;
            if (contentHeaders != null && contentHeaders.TryGetValues(name, out values))
                return values.First();
            if (headers != null && headers.TryGetValues(name, out values))
                return values.First();
            return null;
        }
#endif

        // --- demo endpoint JSON-error helpers ---------------------------------------
        // The demo endpoint returns structured JSON for 400 / 413 / 429 / 503 errors:
        //   {"error":"rate_limited","reason":"per_ip","upgrade":"..."}
        //   {"error":"unsafe_url","field":"url","reason":"private_ip"}
        //   {"error":"unsupported_in_demo","field":"user_password","upgrade":"..."}
        //   {"error":"body_too_large","max_bytes":1048576,"upgrade":"..."}
        // We do a hand-rolled parse to avoid pulling in Json.NET as a hard dependency.

        internal static ApiException TryBuildDemoException(int statusCode, string body, WebHeaderCollection headers, Exception inner)
        {
            int retryAfter = 0;
            if (headers != null)
            {
                string ra = headers["Retry-After"];
                if (!string.IsNullOrEmpty(ra)) int.TryParse(ra, out retryAfter);
            }
            return TryBuildDemoExceptionCore(statusCode, body, retryAfter, inner);
        }

#if NET_STANDARD_20
        internal static ApiException TryBuildDemoExceptionFromHttpClient(int statusCode, string body, System.Net.Http.Headers.HttpResponseHeaders responseHeaders, System.Net.Http.Headers.HttpContentHeaders contentHeaders)
        {
            int retryAfter = 0;
            try
            {
                if (responseHeaders != null && responseHeaders.RetryAfter != null && responseHeaders.RetryAfter.Delta.HasValue)
                {
                    retryAfter = (int)responseHeaders.RetryAfter.Delta.Value.TotalSeconds;
                }
            }
            catch { }
            return TryBuildDemoExceptionCore(statusCode, body, retryAfter, null);
        }
#endif

        private static ApiException TryBuildDemoExceptionCore(int statusCode, string body, int retryAfter, Exception inner)
        {
            if (string.IsNullOrEmpty(body)) return null;
            string error  = ExtractJsonField(body, "error");
            if (string.IsNullOrEmpty(error)) return null;
            string reason = ExtractJsonField(body, "reason");
            string field  = ExtractJsonField(body, "field");
            string upgrade = ExtractJsonField(body, "upgrade");

            switch (error)
            {
                case "rate_limited":
                    return new DemoRateLimitException(statusCode, reason, retryAfter, upgrade, body, inner);
                case "unsafe_url":
                    return new DemoSafetyException(statusCode, field, reason, body, inner);
                case "unsupported_in_demo":
                    return new DemoUnsupportedException(statusCode, field, upgrade, body, inner);
                case "body_too_large":
                    return new ApiException(string.Format("({0}) Demo request body exceeds the demo cap. Upgrade at {1}.", statusCode, upgrade ?? "https://selectpdf.com/pricing/"), inner);
                default:
                    return null;
            }
        }

        private static string ExtractJsonField(string json, string field)
        {
            // Hand-rolled extraction of a top-level string field from a flat JSON object.
            // Matches "field":"value" with arbitrary whitespace and basic backslash-escape handling.
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(field)) return null;
            string needle = "\"" + field + "\"";
            int idx = json.IndexOf(needle, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx + needle.Length);
            if (idx < 0) return null;
            // skip whitespace
            while (idx + 1 < json.Length && char.IsWhiteSpace(json[idx + 1])) idx++;
            if (idx + 1 >= json.Length) return null;
            char first = json[idx + 1];
            if (first == '"')
            {
                int start = idx + 2;
                var sb = new StringBuilder();
                int i = start;
                while (i < json.Length)
                {
                    char c = json[i];
                    if (c == '\\' && i + 1 < json.Length) { sb.Append(json[i + 1]); i += 2; continue; }
                    if (c == '"') return sb.ToString();
                    sb.Append(c); i++;
                }
                return sb.ToString();
            }
            // numeric / bool / null — read until comma / brace
            int e = idx + 1;
            while (e < json.Length && json[e] != ',' && json[e] != '}' && json[e] != '\n' && json[e] != '\r') e++;
            return json.Substring(idx + 1, e - idx - 1).Trim();
        }
    }
}
