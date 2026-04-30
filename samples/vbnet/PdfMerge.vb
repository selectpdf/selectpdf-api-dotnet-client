Imports System
Imports SelectPdf.Api

Public Class PdfMerge
    Public Shared Sub RunTest()
        Dim testUrl As String = "https://selectpdf.com/demo/files/selectpdf.pdf"
        Dim testPdf As String = "Input.pdf"
        Dim localFile As String = "Result.pdf"

        ' PdfMerge requires an API key (no demo endpoint for this service).
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As PdfMergeClient = New PdfMergeClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/pdf-merge-api/

            ' specify the pdf files that will be merged (order will be preserved in the final pdf)
            client.addUrlFile(testUrl) ' add PDF From public url
            ' client.addFile(testPdf) ' add PDF from local file
            ' client.addFile(testPdf, "pdf_password") ' add PDF (that requires a password) from local file
            ' client.addUrlFile(testUrl, "pdf_password") ' add PDF (that requires a password) from public url

            Console.WriteLine("Starting pdf merge ...")

            ' merge pdfs to local file
            client.saveToFile(localFile)

            ' merge pdfs to memory
            ' Dim pdf As Byte() = client.save()

            Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages())

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
