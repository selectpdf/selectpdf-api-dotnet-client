Imports System
Imports SelectPdf.Api

Public Class PdfToText
    Public Shared Sub RunTest()
        Dim testUrl As String = "https://selectpdf.com/demo/files/selectpdf.pdf"
        Dim testPdf As String = "Input.pdf"
        Dim localFile As String = "Result.txt"
        Dim apiKey As String = "Your API key here"

        Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)

        Try
            Dim client As PdfToTextClient = New PdfToTextClient(apiKey)

            ' set parameters - see full list at https://selectpdf.com/pdf-to-text-api/

            client.setStartPage(1) ' start page (processing starts from here)
            client.setEndPage(0) ' End page (Set 0 To process file til the End)
            client.setOutputFormat(OutputFormat.Text) ' Set output format (0-Text Or 1-HTML)

            Console.WriteLine("Starting pdf to text ...")

            ' convert local pdf to local text file
            client.getTextFromFileToFile(testPdf, localFile)

            ' extract text from local pdf to memory
            ' Dim text As String = client.getTextFromFile(testPdf)
            ' print text
            ' Console.WriteLine(text)

            ' convert pdf from public url to local text file
            ' client.getTextFromUrlToFile(testUrl, localFile)

            ' extract text from pdf from public url to memory
            ' Dim text As String = client.getTextFromUrl(testUrl)
            ' print text
            ' Console.WriteLine(text)

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
