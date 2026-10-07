# SelectPdf Online REST API - .NET Client

SelectPdf Online REST API is a professional solution for managing PDF documents online. It now has a dedicated, easy to use, .NET client library that can be setup in minutes.

## Try without an API key (demo mode)

```csharp
using System;
using System.IO;
using SelectPdf.Api;

class Program {
    static void Main() {
        // No API key -> keyless demo mode (5-page cap, watermarked output, Chromium engine).
        HtmlToPdfClient client = new HtmlToPdfClient(); // also: new HtmlToPdfClient("demo")
        byte[] pdf = client.convertUrl("https://example.com");
        File.WriteAllBytes("demo.pdf", pdf);
        Console.WriteLine("Pages: {0}", client.getNumberOfPages());
        if (client.WasClamped)
            Console.WriteLine("Server clamped: {0}", string.Join(",", client.ClampedFields));
        // Upgrade for unwatermarked, unlimited-page PDFs: https://selectpdf.com/pricing/
    }
}
```

### Demo mode limitations

Demo mode is intended for evaluating the API. To keep the public endpoint stable and prevent abuse, several limits and restrictions apply that don't exist on a paid endpoint.

**Output:**

- **5 pages maximum.** Pages beyond the fifth are dropped, then a final branding/notice page is appended (so a typical demo PDF is 1–6 pages depending on the source).
- **Watermarked.** A diagonal SelectPdf demo stamp is rendered on every page plus a footer attribution line. The watermark is added by the rendering engine itself — there is no client- or server-side post-processing that can be disabled.
- **Chromium rendering engine only.** If you call `setRenderingEngine(...)` with `WebKit`, `Restricted`, or `Blink`, the server silently force-clamps to `Chromium`. The response carries `WasClamped == true` and `ClampedFields` will include `engine`.

**Parameters not allowed:**

| Parameter / setter | Behavior in demo mode |
| --- | --- |
| `setUserPassword(...)` | Throws `DemoUnsupportedException` client-side (before the request leaves the SDK). |
| `setOwnerPassword(...)` | Same — throws `DemoUnsupportedException`. |
| `async = true` | Not supported. The demo endpoint is synchronous only; there is no `/api2/asyncjob/` polling for demo conversions. |

The two clamped time parameters are accepted but capped:

| Parameter | Production cap | Demo cap |
| --- | --- | --- |
| `setNavigationTimeout(...)` (`max_load_time`) | up to 120 s | clamped to **15 s** |
| `setConversionDelay(...)` (`min_load_time`) | up to 120 s | clamped to **5 s** |

When clamping happens, the response sets `WasClamped = true` and lists the affected field names in `ClampedFields`.

**Silently dropped fields:**

A few request fields are accepted but never honored by the demo endpoint — they're zeroed out server-side because they would compromise the demo's safety posture. Any caller-supplied value is reported back via `WasAnyFieldDropped` / `DroppedFields` so you can see what was ignored.

| Field | Reason |
| --- | --- |
| `auth_username` / `auth_password` | Would let demo callers pump credentials at internal HTTP-Basic endpoints. |
| `cookies` (`setCookies(...)`) / `cookies_string` | Session-hijack vector if combined with a target inside our own infra. |
| `raw_parameters` | Escape hatch that bypasses the parameter clamp; must stay dropped. |
| `pdf_name` | Demo always returns inline `demo.pdf`. |
| `async` | Demo is sync-only. |

Distinct from `ClampedFields`: clamped means the value was modified (capped, force-set), dropped means it was thrown away.

**URLs:**

The demo endpoint accepts only publicly reachable `http://` / `https://` URLs. Requests targeting non-public destinations are rejected with `DemoSafetyException` (HTTP 400). Use a paid endpoint if you need to convert content from a private network.

**Rate limits:**

| Layer | Default | Failure |
| --- | --- | --- |
| Per-IP | 30 requests / rolling hour | `DemoRateLimitException` with `Reason="per_ip"`, `RetryAfter≈3600` |
| Daily global ceiling | 50,000 successful conversions / 24 h across all clients | `DemoRateLimitException` with `Reason="daily_cap"` |
| Concurrency | 4 in-flight conversions globally | `DemoRateLimitException` with `Reason="concurrency"`, `RetryAfter≈5` |

`Retry-After` is taken from the response header. Both the per-IP and daily counters include rejected attempts, so spamming after a block doesn't reset anything.

