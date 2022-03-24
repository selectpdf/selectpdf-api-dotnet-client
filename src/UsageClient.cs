using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace SelectPdf.Api
{
    /// <summary>
    /// Get usage details for SelectPdf Online API.
    /// </summary>
    /// <example>
    /// Get usage details for SelectPdf online REST API:
    /// <code language="cs">
    /// using System;
    /// using SelectPdf.Api;
    /// 
    /// namespace SelectPdf.Api.Tests
    /// {
    ///     class Program
    ///     {
    ///         static void Main(string[] args)
    ///         {
    ///             string apiKey = "Your API key here";
    /// 
    ///             Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION);
    /// 
    ///             try
    ///             {
    ///                 // get API usage
    ///                 UsageClient usageClient = new UsageClient(apiKey);
    ///                 UsageInformation usage = usageClient.getUsage(false);
    ///                 Console.WriteLine("Conversions remained this month: {0}.", usage.Available);
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 Console.WriteLine("An error occurred: " + ex.Message);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language="vb">
    /// Imports SelectPdf.Api
    /// 
    /// Module Program
    ///     Sub Main(args As String())
    ///         Dim apiKey As String = "Your API key here"
    /// 
    ///         Console.WriteLine("This is SelectPdf-{0}.", ApiClient.CLIENT_VERSION)
    /// 
    ///         Try
    ///             ' get API usage
    ///             Dim usageClient As UsageClient = New UsageClient(apiKey)
    ///             Dim usage As UsageInformation = usageClient.getUsage(False)
    ///             Console.WriteLine("Conversions remained this month: {0}.", usage.Available)
    /// 
    ///         Catch ex As Exception
    ///             Console.WriteLine("An error occurred: " &amp; ex.Message)
    ///         End Try
    ///     End Sub
    /// End Module
    /// </code>
    /// </example>
    public class UsageClient : ApiClient
    {
        /// <summary>
        /// Construct the Usage client.
        /// </summary>
        /// <param name="apiKey">API Key.</param>
        public UsageClient(string apiKey)
        {
            apiEndpoint = "https://selectpdf.com/api2/usage/";
            parameters["key"] = apiKey;
        }

        /// <summary>
        /// Get API usage information.
        /// </summary>
        /// <returns>Usage information.</returns>
        public UsageInformation getUsage()
        {
            return getUsage(false);
        }

        /// <summary>
        /// Get API usage information with history if specified.
        /// </summary>
        /// <param name="getHistory">Get history also.</param>
        /// <returns>Usage information.</returns>
        public UsageInformation getUsage(bool getHistory)
        {
            headers["Accept"] = "text/xml";

            if (getHistory)
            {
                parameters["get_history"] = "True";
            }

            MemoryStream outStream = new MemoryStream();
            PerformPost(outStream);

            try
            {
                UsageInformation usage = null;
                XmlSerializer serializer = new XmlSerializer(typeof(UsageInformation));

                usage = (UsageInformation)serializer.Deserialize(outStream);
                outStream.Close();

                return usage;
            }
            catch (Exception ex)
            {
                throw new ApiException("Could not get API usage.", ex.InnerException);
            }
        }


    }

    /// <summary>
    /// SelectPdf API usage information.
    /// </summary>
    [Serializable, XmlRoot(ElementName = "UsageResponse", Namespace = "http://schemas.datacontract.org/2004/07/SelectPdf"), XmlType("UsageResponse")]
    public class UsageInformation
    {
        /// <summary>
        /// Subscription status.
        /// </summary>
        [XmlElement("status")]
        public string Status { get; set; }

        /// <summary>
        /// Subscription type.
        /// </summary>
        [XmlElement("subscription_type")]
        public string SubscriptionType { get; set; }

        /// <summary>
        /// Monthly conversions limit.
        /// </summary>
        [XmlElement("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Number of conversions used in the current month.
        /// </summary>
        [XmlElement("used")]
        public int Used { get; set; }

        /// <summary>
        /// Number of conversions available in the current month.
        /// </summary>
        [XmlElement("available")]
        public int Available { get; set; }

        /// <summary>
        /// Usage monthly history.
        /// </summary>
        [XmlArray("history")]
        [XmlArrayItem("UsageHistory")]
        public List<UsageMonthlyDetails> History { get; set; }

        /// <summary>
        /// Construct the usage information object.
        /// </summary>
        public UsageInformation()
        {
            History = new List<UsageMonthlyDetails>();
        }
    }

    /// <summary>
    /// SelectPdf API monthly usage information.
    /// </summary>
    [Serializable, XmlRoot(ElementName = "UsageHistory"), XmlType("UsageHistory")]
    public class UsageMonthlyDetails
    {
        /// <summary>
        /// Year.
        /// </summary>
        [XmlElement("year")]
        public int Year { get; set; }

        /// <summary>
        /// Month.
        /// </summary>
        [XmlElement("month")]
        public int Month { get; set; }

        /// <summary>
        /// Number of conversions performed.
        /// </summary>
        [XmlElement("conversions")]
        public int Conversions { get; set; }

        /// <summary>
        /// Number of credits used.
        /// </summary>
        [XmlElement("credits")]
        public int Credits { get; set; }

    }
}
