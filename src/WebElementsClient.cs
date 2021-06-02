using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SelectPdf.Api
{
    /// <summary>
    /// Get the locations of certain web elements. This is retrieved if pdf_web_elements_selectors parameter was set during the initial conversion call and elements were found to match the selectors.
    /// </summary>
    public class WebElementsClient : ApiClient
    {
        /// <summary>
        /// Construct the sync job client.
        /// </summary>
        /// <param name="apiKey">API Key.</param>
        /// <param name="jobId">Job ID.</param>
        public WebElementsClient(string apiKey, string jobId)
        {
            apiEndpoint = "https://selectpdf.com/api2/webelements/";
            parameters["key"] = apiKey;
            parameters["job_id"] = jobId;
        }

        /// <summary>
        /// Get the locations of certain web elements. This is retrieved if pdf_web_elements_selectors parameter is set and elements were found to match the selectors.
        /// </summary>
        /// <returns>List of web elements locations.</returns>
        public IList<WebElement> getWebElements()
        {
            headers["Accept"] = "application/json";
            MemoryStream outStream = new MemoryStream();
            PerformPost(outStream);

            try
            {
                // get json from stream
                string webElementsJson = new StreamReader(outStream).ReadToEnd();
                outStream.Close();

                // deserialize json
                IList<WebElement>  webElements = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<WebElement>>(webElementsJson);

                return webElements;
            }
            catch (Exception ex)
            {
                throw new ApiException("Could not get API web elements.", ex.InnerException);
            }
        }
    }
}
