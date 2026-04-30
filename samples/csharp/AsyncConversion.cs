using System;
using SelectPdf.Api;

namespace Samples
{
    public class AsyncConversion
    {
        public static void RunTest()
        {
            string url = "https://selectpdf.com";
            string localFile = "Test.pdf";

            // Async conversions are not supported on the demo endpoint —
            // this sample requires a paid API key.
            string apiKey = "Your API key here";

            Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);

            try
            {
                HtmlToPdfClient client = new HtmlToPdfClient(apiKey);

                // Tune polling for the async job (optional).
                // The client polls /api2/asyncjob/ every AsyncCallsPingInterval
                // seconds, up to AsyncCallsMaxPings times, then gives up.
                client.AsyncCallsPingInterval = 3; // seconds between polls
                client.AsyncCallsMaxPings = 1000;  // max polls before timeout

                client
                    .setPageSize(PageSize.A4)
                    .setPageOrientation(PageOrientation.Portrait)
                    .setMargins(0)
                    .setPageBreaksEnhancedAlgorithm(true)
                ;

                Console.WriteLine("Starting async conversion ...");

                // url to file (async)
                client.convertUrlToFileAsync(url, localFile);

                // url to memory (async)
                // byte[] pdf = client.convertUrlAsync(url);

                // html string to file (async)
                // client.convertHtmlStringToFileAsync("This is some <b>html</b>.", localFile);

                // html string to memory (async)
                // byte[] pdf = client.convertHtmlStringAsync("This is some <b>html</b>.");

                Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages());

                // response telemetry (SDK 1.5.0)
                Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode);
                Console.WriteLine("Credits remaining: {0} / {1}.",
                    client.CreditsRemaining, client.CreditsTotal);
            }
            catch (ApiException ex)
            {
                Console.WriteLine("API error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

    }
}
