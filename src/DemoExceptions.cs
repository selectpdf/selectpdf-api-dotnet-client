using System;

namespace SelectPdf.Api
{
    /// <summary>
    /// Default upgrade URL displayed in demo-mode error messages.
    /// </summary>
    internal static class DemoConstants
    {
        public const string UpgradeUrl = "https://selectpdf.com/pricing/";
    }

    /// <summary>
    /// Thrown when the demo endpoint refuses a request because of a rate limit
    /// (HTTP 429 or 503). Inspect <see cref="Reason"/> to distinguish:
    ///   <list type="bullet">
    ///     <item><c>per_ip</c> — the source IP exceeded its hourly conversion budget</item>
    ///     <item><c>concurrency</c> — too many demo conversions are running right now</item>
    ///     <item><c>daily_cap</c> — the global demo budget for today has been reached</item>
    ///   </list>
    /// </summary>
    public class DemoRateLimitException : ApiException
    {
        /// <summary>HTTP status code returned by the server (429 or 503).</summary>
        public int StatusCode { get; private set; }

        /// <summary>Machine-readable rate-limit reason: <c>per_ip</c>, <c>concurrency</c>, or <c>daily_cap</c>.</summary>
        public string Reason { get; private set; }

        /// <summary>Seconds the client should wait before retrying (parsed from <c>Retry-After</c>). Zero if absent.</summary>
        public int RetryAfter { get; private set; }

        /// <summary>URL the user can visit to upgrade out of demo mode.</summary>
        public string UpgradeUrl { get; private set; }

        /// <summary>The raw JSON body the server returned, for diagnostics / logging.</summary>
        /// <summary>The raw JSON body the server returned, for diagnostics / logging.</summary>
        public string ResponseBody { get; private set; }

        /// <summary>Constructor invoked by the SDK when the demo endpoint returns a rate-limit error.</summary>
        public DemoRateLimitException(int statusCode, string reason, int retryAfter, string upgradeUrl, string body, Exception inner)
            : base(BuildMessage(statusCode, reason, retryAfter, upgradeUrl), inner)
        {
            StatusCode = statusCode;
            Reason = reason ?? string.Empty;
            RetryAfter = retryAfter;
            UpgradeUrl = string.IsNullOrEmpty(upgradeUrl) ? DemoConstants.UpgradeUrl : upgradeUrl;
            ResponseBody = body;
        }

        private static string BuildMessage(int statusCode, string reason, int retryAfter, string upgradeUrl)
        {
            string upgrade = string.IsNullOrEmpty(upgradeUrl) ? DemoConstants.UpgradeUrl : upgradeUrl;
            string ra = retryAfter > 0 ? string.Format(" Retry after {0}s.", retryAfter) : string.Empty;
            return string.Format("({0}) Demo rate limit reached (reason={1}).{2} Upgrade at {3}.",
                statusCode, reason ?? "?", ra, upgrade);
        }
    }

    /// <summary>
    /// Thrown when the demo SSRF guard rejects a request because a URL field
    /// references a non-public host (HTTP 400). Inspect <see cref="Field"/> for
    /// which parameter was rejected and <see cref="Reason"/> for why.
    /// </summary>
    public class DemoSafetyException : ApiException
    {
        /// <summary>HTTP status code returned by the server (400).</summary>
        public int StatusCode { get; private set; }

        /// <summary>Which input field was rejected: <c>url</c>, <c>html</c>, <c>base_url</c>, <c>header_url</c>, <c>footer_url</c>.</summary>
        public string Field { get; private set; }

        /// <summary>Why the field was rejected: <c>blocked_host</c>, <c>private_ip</c>, <c>loopback</c>, <c>link_local</c>, <c>cgnat</c>, <c>metadata</c>, <c>multicast</c>, <c>bad_scheme</c>, <c>bad_url</c>, <c>inline_internal_ref:&lt;sub-reason&gt;</c>.</summary>
        public string Reason { get; private set; }

        /// <summary>The raw JSON body the server returned, for diagnostics / logging.</summary>
        public string ResponseBody { get; private set; }

        /// <summary>Constructor invoked by the SDK when the demo SSRF guard rejects a URL field.</summary>
        public DemoSafetyException(int statusCode, string field, string reason, string body, Exception inner)
            : base(string.Format("({0}) Demo safety guard rejected {1} (reason={2}). Demo conversions cannot fetch internal/private hosts.",
                statusCode, field ?? "?", reason ?? "?"), inner)
        {
            StatusCode = statusCode;
            Field = field ?? string.Empty;
            Reason = reason ?? string.Empty;
            ResponseBody = body;
        }
    }

    /// <summary>
    /// Thrown when the caller tried to use a feature that demo mode doesn't
    /// support — most commonly PDF passwords (HTTP 400). For paid keys, this
    /// exception is never thrown.
    /// </summary>
    public class DemoUnsupportedException : ApiException
    {
        /// <summary>HTTP status code returned by the server (400). Zero if the exception was raised by the local client guard before the request was sent.</summary>
        public int StatusCode { get; private set; }

        /// <summary>Which feature is unsupported: <c>user_password</c>, <c>owner_password</c>.</summary>
        public string Field { get; private set; }

        /// <summary>URL the user can visit to upgrade out of demo mode.</summary>
        public string UpgradeUrl { get; private set; }

        /// <summary>The raw JSON body the server returned, for diagnostics / logging. Null when raised by the local client guard.</summary>
        public string ResponseBody { get; private set; }

        /// <summary>Constructor invoked by the SDK when the server rejects a demo-incompatible parameter.</summary>
        public DemoUnsupportedException(int statusCode, string field, string upgradeUrl, string body, Exception inner)
            : base(string.Format("({0}) Feature '{1}' is not available in demo mode. Upgrade at {2}.",
                statusCode, field ?? "?", string.IsNullOrEmpty(upgradeUrl) ? DemoConstants.UpgradeUrl : upgradeUrl), inner)
        {
            StatusCode = statusCode;
            Field = field ?? string.Empty;
            UpgradeUrl = string.IsNullOrEmpty(upgradeUrl) ? DemoConstants.UpgradeUrl : upgradeUrl;
            ResponseBody = body;
        }

        /// <summary>Constructor used by the local client-side guard before the request is sent.</summary>
        public DemoUnsupportedException(string field)
            : base(string.Format("Feature '{0}' is not available in demo mode. Construct HtmlToPdfClient with a paid API key, or upgrade at {1}.",
                field ?? "?", DemoConstants.UpgradeUrl))
        {
            StatusCode = 0;
            Field = field ?? string.Empty;
            UpgradeUrl = DemoConstants.UpgradeUrl;
            ResponseBody = null;
        }
    }
}
