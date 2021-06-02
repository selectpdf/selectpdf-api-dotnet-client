using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SelectPdf.Api
{
    /// <summary>
    /// Get the result of an asynchronous call.
    /// </summary>
    public class AsyncJobClient : ApiClient
    {
        /// <summary>
        /// Construct the sync job client.
        /// </summary>
        /// <param name="apiKey">API Key.</param>
        /// <param name="jobId">Job ID.</param>
        public AsyncJobClient(string apiKey, string jobId)
        {
            apiEndpoint = "https://selectpdf.com/api2/asyncjob/";
            parameters["key"] = apiKey;
            parameters["job_id"] = jobId;
        }

        /// <summary>
        /// Get result of the asynchronous job.
        /// </summary>
        /// <returns>Byte array containing the resulted file if the job is finished. Returns Null if the job is still running. Throws an exception if an error occurred.</returns>
        public byte[] getResult()
        {
            byte[] result = PerformPost(null);

            if (!string.IsNullOrEmpty(jobId))
            {
                // job is still running
                //Console.WriteLine("Job is still running! Job ID: " + jobId);
                return null;
            }
            else
            {
                //Console.WriteLine("Job finished!");
                return result;
            }
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
            return webElements;
        }

    }
}
