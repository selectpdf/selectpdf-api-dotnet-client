# SelectPdf Online REST API — VB.NET Samples

Runnable VB.NET samples for the **SelectPdf.Api 1.5.0** NuGet package, covering every service the API exposes: HTML-to-PDF (sync and async), PDF merge, PDF-to-text, text search, and web-element location lookup.

## Requirements

- .NET 5.0 SDK or later (the project targets `net5.0`; any newer SDK that can target it works).
- Internet access to reach `https://selectpdf.com/api2/...`.
- Optional: a paid SelectPdf API key. Without one, only the HTML-to-PDF sample runs (against the keyless demo endpoint).

## Get the package

```
Install-Package SelectPdf.Api -Version 1.5.0
```

The samples already reference `SelectPdf.Api 1.5.0` from nuget.org. A first `dotnet build` will restore it automatically.

## Set your API key

Open the sample you want to run and replace the placeholder:

```vb
Dim apiKey As String = "Your API key here"
```

For HTML-to-PDF, you can leave the key as `Nothing` (or pass `"demo"`) to use the **keyless demo endpoint** — no signup required, but output is watermarked, capped at 5 pages, and uses the Chromium engine only. PDF Merge, PDF-to-Text, Search, Web Elements, and Async conversion all require a paid key.

Get a key at <https://selectpdf.com/pricing/>.

## Run

From this folder:

```
dotnet run                       ' default: HtmlToPdfMain (works with no key)
dotnet run -- htmltopdf
dotnet run -- headersfooters
dotnet run -- pdfmerge
dotnet run -- pdftotext
dotnet run -- searchpdf
dotnet run -- webelements
dotnet run -- async
```

| Sample | File | Service | Demo-friendly |
| --- | --- | --- | :---: |
| HTML to PDF | `HtmlToPdfMain.vb` | HTML → PDF | yes |
| Headers & footers | `HtmlToPdfHeadersAndFooters.vb` | HTML → PDF with custom header/footer | yes |
| PDF Merge | `PdfMerge.vb` | merge multiple PDFs | no (paid key) |
| PDF to Text | `PdfToText.vb` | extract text from PDF | no (paid key) |
| Search PDF | `SearchPdf.vb` | search inside a PDF | no (paid key) |
| Web Elements | `WebElements.vb` | locate HTML elements in the generated PDF | no (paid key) |
| Async conversion | `AsyncConversion.vb` | long-running HTML → PDF jobs | no (paid key) |

## What 1.5.0 adds you'll see in the samples

- **Keyless demo endpoint** (`HtmlToPdfClient`) — `Nothing`, `""`, or `"demo"` selects it. Inspect `client.IsDemoMode` and `client.IsDemoResponse` for whether you're running against demo. See `HtmlToPdfMain.vb` and `HtmlToPdfHeadersAndFooters.vb`.
- **Typed demo exceptions** — `DemoRateLimitException`, `DemoSafetyException`, `DemoUnsupportedException`. Catch these explicitly to react (e.g. honor `Retry-After`, prompt for a paid upgrade) instead of parsing `ex.Message`.
- **Response telemetry** on every client — `Mode`, `ExecutionMode`, `CreditsTotal`, `CreditsRemaining`, plus `ClampedFields` / `DroppedFields` on `HtmlToPdfClient`. The samples print these after every conversion.
- **Chromium rendering engine** — `RenderingEngine.Chromium` is now selectable on paid keys; demo mode forces it.

## Demo-mode quick reference

- 5-page cap, watermarked output, Chromium engine only.
- Public URLs only (HTTPS preferred). Internal/private hosts are rejected with `DemoSafetyException`.
- Per-IP and global rate limits — handled via `DemoRateLimitException` (read `Reason`, `RetryAfter`, `UpgradeUrl`).
- `setUserPassword` / `setOwnerPassword` / `async = True` are not available — `DemoUnsupportedException`.
- `auth_username`, `auth_password`, `cookies`, `raw_parameters`, `pdf_name`, `async` are silently dropped — see `client.DroppedFields`.
- `setNavigationTimeout` is clamped to 15 s, `setConversionDelay` to 5 s — see `client.ClampedFields`.

For full demo behavior and limits, see the package readme on NuGet or <https://selectpdf.com/html-to-pdf-api/>.

## Documentation

- HTML to PDF API: <https://selectpdf.com/html-to-pdf-api/>
- PDF Merge API: <https://selectpdf.com/pdf-merge-api/>
- PDF to Text API: <https://selectpdf.com/pdf-to-text-api/>
- Pricing & paid plans: <https://selectpdf.com/pricing/>
- Support: <https://selectpdf.com/support/>
