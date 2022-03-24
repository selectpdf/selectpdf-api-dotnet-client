using System;
using System.Collections.Generic;
using SelectPdf.Api;

namespace Samples
{
    public class SearchPdf
    {
        public static void RunTest()
        {
            string testUrl = "https://selectpdf.com/demo/files/selectpdf.pdf";
            string testPdf = "Input.pdf";
            string apiKey = "Your API key here";

            Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);

            try
            {
                PdfToTextClient client = new PdfToTextClient(apiKey);

                // set parameters - see full list at https://selectpdf.com/pdf-to-text-api/
                client
                    .setStartPage(1) // start page (processing starts from here)
                    .setEndPage(0) // end page (set 0 to process file til the end)
                    .setOutputFormat(OutputFormat.Text) // set output format (0-Text or 1-HTML)
                ;

                Console.WriteLine("Starting search pdf ...");

                // search local pdf
                IList<TextPosition> results = client.searchFile(testPdf, "pdf");

                // search pdf from public url
                // IList<TextPosition> results = client.searchUrl(testUrl, "pdf");

                Console.WriteLine("Search results:\n{0}\nSearch results count: {1}.", string.Join("\n", results), results.Count);

                Console.WriteLine("Finished! Number of pages processed: {0}.", client.getNumberOfPages());

                // get API usage
                UsageClient usageClient = new UsageClient(apiKey);
                UsageInformation usage = usageClient.getUsage(false);
                Console.WriteLine("Conversions remained this month: {0}.", usage.Available);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

    }
}
