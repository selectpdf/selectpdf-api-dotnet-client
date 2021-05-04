# HTML To PDF API - .NET Client

SelectPdf HTML To PDF Online REST API is a professional solution that lets you create PDF from web pages and raw HTML code in your applications. The API is easy to use and the integration takes only a few lines of code.

## Features

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

## Installation

Download [selectpdf-api-dotnet-client-1.1.3.zip](https://github.com/selectpdf/selectpdf-api-dotnet-client/releases/download/1.1.3/selectpdf-api-dotnet-client-1.1.3.zip), unzip it and add a reference to SelectPdf.Api.dll to your project.

OR

Install SelectPdf .NET Client for Online API via Nuget: [SelectPdf API on Nuget](https://www.nuget.org/packages/SelectPdf.Api/).

```
Install-Package SelectPdf.Api -Version 1.1.3
```

OR

Clone [selectpdf-api-dotnet-client](https://github.com/selectpdf/selectpdf-api-dotnet-client) from Github and build the library.

```
git clone https://github.com/selectpdf/selectpdf-api-dotnet-client
cd selectpdf-api-dotnet-client
```

## Sample Code

```
using System;
using SelectPdf.Api;

namespace SelectPdf.Api.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            String url = "https://selectpdf.com";
            String outFile = "test.pdf";

            try
            {
                HtmlToPdfClient api = new HtmlToPdfClient("Your key here");
                api
                    .setPageSize(PageSize.A4)
                    .setPageOrientation(PageOrientation.Portrait)
                    .setMargins(0)
                    .setNavigationTimeout(30)
                    .setShowPageNumbers(false)
                    .setPageBreaksEnhancedAlgorithm(true)
                ;

                Console.WriteLine("Starting conversion ...");

                api.convertUrlToFile(url, outFile);

                Console.WriteLine("Conversion finished successfully!");


                UsageClient usage = new UsageClient("Your key here");
                UsageInformation info = usage.getUsage(false);
                Console.WriteLine("Conversions left this month: " + info.Available);

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
```
