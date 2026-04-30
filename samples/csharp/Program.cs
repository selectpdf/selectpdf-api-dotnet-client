using System;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing SelectPdf online REST API Client for .NET...");

            string sample = (args != null && args.Length > 0) ? args[0].ToLowerInvariant() : "htmltopdf";

            switch (sample)
            {
                case "htmltopdf":
                    HtmlToPdfMain.RunTest();
                    break;
                case "headersfooters":
                    HtmlToPdfHeadersAndFooters.RunTest();
                    break;
                case "pdfmerge":
                    PdfMerge.RunTest();
                    break;
                case "pdftotext":
                    PdfToText.RunTest();
                    break;
                case "searchpdf":
                    SearchPdf.RunTest();
                    break;
                case "webelements":
                    WebElements.RunTest();
                    break;
                case "async":
                    AsyncConversion.RunTest();
                    break;
                default:
                    Console.WriteLine("Unknown sample: {0}", sample);
                    Console.WriteLine("Available: htmltopdf, headersfooters, pdfmerge, pdftotext, searchpdf, webelements, async");
                    break;
            }
        }

    }
}
