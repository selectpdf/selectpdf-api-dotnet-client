Imports System
Imports SelectPdf.Api
Public Class HtmlToPdfHeadersAndFooters
    Public Shared Sub RunTest()
        Dim url As String = "https://selectpdf.com"
        Dim localFile As String = "Test.pdf"
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As HtmlToPdfClient = New HtmlToPdfClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/html-to-pdf-api/

            client.setMargins(0) ' PDF page margins
            client.setPageBreaksEnhancedAlgorithm(True) ' enhanced page break algorithm

            ' header properties
            client.setShowHeader(True) ' display header
            ' client.setHeaderHeight(50) ' header height
            ' client.setHeaderUrl(url) ' header url
            client.setHeaderHtml("This is the <b>HEADER</b>!!!!") ' header html

            ' footer properties
            client.setShowFooter(True) ' display footer
            ' client.setFooterHeight(60) ' footer height
            ' client.setFooterUrl(url) ' footer url
            client.setFooterHtml("This is the <b>FOOTER</b>!!!!") ' footer html

            ' footer page numbers
            client.setShowPageNumbers(True) ' show page numbers In footer
            client.setPageNumbersTemplate("{page_number} / {total_pages}") ' page numbers template
            client.setPageNumbersFontName("Verdana") ' page numbers font name
            client.setPageNumbersFontSize(12) ' page numbers font size
            client.setPageNumbersAlignment(PageNumbersAlignment.Center) ' page numbers alignment (2-Center)

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
