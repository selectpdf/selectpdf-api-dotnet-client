Imports System

Module Program
    Sub Main(args As String())
        Console.WriteLine("Testing SelectPdf online REST API Client for .NET...")

        Dim sample As String = If(args IsNot Nothing AndAlso args.Length > 0, args(0).ToLowerInvariant(), "htmltopdf")

        Select Case sample
            Case "htmltopdf"
                HtmlToPdfMain.RunTest()
            Case "headersfooters"
                HtmlToPdfHeadersAndFooters.RunTest()
            Case "pdfmerge"
                PdfMerge.RunTest()
            Case "pdftotext"
                PdfToText.RunTest()
            Case "searchpdf"
                SearchPdf.RunTest()
            Case "webelements"
                WebElements.RunTest()
            Case "async"
                AsyncConversion.RunTest()
            Case "accessible"
                AccessiblePdf.RunTest()
            Case "invoice"
                ElectronicInvoice.RunTest()
            Case Else
                Console.WriteLine("Unknown sample: {0}", sample)
                Console.WriteLine("Available: htmltopdf, headersfooters, pdfmerge, pdftotext, searchpdf, webelements, async, accessible, invoice")
        End Select

    End Sub
End Module
