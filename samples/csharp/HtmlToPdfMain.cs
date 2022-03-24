using System;
using SelectPdf.Api;

namespace Samples
{
    public class HtmlToPdfMain
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
                    // main properties

                    .setPageSize(PageSize.A4) // PDF page size
                    .setPageOrientation(PageOrientation.Portrait) // PDF page orientation
                    .setMargins(0) // PDF page margins
                    .setRenderingEngine(RenderingEngine.WebKit) // rendering engine
                    .setConversionDelay(1) // conversion delay
                    .setNavigationTimeout(30) // navigation timeout 
                    .setShowPageNumbers(false) // page numbers
                    .setPageBreaksEnhancedAlgorithm(true) // enhanced page break algorithm

                    // additional properties

                    // .setUseCssPrint(true) // enable CSS media print
                    // .setDisableJavascript(true) // disable javascript
                    // .setDisableInternalLinks(true) // disable internal links
                    // .setDisableExternalLinks(true) // disable external links
                    // .setKeepImagesTogether(true) // keep images together
                    // .setScaleImages(true) // scale images to create smaller pdfs
                    // .setSinglePagePdf(true) // generate a single page PDF
                    // .setUserPassword("password") // secure the PDF with a password

                    // generate automatic bookmarks

                    // .setPdfBookmarksSelectors("H1, H2") // create outlines (bookmarks) for the specified elements
                    // .setViewerPageMode(PageMode.UseOutlines) // display outlines (bookmarks) in viewer
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
