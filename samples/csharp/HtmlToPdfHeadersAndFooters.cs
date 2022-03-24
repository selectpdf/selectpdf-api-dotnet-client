using System;
using SelectPdf.Api;

namespace Samples
{
    public class HtmlToPdfHeadersAndFooters
    {
        public static void RunTest()
        {
            string url = "https://selectpdf.com";
            string localFile = "Test.pdf";
            string apiKey = "Your API key here";

            Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);

            try
            {
                HtmlToPdfClient client = new HtmlToPdfClient(apiKey);

                // set parameters - see full list at https://selectpdf.com/html-to-pdf-api/
                client
                    .setMargins(0) // PDF page margins
                    .setPageBreaksEnhancedAlgorithm(true) // enhanced page break algorithm

                    // header properties
                    .setShowHeader(true) // display header
                    // .setHeaderHeight(50) // header height
                    // .setHeaderUrl(url) // header url
                    .setHeaderHtml("This is the <b>HEADER</b>!!!!") // header html

                    // footer properties
                    .setShowFooter(true) // display footer
                    // .setFooterHeight(60) // footer height
                    // .setFooterUrl(url) // footer url
                    .setFooterHtml("This is the <b>FOOTER</b>!!!!") // footer html

                    // footer page numbers
                    .setShowPageNumbers(true) // show page numbers in footer
                    .setPageNumbersTemplate("{page_number} / {total_pages}") // page numbers template
                    .setPageNumbersFontName("Verdana") // page numbers font name
                    .setPageNumbersFontSize(12) // page numbers font size
                    .setPageNumbersAlignment(PageNumbersAlignment.Center) // page numbers alignment (2-Center)
                ;

                Console.WriteLine("Starting conversion ...");

                // convert url to file
                client.convertUrlToFile(url, localFile);

                // convert url to memory
                // byte[] pdf = client.convertUrl(url);

                // convert html string to file
                // client.convertHtmlStringToFile("This is some <b>html</b>.", localFile);

                // convert html string to memory
                // byte[] pdf = client.convertHtmlString("This is some <b>html</b>.");

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
