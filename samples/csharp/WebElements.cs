using System;
using System.Collections.Generic;
using SelectPdf.Api;

namespace Samples
{
    public class WebElements
    {
        public static void RunTest()
        {
            string url = "https://selectpdf.com";
            string localFile = "Test.pdf";

            // Web elements lookup requires a paid API key (the demo endpoint
            // does not expose the elements service).
            string apiKey = "Your API key here";

            Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);

            try
            {
                HtmlToPdfClient client = new HtmlToPdfClient(apiKey);

                // CSS selectors used to identify HTML elements whose location in
                // the resulting PDF should be reported back. See the API docs for
                // selector syntax: https://selectpdf.com/html-to-pdf-api/
                client
                    .setPageSize(PageSize.A4)
                    .setMargins(0)
                    .setPdfWebElementsSelectors("H1, H2, *.menu, *#footer")
                ;

                Console.WriteLine("Starting conversion ...");

                client.convertUrlToFile(url, localFile);

                Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages());

                // Retrieve element rectangles. Returns an empty list if no element
                // matched the configured selectors.
                IList<WebElement> elements = client.getWebElements();
                Console.WriteLine("Web elements found: {0}.", elements.Count);

                foreach (WebElement element in elements)
                {
                    Console.WriteLine(" - <{0}> id='{1}' class='{2}' rectangles={3}",
                        element.HtmlElementTagName,
                        element.HtmlElementId,
                        element.HtmlElementCssClassName,
                        element.PdfRectangles == null ? 0 : element.PdfRectangles.Length);
                }

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
