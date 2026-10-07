using System;
using SelectPdf.Api;

namespace Samples
{
    /// <summary>
    /// ZUGFeRD / Factur-X hybrid electronic invoice.
    /// </summary>
    public class ElectronicInvoice
    {
        // A minimal, well-formed EN 16931 CrossIndustryInvoice, inline so the
        // sample runs without any extra files. A real integration produces this
        // from its own invoice data — the API embeds the bytes as given and does
        // not validate the invoice content.
        private const string InvoiceXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<rsm:CrossIndustryInvoice" +
            " xmlns:rsm=\"urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100\"" +
            " xmlns:ram=\"urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100\"" +
            " xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100\">" +
              "<rsm:ExchangedDocumentContext>" +
                "<ram:GuidelineSpecifiedDocumentContextParameter>" +
                  "<ram:ID>urn:cen.eu:en16931:2017</ram:ID>" +
                "</ram:GuidelineSpecifiedDocumentContextParameter>" +
              "</rsm:ExchangedDocumentContext>" +
              "<rsm:ExchangedDocument>" +
                "<ram:ID>INV-2026-001</ram:ID>" +
                "<ram:TypeCode>380</ram:TypeCode>" +
                "<ram:IssueDateTime>" +
                  "<udt:DateTimeString format=\"102\">20260924</udt:DateTimeString>" +
                "</ram:IssueDateTime>" +
              "</rsm:ExchangedDocument>" +
            "</rsm:CrossIndustryInvoice>";

        private const string InvoiceHtml =
            "<html><head><title>Invoice INV-2026-001</title></head><body>" +
            "<h1>Invoice INV-2026-001</h1>" +
            "<p>Seller Ltd &#8212; Buyer GmbH</p>" +
            "<table>" +
              "<tr><th>Item</th><th>Total</th></tr>" +
              "<tr><td>Consulting</td><td>1000.00 EUR</td></tr>" +
            "</table>" +
            "</body></html>";

        public static void RunTest()
        {
            string localFile = "Invoice.pdf";

            // Electronic invoices require an API key — the keyless demo endpoint
            // does not produce them, and InvoiceClient refuses a demo key rather
            // than failing later on the server.
            string apiKey = "Your API key here";

            Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);

            try
            {
                InvoiceClient client = new InvoiceClient(apiKey);

                // set parameters - see full list at https://selectpdf.com/html-to-pdf-api-parameters/#invoicing
                client
                    // The invoice XML. From memory here; from disk with
                    //   client.setInvoiceXmlFile("factur-x.xml");
                    // Either way the name recorded inside the PDF is the one the
                    // standard prescribes — recipients look it up by name, so it is
                    // not taken from your file name.
                    .setInvoiceXml(InvoiceXml)

                    // How much of the EN 16931 model the XML carries. Required.
                    //   Minimum / Basic_WL - not complete invoices
                    //   Basic / En16931 / Extended - complete, increasing detail
                    //   XRechnung - German public sector; the embedded file is then
                    //               named xrechnung.xml instead of factur-x.xml
                    .setZugferdProfile(ZugferdProfile.En16931)
                ;

                // Optional. Left unset, the API derives the relationship from the
                // profile: Alternative for Minimum and Basic_WL, where the visible
                // page carries more than the XML, and Data for the rest, where both
                // carry the same content — which Germany mandates for BASIC,
                // EN 16931, EXTENDED and XRECHNUNG. Minimum or Basic_WL combined
                // with Data is rejected, because it would not be true.
                //
                //     client.setZugferdRelationship(ZugferdRelationship.Data);

                // Optional. Factur-X 1.0 is the current schema and the default;
                // ZUGFeRD 2.0 is deprecated and only for recipients that require it.
                //
                //     client.setZugferdSchema(ZugferdSchema.Zugferd20);

                // The carrier must be PDF/A-3. It defaults to PdfA3A, the accessible
                // level, which the standards recommend because it also makes the
                // visible invoice readable by assistive technology. PdfA3B and
                // PdfA3U are accepted; anything else is rejected.
                //
                //     client.setPdfStandard(PdfStandard.PdfA3B);

                // Every ordinary conversion setting applies here as well, because
                // InvoiceClient derives from HtmlToPdfClient.
                client.setMargins(20);

                Console.WriteLine("Starting invoice conversion ...");

                // create the hybrid invoice from raw html, into a local file
                client.createFromHtmlStringToFile(InvoiceHtml, localFile);

                // ... or from the invoice page your application already renders
                // client.createFromUrlToFile("https://your-app.example/invoices/INV-2026-001", localFile);

                // ... or into memory
                // byte[] pdf = client.createFromHtmlString(InvoiceHtml);

                // ... or asynchronously, for long invoice runs
                // client.createFromHtmlStringToFileAsync(InvoiceHtml, localFile);

                Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages());
                Console.WriteLine("Wrote {0} — a PDF/A-3 document with factur-x.xml embedded.", localFile);

                // response telemetry
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
