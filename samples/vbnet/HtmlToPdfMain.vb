Imports System
Imports SelectPdf.Api

Public Class HtmlToPdfMain
    Public Shared Sub RunTest()
        Dim url As String = "https://selectpdf.com"
        Dim localFile As String = "Test.pdf"
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As HtmlToPdfClient = New HtmlToPdfClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/html-to-pdf-api/

            ' main properties
            client.setPageSize(PageSize.A4) ' PDF page size
            client.setPageOrientation(PageOrientation.Portrait) ' PDF page orientation
            client.setMargins(0) ' PDF page margins
            client.setRenderingEngine(RenderingEngine.WebKit) ' rendering engine
            client.setConversionDelay(1) ' conversion delay
            client.setNavigationTimeout(30) ' navigation timeout
            client.setShowPageNumbers(False) ' page numbers
            client.setPageBreaksEnhancedAlgorithm(True) ' enhanced page break algorithm

            ' additional properties
            ' client.setUseCssPrint(True) ' enable CSS media print
            ' client.setDisableJavascript(True) ' disable javascript
            ' client.setDisableInternalLinks(True) ' disable internal links
            ' client.setDisableExternalLinks(True) ' disable external links
            ' client.setKeepImagesTogether(True) ' keep images together
            ' client.setScaleImages(True) ' scale images To create smaller pdfs
            ' client.setSinglePagePdf(True) ' generate a Single page PDF
            ' client.setUserPassword("password") ' secure the PDF With a password

            ' generate automatic bookmarks

            ' client.setPdfBookmarksSelectors("H1, H2") ' create outlines (bookmarks) For the specified elements
            ' client.setViewerPageMode(PageMode.UseOutlines) ' display outlines (bookmarks) In viewer

            Console.WriteLine("Starting conversion ...")

            ' convert url to file
            client.convertUrlToFile(url, localFile)

            ' convert url to memory
            ' Dim pdf As Byte() = client.convertUrl(url)

            ' convert html string to file
            ' client.convertHtmlStringToFile("This is some <b>html</b>.", localFile)

            ' convert html string to memory
            ' Dim pdf As Byte() = client.convertHtmlString("This is some <b>html</b>.")

            Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages())

            ' get API usage
            Dim usageClient As UsageClient = New UsageClient(apiKey)
            Dim usage As UsageInformation = usageClient.getUsage(False)
            Console.WriteLine("Conversions remained this month: {0}.", usage.Available)

        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
