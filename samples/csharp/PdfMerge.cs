using System;
using SelectPdf.Api;

namespace Samples
{
    public class PdfMerge
    {
        public static void RunTest()
        {
            string testUrl = "https://selectpdf.com/demo/files/selectpdf.pdf";
            string testPdf = "Input.pdf";
            string localFile = "Result.pdf";
            string apiKey = "Your API key here";

            Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);

            try
            {
                PdfMergeClient client = new PdfMergeClient(apiKey);

                // set parameters - see full list at https://selectpdf.com/pdf-merge-api/
                client
                    // specify the pdf files that will be merged (order will be preserved in the final pdf)

                    .addFile(testPdf) // add PDF from local file
                    .addUrlFile(testUrl) // add PDF From public url
                    // .addFile(testPdf, "pdf_password") // add PDF (that requires a password) from local file
                    // .addUrlFile(testUrl, "pdf_password") // add PDF (that requires a password) from public url
                ;

                Console.WriteLine("Starting pdf merge ...");

                // merge pdfs to local file
                client.saveToFile(localFile);

                // merge pdfs to memory
                // byte[] pdf = client.save();

                Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages());

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
