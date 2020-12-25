using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Serilog;
using Ipdb.Models;

namespace Ipdb.Utilities
{
    //TODO: Need to implement and test retry mechanism and failure states (such as 404, HTTP 500, etc). Also need to ensure that if a URL requires login we monitor failure to download due to login no longer exists (perhaps cookie timed out, or for some reason does not exist anymore)
    public class ScraperBase
    {
        public ScraperStatistics Statistics { get; set; }
        private ProxyServices ProxyService { get; set; }
        //private HtmlWeb HtmlWeb { get; set; }
        public HttpClient HttpWebClient { get; set; }
        public CookieContainer CookieJar { get; set; } //https://stackoverflow.com/questions/15206644/how-to-pass-cookies-to-htmlagilitypack-or-webclient
        public string RandomUserAgentString { get; set; }

        private int HTTPRequestMaxRetryCount { get; set; }
        private int SleepTimeBetweenHTTPRequestsLowRange { get; set; }
        private int SleepTimeBetweenHTTPRequestsHighRange { get; set; }

        public ScraperBase(int httpRequestMaxRetryCount = 5, int sleepTimeBetweenHTTPRequestsLowRange = 250, int sleepTimeBetweenHTTPRequestsHighRange = 1500)
        {
            CookieJar = new CookieContainer();
            Statistics = new ScraperStatistics();
            RandomUserAgentString = UserAgentStrings.GetRandomUserAgentString();

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = CookieJar,
                AllowAutoRedirect = false,
                MaxAutomaticRedirections = 20
                //UseProxy = true
            };

            HTTPRequestMaxRetryCount = httpRequestMaxRetryCount;
            SleepTimeBetweenHTTPRequestsLowRange = sleepTimeBetweenHTTPRequestsLowRange;
            SleepTimeBetweenHTTPRequestsHighRange = sleepTimeBetweenHTTPRequestsHighRange;

            HttpWebClient = new HttpClient(httpClientHandler);
        }

        public ScraperBase(ProxyServices proxyService = null, int httpRequestMaxRetryCount = 5, int sleepTimeBetweenHTTPRequestsLowRange = 250, int sleepTimeBetweenHTTPRequestsHighRange = 1500)
        {
            ProxyService = proxyService;

            HTTPRequestMaxRetryCount = httpRequestMaxRetryCount;
            SleepTimeBetweenHTTPRequestsLowRange = sleepTimeBetweenHTTPRequestsLowRange;
            SleepTimeBetweenHTTPRequestsHighRange = sleepTimeBetweenHTTPRequestsHighRange;

            //User-Agent: https://techblog.willshouse.com/2012/01/03/most-common-user-agents/
            //HttpWebClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;");
            //wc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate"); //This gets added above in the HttpClientHandler constructor
            //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", _randomUserAgentString);
            //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
        }

        internal static string RemoveUnwantedTags(string data) //https://stackoverflow.com/questions/12787449/html-agility-pack-removing-unwanted-tags-without-removing-content
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var acceptableTags = new String[] { "strong", "em", "u", "br" };

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (!acceptableTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
            }

            return document.DocumentNode.InnerHtml;
        }

        public void ClearCookies()
        {
            CookieJar = new CookieContainer();
        }

        public HtmlDocument GetPage(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            string data = string.Empty;
            int tryCount = 0;
            while (tryCount < HTTPRequestMaxRetryCount)
            {
                if (tryCount != 0)
                    Statistics.TotalRetries++;

                try
                {
                    HttpResponseMessage response = HttpWebClient.GetAsync(url).Result;
                    Statistics.TotalHTTPGets++;
                    response.EnsureSuccessStatusCode();

                    using (Stream responseStream = response.Content.ReadAsStreamAsync().Result)
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                    Extensions.SleepForRandomTime(true, SleepTimeBetweenHTTPRequestsLowRange, SleepTimeBetweenHTTPRequestsHighRange); //Make HTTP request look more natural
                    break;
                }
                catch (Exception e)
                {
                    Statistics.TotalFailures++;
                    Log.Error(e, "An error occurred download from the page. {Exception}", e);
                }
            }

            HtmlDocument document = null;
            if (!string.IsNullOrWhiteSpace(data))
            {
                document = new HtmlDocument();
                document.LoadHtml(data);
            }
            return document;
        }
    }
}
