# SelectPdf Online REST API - .NET Client

SelectPdf Online REST API is a professional solution for managing PDF documents online. It now has a dedicated, easy to use, .NET client library that can be setup in minutes.

## Installation

Download [selectpdf-api-dotnet-client-1.4.0.zip](https://github.com/selectpdf/selectpdf-api-dotnet-client/releases/download/1.4.0/selectpdf-api-dotnet-client-1.4.0.zip), unzip it and add a reference to SelectPdf.Api.dll to your project.

OR

Install SelectPdf .NET Client for Online API via Nuget: [SelectPdf API on Nuget](https://www.nuget.org/packages/SelectPdf.Api/).

```
Install-Package SelectPdf.Api -Version 1.4.0
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