**Request size:**

- **1 MB request body cap.** Larger payloads return HTTP 413 — typically only an issue if you're inlining a very large HTML string.

**Response:**

- No `X-SelectPdf-Credits-Total` / `X-SelectPdf-Credits-Remaining` headers (the demo endpoint is keyless, so there's no billing account to attribute usage to). On the SDK client these surface as `client.CreditsTotal == null` and `client.CreditsRemaining == null`.
- `client.Mode == "demo"` (helper: `client.IsDemoResponse == true`).
- `client.ExecutionMode` is either `"in-process"` or `"worker"` depending on how the server was deployed.

### Catching demo-mode errors

The SDK exposes three typed exceptions for demo-specific failure modes; everything else surfaces as a regular `ApiException` (paid-endpoint errors) or `WebException` (network failures):

```csharp
try
{
    HtmlToPdfClient client = new HtmlToPdfClient(); // demo mode
    byte[] pdf = client.convertUrl("https://example.com");
}
catch (DemoRateLimitException ex)
{
    // Reason: "per_ip" | "daily_cap" | "concurrency"
    Console.WriteLine("Rate limited: {0}, retry after {1}s", ex.Reason, ex.RetryAfter);
    Console.WriteLine("Upgrade: {0}", ex.UpgradeUrl);
}
catch (DemoSafetyException ex)
{
    // The supplied URL was rejected. Use a paid endpoint for non-public targets.
    Console.WriteLine("URL rejected: {0}", ex.Message);
}
catch (DemoUnsupportedException ex)
{
    // Field: which parameter is unavailable in demo mode
    Console.WriteLine("'{0}' not available in demo mode. Upgrade: {1}", ex.Field, ex.UpgradeUrl);
}
```

For unwatermarked production output without these limits, pass a real API key — same client class, same setters:

```csharp
HtmlToPdfClient client = new HtmlToPdfClient("Your API key here");
```

`IsDemoMode` on the client is set at construction time and stable for the client's lifetime, so a real-key client will never silently fall back to demo behavior.

## Installation

Download [selectpdf-api-dotnet-client-1.6.0.zip](https://github.com/selectpdf/selectpdf-api-dotnet-client/releases/download/1.6.0/selectpdf-api-dotnet-client-1.6.0.zip), unzip it and add a reference to SelectPdf.Api.dll to your project.

OR

Install SelectPdf .NET Client for Online API via Nuget: [SelectPdf API on Nuget](https://www.nuget.org/packages/SelectPdf.Api/).

```
Install-Package SelectPdf.Api -Version 1.6.0
```

OR

Clone [selectpdf-api-dotnet-client](https://github.com/selectpdf/selectpdf-api-dotnet-client) from Github and build the library.

```
git clone https://github.com/selectpdf/selectpdf-api-dotnet-client
cd selectpdf-api-dotnet-client
```


## HTML To PDF API - .NET Client

SelectPdf HTML To PDF Online REST API is a professional solution that lets you create PDF from web pages and raw HTML code in your applications. The API is easy to use and the integration takes only a few lines of code.

### Features

* Create PDF from any web page or html string.
* Full html5/css3/javascript support.
* Set PDF options such as page size and orientation, margins, security, web page settings.
* Set PDF viewer options and PDF document information.
* Create custom headers and footers for the pdf document.
* Hide web page elements during the conversion.
* Automatically generate bookmarks during the html to pdf conversion.
* Support for partial page conversion.
* Produce tagged, accessible PDF and target a PDF/A, PDF/X or PDF/SiqQ conformance level.
* Easy integration, no third party libraries needed.
* Works in all programming languages.
* No installation required.

Sign up for for free to get instant API access to SelectPdf [HTML to PDF API](https://selectpdf.com/html-to-pdf-api/).

### Sample Code

```csharp
using System;
using SelectPdf.Api;

namespace SelectPdf.Api.Tests
{
    class Program
    {
        static void Main(string[] args)
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
```

## Accessible PDF and PDF Standards

Produce a tagged, accessible PDF and target a PDF conformance level, using the same `HtmlToPdfClient` you already use for conversions.

### Features

* Tagged PDF with a logical structure tree: headings, paragraphs, lists, tables, figures with alternate text, links and reading order.
* Document language written as the PDF `/Lang` entry and onto the tagged structure elements.
* PDF/A for long term archiving, PDF/X for graphics exchange, PDF/SiqQ for documents that will be digitally signed.
* PDF/A-3A is the accessible archival level and implies a tagged document on its own.

Tagged output requires the Blink or Chromium rendering engine - the WebKit engines cannot build a structure tree. If you do not set an engine, the API promotes the conversion to Chromium and reports the engine it used in the `X-SelectPdf-Engine` response header. Asking for tagged output together with an explicit WebKit engine is rejected rather than silently producing an untagged PDF.

Note this produces tagged PDF/A output; it is not a conformance certification.

### Sample Code - Accessible PDF

```csharp
using System;
using SelectPdf.Api;

HtmlToPdfClient client = new HtmlToPdfClient("Your API key here");
client
    .setTagged(true)
    .setDocTitle("SelectPdf - accessible sample")
    .setViewerDisplayDocTitle(true)
    .setDocumentLanguage("en-US")
    .setPdfStandard(PdfStandard.PdfA3A)
;

client.convertUrlToFile("https://selectpdf.com", "Accessible.pdf");
Console.WriteLine("Pages: {0}.", client.getNumberOfPages());
```

## Electronic Invoicing API - ZUGFeRD / Factur-X

Turn an HTML invoice into a hybrid electronic invoice: one PDF/A-3 file carrying both the invoice a human reads and the XML a recipient's accounting system reads, embedded as an associated file with the metadata invoice software looks for.

### Features

* Profiles MINIMUM, BASIC WL, BASIC, EN 16931, EXTENDED and XRECHNUNG.
* The attachment relationship is derived from the profile when you do not set one.
* The embedded file is named as the standard requires - `factur-x.xml`, or `xrechnung.xml` for the XRECHNUNG profile - because recipients look it up by name.
* Factur-X 1.0 metadata by default; the deprecated ZUGFeRD 2.0 schema is available for recipients that still require it.
* Carrier defaults to PDF/A-3A, the accessible level, so the visible invoice is readable by assistive technology as well as archivable.
* Every HTML to PDF conversion setting applies, because `InvoiceClient` derives from `HtmlToPdfClient`.

The invoice XML can only be embedded into a document created as PDF/A-3, so there is no variant that attaches it to an existing PDF you already have. An API key is required - the keyless demo endpoint does not produce electronic invoices.

### Sample Code - Electronic Invoice

```csharp
using System;
using SelectPdf.Api;

InvoiceClient client = new InvoiceClient("Your API key here");
client
    .setInvoiceXmlFile("factur-x.xml")   // or setInvoiceXml(bytes / string)
    .setZugferdProfile(ZugferdProfile.En16931)
;

client.createFromHtmlStringToFile(invoiceHtml, "Invoice.pdf");
// ... or from the invoice page your application already renders:
// client.createFromUrlToFile("https://your-app.example/invoices/INV-2026-001", "Invoice.pdf");

Console.WriteLine("Pages: {0}.", client.getNumberOfPages());
```

## Pdf Merge API

SelectPdf Pdf Merge REST API is an online solution that lets you merge local or remote PDFs into a final PDF document.

### Features

* Merge local PDF document.
* Merge remote PDF from public url.
* Set PDF viewer options and PDF document information.
* Secure generated PDF with a password.
* Works in all programming languages.

See [PDF Merge API](https://selectpdf.com/pdf-merge-api/) page for full list of parameters.

### Sample Code

```csharp
using System;
using SelectPdf.Api;

namespace SelectPdf.Api.Tests
{
    class Program
    {
        static void Main(string[] args)
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
```

## Pdf To Text API

SelectPdf Pdf To Text REST API is an online solution that lets you extract text from your PDF documents or search your PDF document for certain words.

### Features

* Extract text from PDF.
* Search PDF.
* Specify start and end page for partial file processing.
* Specify output format (plain text or html).
* Use a PDF from an online location (url) or upload a local PDF document.

See [Pdf To Text API](https://selectpdf.com/pdf-to-text-api/) page for full list of parameters.

### Sample Code - Pdf To Text

```csharp
using System;
using SelectPdf.Api;

namespace SelectPdf.Api.Tests
{
    class Program
    {
        static void Main(string[] args)
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
```

### Sample Code - Search Pdf

```csharp
using System;
using SelectPdf.Api;

namespace SelectPdf.Api.Tests
{
    class Program
    {
        static void Main(string[] args)
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
```

