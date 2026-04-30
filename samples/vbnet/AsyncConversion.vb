Imports System
Imports SelectPdf.Api

Public Class AsyncConversion
    Public Shared Sub RunTest()
        Dim url As String = "https://selectpdf.com"
        Dim localFile As String = "Test.pdf"

        ' Async conversions are not supported on the demo endpoint -
        ' this sample requires a paid API key.
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As HtmlToPdfClient = New HtmlToPdfClient(apiKey)

            ' Tune polling for the async job (optional).
            ' The client polls /api2/asyncjob/ every AsyncCallsPingInterval
            ' seconds, up to AsyncCallsMaxPings times, then gives up.
            client.AsyncCallsPingInterval = 3 ' seconds between polls
            client.AsyncCallsMaxPings = 1000 ' max polls before timeout

            client.setPageSize(PageSize.A4)
            client.setPageOrientation(PageOrientation.Portrait)
            client.setMargins(0)
            client.setPageBreaksEnhancedAlgorithm(True)

            Console.WriteLine("Starting async conversion ...")

            ' url to file (async)
            client.convertUrlToFileAsync(url, localFile)

            ' url to memory (async)
            ' Dim pdf As Byte() = client.convertUrlAsync(url)

            ' html string to file (async)
            ' client.convertHtmlStringToFileAsync("This is some <b>html</b>.", localFile)

            ' html string to memory (async)
            ' Dim pdf As Byte() = client.convertHtmlStringAsync("This is some <b>html</b>.")

            Console.WriteLine("Finished! Number of pages: {0}.", client.getNumberOfPages())

            ' response telemetry (SDK 1.5.0)
            Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode)
            Console.WriteLine("Credits remaining: {0} / {1}.", client.CreditsRemaining, client.CreditsTotal)

        Catch ex As ApiException
            Console.WriteLine("API error: {0}", ex.Message)
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
