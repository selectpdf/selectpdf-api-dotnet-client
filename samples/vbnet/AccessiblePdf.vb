Imports System
Imports SelectPdf.Api

''' <summary>
''' Tagged (accessible) PDF and PDF conformance standards.
''' </summary>
Public Class AccessiblePdf
    Public Shared Sub RunTest()
        Dim url As String = "https://selectpdf.com"
        Dim localFile As String = "Accessible.pdf"

        ' Works with the keyless demo endpoint too - leave the key as Nothing or
        ' pass "demo". Note the demo stamps its output after conversion, so a demo
        ' PDF demonstrates the feature rather than being a conformant artifact;
        ' use a real key for output you intend to ship.
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As HtmlToPdfClient = New HtmlToPdfClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/html-to-pdf-api-parameters/

            ' Produce a tagged PDF: a logical structure tree covering headings,
            ' paragraphs, lists, tables, figures with alternate text, links and
            ' reading order - what a screen reader needs to read the document.
            client.setTagged(True)

            ' A tagged document needs a title. Without this the converter falls
            ' back to the HTML <title>.
            client.setDocTitle("SelectPdf - accessible sample")

            ' Shown by viewers that honour it; accessible PDF expects it on, and
            ' the API turns it on for you whenever tagged output is requested.
            client.setViewerDisplayDocTitle(True)

            ' The document language, written as the PDF /Lang entry and onto the
            ' tagged structure elements.
            client.setDocumentLanguage("en-US")

            ' Conformance target. PdfA3A is the ACCESSIBLE level of PDF/A-3: it
            ' implies a tagged document on its own, and it is the level required
            ' to carry a ZUGFeRD / Factur-X invoice (see ElectronicInvoice).
            '   Full   - the complete PDF feature set (default)
            '   PdfA / PdfA2B / PdfA3B / PdfA3U - long term archiving
            '   PdfA3A - archiving + accessibility
            '   PdfX   - graphics exchange
            '   PdfSiqQ_A / PdfSiqQ_B - digital signatures
            client.setPdfStandard(PdfStandard.PdfA3A)

            ' Tagged output requires the Blink or Chromium engine - the WebKit
            ' engines cannot build a structure tree. You can name one explicitly:
            '
            '     client.setRenderingEngine(RenderingEngine.Chromium)
            '
            ' If you don't, the API promotes the conversion to Chromium for you and
            ' reports the engine it used in the X-SelectPdf-Engine response header.
            ' Asking for tagged output together with an explicit WebKit engine is
            ' rejected with HTTP 400 rather than silently producing an untagged PDF.

            Console.WriteLine("Starting conversion ...")

            ' convert url to local file
            client.convertUrlToFile(url, localFile)

            ' convert url to memory
            ' Dim pdf As Byte() = client.convertUrl(url)

            Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages())

            ' response telemetry
            Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode)
            Console.WriteLine("Credits remaining: {0} / {1}.", client.CreditsRemaining, client.CreditsTotal)

        Catch ex As ApiException
            Console.WriteLine("API error: {0}", ex.Message)
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
