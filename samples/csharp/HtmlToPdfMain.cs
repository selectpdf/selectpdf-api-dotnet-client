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

            // Pass null / empty / "demo" to use the keyless demo endpoint
            // (output is watermarked, capped at 5 pages, Chromium engine only).
            // Replace with a real key for full production output.
            string apiKey = null;

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
                    .setRenderingEngine(RenderingEngine.WebKit) // rendering engine (demo mode forces Chromium)
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
                    // .setUserPassword("password") // secure the PDF with a password (paid keys only)

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

                // response telemetry (SDK 1.5.0)
                Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode);
                if (client.IsDemoMode)
                {
                    if (client.WasClamped)
                        Console.WriteLine("Demo clamped: {0}.", string.Join(", ", client.ClampedFields));
                    if (client.WasAnyFieldDropped)
                        Console.WriteLine("Demo dropped: {0}.", string.Join(", ", client.DroppedFields));
                }
                else
                {
                    Console.WriteLine("Credits remaining: {0} / {1}.",
                        client.CreditsRemaining, client.CreditsTotal);

                    // get API usage (paid keys only — demo endpoint has no usage account)
                    UsageClient usageClient = new UsageClient(apiKey);
                    UsageInformation usage = usageClient.getUsage(false);
                    Console.WriteLine("Conversions remained this month: {0}.", usage.Available);
                }
            }
            catch (DemoRateLimitException ex)
            {
                // reason is one of: per_ip, daily_cap, concurrency
                Console.WriteLine("Demo rate limit ({0}). Retry after {1}s. Upgrade: {2}",
                    ex.Reason, ex.RetryAfter, ex.UpgradeUrl);
            }
            catch (DemoSafetyException ex)
            {
                // demo only converts public URLs — internal/private hosts are rejected
                Console.WriteLine("Demo safety guard rejected '{0}' (reason={1}).", ex.Field, ex.Reason);
            }
            catch (DemoUnsupportedException ex)
            {
                // feature not available on the demo endpoint (e.g. setUserPassword)
                Console.WriteLine("Feature '{0}' not available in demo mode. Upgrade: {1}", ex.Field, ex.UpgradeUrl);
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
