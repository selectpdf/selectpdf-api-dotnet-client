Imports System
Imports System.Collections.Generic
Imports SelectPdf.Api

Public Class WebElements
    Public Shared Sub RunTest()
        Dim url As String = "https://selectpdf.com"
        Dim localFile As String = "Test.pdf"

        ' Web elements lookup requires a paid API key (the demo endpoint
        ' does not expose the elements service).
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As HtmlToPdfClient = New HtmlToPdfClient(apiKey)

            ' CSS selectors used to identify HTML elements whose location in
            ' the resulting PDF should be reported back. See the API docs for
            ' selector syntax: https://selectpdf.com/html-to-pdf-api/
            client.setPageSize(PageSize.A4)
            client.setMargins(0)
            client.setPdfWebElementsSelectors("H1, H2, *.menu, *#footer")

            Console.WriteLine("Starting conversion ...")

            client.convertUrlToFile(url, localFile)

            Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages())

            ' Retrieve element rectangles. Returns an empty list if no element
            ' matched the configured selectors.
            Dim elements As IList(Of WebElement) = client.getWebElements()
            Console.WriteLine("Web elements found: {0}.", elements.Count)

            For Each element As WebElement In elements
                Dim rectCount As Integer = If(element.PdfRectangles Is Nothing, 0, element.PdfRectangles.Length)
                Console.WriteLine(" - <{0}> id='{1}' class='{2}' rectangles={3}",
                    element.HtmlElementTagName,
                    element.HtmlElementId,
                    element.HtmlElementCssClassName,
                    rectCount)
            Next

            Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode)
            Console.WriteLine("Credits remaining: {0} / {1}.", client.CreditsRemaining, client.CreditsTotal)

        Catch ex As ApiException
            Console.WriteLine("API error: {0}", ex.Message)
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
