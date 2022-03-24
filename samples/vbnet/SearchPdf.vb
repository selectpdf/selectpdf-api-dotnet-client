Imports System
Imports System.Collections.Generic
Imports SelectPdf.Api

Public Class SearchPdf
    Public Shared Sub RunTest()
        Dim testUrl As String = "https://selectpdf.com/demo/files/selectpdf.pdf"
        Dim testPdf As String = "Input.pdf"
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As PdfToTextClient = New PdfToTextClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/pdf-to-text-api/

            client.setStartPage(1) ' start page (processing starts from here)
            client.setEndPage(0) ' End page (Set 0 To process file til the End)
            client.setOutputFormat(OutputFormat.Text) ' Set output format (0-Text Or 1-HTML)

            Console.WriteLine("Starting search pdf ...")

            ' search local pdf
            Dim results As IList(Of TextPosition) = client.searchFile(testPdf, "pdf")

            ' search pdf from public url
            ' Dim results As IList(Of TextPosition) = client.searchUrl(testUrl, "pdf")

            Console.WriteLine("Search results:{0}{1}{0}Search results count: {2}.", Environment.NewLine, String.Join(Environment.NewLine, results), results.Count)

            Console.WriteLine("Finished! Number of pages processed: {0}.", client.getNumberOfPages())

            ' get API usage
            Dim usageClient As UsageClient = New UsageClient(apiKey)
            Dim usage As UsageInformation = usageClient.getUsage(False)
            Console.WriteLine("Conversions remained this month: {0}.", usage.Available)

        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
