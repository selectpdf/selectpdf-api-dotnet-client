using System;
using SelectPdf.Api;

namespace Samples
{
    public class PdfToText
    {
        public static void RunTest()
        {
            string testUrl = "https://selectpdf.com/demo/files/selectpdf.pdf";
            string testPdf = "Input.pdf";
            string localFile = "Result.txt";
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

                Console.WriteLine("Starting pdf to text ...");

                // convert local pdf to local text file
                client.getTextFromFileToFile(testPdf, localFile);

                // extract text from local pdf to memory
                // string text = client.getTextFromFile(testPdf);
                // print text
                // Console.WriteLine(text);

                // convert pdf from public url to local text file
                // client.getTextFromUrlToFile(testUrl, localFile);

                // extract text from pdf from public url to memory
                // string text = client.getTextFromUrl(testUrl);
                // print text
                // Console.WriteLine(text);

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
