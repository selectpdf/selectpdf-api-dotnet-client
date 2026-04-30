Imports System
Imports SelectPdf.Api

Public Class HtmlToPdfHeadersAndFooters
    Public Shared Sub RunTest()
        Dim url As String = "https://selectpdf.com"
        Dim localFile As String = "Test.pdf"

        ' Pass Nothing / empty / "demo" to use the keyless demo endpoint
        ' (output is watermarked, capped at 5 pages, Chromium engine only).
        ' Replace with a real key for full production output.
        Dim apiKey As String = Nothing

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

            ' response telemetry (SDK 1.5.0)
            Console.WriteLine("Mode: {0}, Execution: {1}.", client.Mode, client.ExecutionMode)
            If client.IsDemoMode Then
                If client.WasClamped Then
                    Console.WriteLine("Demo clamped: {0}.", String.Join(", ", client.ClampedFields))
                End If
                If client.WasAnyFieldDropped Then
                    Console.WriteLine("Demo dropped: {0}.", String.Join(", ", client.DroppedFields))
                End If
            Else
                Console.WriteLine("Credits remaining: {0} / {1}.", client.CreditsRemaining, client.CreditsTotal)

                ' get API usage (paid keys only)
                Dim usageClient As UsageClient = New UsageClient(apiKey)
                Dim usage As UsageInformation = usageClient.getUsage(False)
                Console.WriteLine("Conversions remained this month: {0}.", usage.Available)
            End If

        Catch ex As DemoRateLimitException
            Console.WriteLine("Demo rate limit ({0}). Retry after {1}s. Upgrade: {2}", ex.Reason, ex.RetryAfter, ex.UpgradeUrl)
        Catch ex As DemoSafetyException
            Console.WriteLine("Demo safety guard rejected '{0}' (reason={1}).", ex.Field, ex.Reason)
        Catch ex As DemoUnsupportedException
            Console.WriteLine("Feature '{0}' not available in demo mode. Upgrade: {1}", ex.Field, ex.UpgradeUrl)
        Catch ex As ApiException
            Console.WriteLine("API error: {0}", ex.Message)
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
