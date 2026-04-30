Imports System
Imports SelectPdf.Api

Public Class PdfToText
    Public Shared Sub RunTest()
        Dim testUrl As String = "https://selectpdf.com/demo/files/selectpdf.pdf"
        Dim testPdf As String = "Input.pdf"
        Dim localFile As String = "Result.txt"

        ' PdfToText requires an API key (no demo endpoint for this service).
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As PdfToTextClient = New PdfToTextClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/pdf-to-text-api/

            client.setStartPage(1) ' start page (processing starts from here)
            client.setEndPage(0) ' End page (Set 0 To process file til the End)
            client.setOutputFormat(OutputFormat.Text) ' Set output format (0-Text Or 1-HTML)

            Console.WriteLine("Starting pdf to text ...")

            ' convert pdf from public url to local text file
            client.getTextFromUrlToFile(testUrl, localFile)

            ' extract text from pdf from public url to memory
            ' Dim text As String = client.getTextFromUrl(testUrl)
            ' Console.WriteLine(text)

            ' convert local pdf to local text file
            ' client.getTextFromFileToFile(testPdf, localFile)

            ' extract text from local pdf to memory
            ' Dim text As String = client.getTextFromFile(testPdf)
            ' Console.WriteLine(text)

            Console.WriteLine("Finished! Number of pages processed: {0}.", client.getNumberOfPages())

            ' response telemetry (SDK 1.5.0)
            Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode)
            Console.WriteLine("Credits remaining: {0} / {1}.", client.CreditsRemaining, client.CreditsTotal)

            ' get API usage
            Dim usageClient As UsageClient = New UsageClient(apiKey)
            Dim usage As UsageInformation = usageClient.getUsage(False)
            Console.WriteLine("Conversions remained this month: {0}.", usage.Available)

        Catch ex As ApiException
            Console.WriteLine("API error: {0}", ex.Message)
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
