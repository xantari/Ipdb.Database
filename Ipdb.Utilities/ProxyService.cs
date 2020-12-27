using HtmlAgilityPack;
using Ipdb.Models;
using Ipdb.Utilities.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Ipdb.Utilities
{
    public class ProxyServices
    {
        private readonly string _userAgentString;
        public ProxyServices(string userAgentString)
        {
            _userAgentString = userAgentString;
        }

        public string GetIPAddress(IpCheckService ipService, ProxySetting proxySetting = null)
        {
            HtmlDocument doc = new HtmlDocument();
            HtmlWeb web = new HtmlWeb();

            if (ipService == IpCheckService.DynDns)
            {
                string checkUrl = "http://checkip.dyndns.com/";
                if (proxySetting == null)
                    doc = web.Load(checkUrl);
                else
                {
                    string page = DownloadHtmlWithProxySettings(checkUrl, proxySetting);
                    if (!string.IsNullOrWhiteSpace(page))
                        doc.LoadHtml(page);
                    else
                        return null;
                }

                string searchString = "Current IP Address: ";
                int startIndexOfIp = doc.DocumentNode.InnerHtml.IndexOf(searchString);
                if (startIndexOfIp >= 0 && (startIndexOfIp - searchString.Length) >= 0)
                {
                    int endOfIpAddress = doc.DocumentNode.InnerHtml.IndexOf("</body>");
                    int startIndex = startIndexOfIp + searchString.Length;
                    string ipTemp = doc.DocumentNode.InnerHtml.Substring(startIndex, endOfIpAddress - startIndex);
                    return ipTemp;
                }
            }
            return null;
        }

        public string DownloadHtmlWithProxySettings(string url, ProxySetting proxySetting)
        {
            WebProxy proxy = new WebProxy($"{proxySetting.Host}:{proxySetting.Port}");
            if (!string.IsNullOrWhiteSpace(proxySetting.UserName))
            {
                proxy.Credentials = new NetworkCredential(proxySetting.UserName, proxySetting.Password);
            }

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
                UseDefaultCredentials = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                //CookieContainer = new CookieContainer(),
                UseProxy = true
            };

            HttpClient wc = new HttpClient(httpClientHandler);

            //User-Agent: 
            //https://techblog.willshouse.com/2012/01/03/most-common-user-agents/
            wc.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            //wc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate"); //This gets added above in the HttpClientHandler constructor
            wc.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", _userAgentString);
            wc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            int tryCount = 0;
            string data = null;
            while (tryCount < proxySetting.FailureRetryCount)
            {
                try
                {
                    HttpResponseMessage response = wc.GetAsync(new Uri(url)).Result;
                    response.EnsureSuccessStatusCode();

                    using (Stream responseStream = response.Content.ReadAsStreamAsync().Result)
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Proxy error encountered. try count = {tryCount} of {retryCount}", tryCount, proxySetting.FailureRetryCount);
                    Extensions.SleepForRandomTime(true, 250, 1500);
                }
                tryCount++;
            }
            if (tryCount >= proxySetting.FailureRetryCount)
            {
                Log.Error("Proxy error encountered. Max tries exceeded.", tryCount, proxySetting.FailureRetryCount);
            }
            return data;
        }

        public bool IsProxyWorking(ProxySetting proxySetting)
        {
            string nonProxyIp = GetIPAddress(IpCheckService.DynDns);
            string proxyIp = GetIPAddress(IpCheckService.DynDns, proxySetting);
            Log.Information("Non proxy IP {nonProxyIp}, proxy ip {proxyIp}", nonProxyIp, proxyIp);
            if (proxyIp == nonProxyIp)
                return false;
            return true;
        }
    }

    public enum IpCheckService
    {
        DynDns
    }
}
