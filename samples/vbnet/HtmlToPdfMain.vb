Imports System
Imports SelectPdf.Api

Public Class HtmlToPdfMain
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

            ' main properties
            client.setPageSize(PageSize.A4) ' PDF page size
            client.setPageOrientation(PageOrientation.Portrait) ' PDF page orientation
            client.setMargins(0) ' PDF page margins
            client.setRenderingEngine(RenderingEngine.WebKit) ' rendering engine (demo mode forces Chromium)
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
            ' client.setScaleImages(True) ' scale images to create smaller pdfs
            ' client.setSinglePagePdf(True) ' generate a Single page PDF
            ' client.setUserPassword("password") ' secure the PDF With a password (paid keys only)

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

                ' get API usage (paid keys only - demo endpoint has no usage account)
                Dim usageClient As UsageClient = New UsageClient(apiKey)
                Dim usage As UsageInformation = usageClient.getUsage(False)
                Console.WriteLine("Conversions remained this month: {0}.", usage.Available)
            End If

        Catch ex As DemoRateLimitException
            ' reason is one of: per_ip, daily_cap, concurrency
            Console.WriteLine("Demo rate limit ({0}). Retry after {1}s. Upgrade: {2}", ex.Reason, ex.RetryAfter, ex.UpgradeUrl)
        Catch ex As DemoSafetyException
            ' demo only converts public URLs - internal/private hosts are rejected
            Console.WriteLine("Demo safety guard rejected '{0}' (reason={1}).", ex.Field, ex.Reason)
        Catch ex As DemoUnsupportedException
            ' feature not available on the demo endpoint (e.g. setUserPassword)
            Console.WriteLine("Feature '{0}' not available in demo mode. Upgrade: {1}", ex.Field, ex.UpgradeUrl)
        Catch ex As ApiException
            Console.WriteLine("API error: {0}", ex.Message)
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
