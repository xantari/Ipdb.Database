using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using Ipdb.Models;
using Newtonsoft.Json;
using Serilog;

namespace Ipdb.Utilities
{
    public class IpdbScraper : ScraperBase
    {
        private string baseUrl = "https://www.ipdb.org/machine.cgi?id=";
        private string _scraperName = "Ipdb scraper";

        public IpdbScraper(int httpRequestMaxRetryCount = 5, int sleepTimeBetweenHTTPRequestsLowRange = 250,
            int sleepTimeBetweenHTTPRequestsHighRange = 1500) : base(httpRequestMaxRetryCount, sleepTimeBetweenHTTPRequestsLowRange, sleepTimeBetweenHTTPRequestsHighRange)
        {

        }
        public IpdbScraper(ProxyServices proxyService, int httpRequestMaxRetryCount = 5, int sleepTimeBetweenHTTPRequestsLowRange = 250, int sleepTimeBetweenHTTPRequestsHighRange = 1500)
        : base(proxyService, httpRequestMaxRetryCount, sleepTimeBetweenHTTPRequestsLowRange, sleepTimeBetweenHTTPRequestsHighRange)
        {

        }

        public IpdbDatabase ScrapeAllResume(IpdbDatabase database, string incrementalSaveLocation, int start = 1, int end = 10000)
        {
            if (database == null)
                throw new Exception("You must resume from an existing database");
            Log.Information("{Scraper}: Beginning Scrape All. Start: {start} End: {end}...", _scraperName, start, end);
            var model = database;
            int maxThresholdOfNullsBeforeQuit = 50;
            int thresholdBeforeQuitCounter = 0;
            for (int i = start; i < end; i++)
            {
                var result = Scrape(i);
                if (result != null)
                {
                    model.Data.Add(result);
                    thresholdBeforeQuitCounter = 0; //Reset since we finally found a valid machine id
                }
                else
                    thresholdBeforeQuitCounter++;

                if (thresholdBeforeQuitCounter > maxThresholdOfNullsBeforeQuit)
                {
                    Log.Information("{Scraper}: Reached maximum threshold of invalid machine id's not returning results. Quiting...", _scraperName);
                    break; //Reached to many invalid machines, quit.
                }

                if (i % 50 == 0 && !string.IsNullOrEmpty(incrementalSaveLocation)) //Every 50 entries save where we are at so we can resume if errors occur
                {
                    Log.Information("{Scraper}: Reached incremental save threshold. Saving where we are at so far.", _scraperName);
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Formatting = Formatting.Indented;
                    //serializer.Error += Serializer_Error; //Ignore errors
                    using (StreamWriter sw = new StreamWriter(incrementalSaveLocation, false))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, model);
                    }
                }
            }
            Log.Information("{Scraper}: Finished Scrape All. Start: {start} End: {end}...", _scraperName, start, end);
            return model;
        }

        public IpdbDatabase ScrapeAll(string incrementalSaveLocation, int start = 1, int end = 10000)
        {
            Log.Information("{Scraper}: Beginning Scrape All. Start: {start} End: {end}...", _scraperName, start,end);
            var model = new IpdbDatabase();
            int maxThresholdOfNullsBeforeQuit = 50;
            int thresholdBeforeQuitCounter = 0;
            for (int i = start; i < end; i++)
            {
                var result = Scrape(i);
                if (result != null)
                {
                    model.Data.Add(result);
                    thresholdBeforeQuitCounter = 0; //Reset since we finally found a valid machine id
                }
                else
                    thresholdBeforeQuitCounter++;

                if (thresholdBeforeQuitCounter > maxThresholdOfNullsBeforeQuit)
                {
                    Log.Information("{Scraper}: Reached maximum threshold of invalid machine id's not returning results. Quiting...", _scraperName);
                    break; //Reached to many invalid machines, quit.
                }

                if (i % 50 == 0 && !string.IsNullOrEmpty(incrementalSaveLocation)) //Every 50 entries save where we are at so we can resume if errors occur
                {
                    Log.Information("{Scraper}: Reached incremental save threshold. Saving where we are at so far.", _scraperName);
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Formatting = Formatting.Indented;
                    //serializer.Error += Serializer_Error; //Ignore errors
                    using (StreamWriter sw = new StreamWriter(incrementalSaveLocation, false))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, model);
                    }
                }
            }
            Log.Information("{Scraper}: Finished Scrape All. Start: {start} End: {end}...", _scraperName, start, end);
            return model;
        }

        public IpdbResult Scrape(int machineId)
        {
            Log.Information("{Scraper}: Retrieving machine id: {id}...", _scraperName, machineId);
            IpdbResult result = new IpdbResult();

            HtmlDocument doc = GetPage(baseUrl + machineId);

            Log.Information("{Scraper}: Retrieved: {url}", _scraperName, baseUrl + machineId);
            result.Title = doc.DocumentNode.SelectSingleNode("//center/font")?.InnerText;
            if (result.Title == null) //Reach an invalid machine id
                return null;
            result.IpdbId = machineId;
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//font[contains(.,'IPD No.')]");
            result.AdditionalDetails = node.InnerText.Replace(result.Title + " /", "").Trim();

            //Get # Players info
            if (result.AdditionalDetails.ToLower().Contains("player"))
            {
                var temp = result.AdditionalDetails.ToLower();
                var endIndex = temp.IndexOf("player");
                if (endIndex >= 0)
                {
                    var previousSlash = temp.LastIndexOf("/", endIndex - 4); //Rewide back a few spaces
                    int players = 0;
                    bool success = int.TryParse(temp.Substring(previousSlash + 1, endIndex - previousSlash - 1), out players);
                    if (success)
                        result.Players = players;
                }
            }

            Log.Information("{Scraper}: Fetching Ratings...", _scraperName);
            string rating = doc.DocumentNode.SelectSingleNode("//a[contains(@href,'rate/showrate')]")?.InnerText.Replace("&nbsp;", "").Replace("/10", "").Trim();
            if (!string.IsNullOrEmpty(rating) && rating.IsNumeric())
                result.AverageFunRating = Convert.ToDecimal(rating);

            node = doc.DocumentNode.SelectSingleNode("//a[contains(@href,'search.pl?searchtype=advanced&amp;mfgid')]");
            if (node != null)
            {
                result.Manufacturer = node.InnerText.Trim();
                string href = node.Attributes["href"].Value;
                result.ManufacturerId =
                    Convert.ToInt32(href.Substring(href.IndexOf("mfgid") + 6, href.Length - (href.IndexOf("mfgid") + 6)));
            }

            Log.Information("{Scraper}: Fetching machine type...", _scraperName);
            result.Type = GetTextBySignificantNode(doc, "Type:");
            result.MPU = GetTextBySignificantNode(doc, "MPU:");
            result.ProductionNumber = GetIntNullableBySignificantNode(doc, "Production:");
            result.CommonAbbreviations = GetTextBySignificantNode(doc, "Common Abbreviations:");
            result.Theme = GetTextBySignificantNode(doc, "Theme:");
            result.NotableFeatures = GetTextBySignificantNode(doc, "Notable Features:");
            result.Toys = GetTextBySignificantNode(doc, "Toys:");
            result.DesignBy = GetTextBySignificantNode(doc, "Design by:");
            result.ArtBy = GetTextBySignificantNode(doc, "Art by:");
            result.DotsAnimationBy = GetTextBySignificantNode(doc, "Dots/Animation by:");
            result.MechanicsBy = GetTextBySignificantNode(doc, "Mechanics by:");
            result.MusicBy = GetTextBySignificantNode(doc, "Music by:");
            result.SoundBy = GetTextBySignificantNode(doc, "Sound by:");
            result.SoftwareBy = GetTextBySignificantNode(doc, "Software by:");
            result.Notes = GetTextBySignificantNode(doc, "Notes:");
            result.MarketingSlogans = GetTextBySignificantNode(doc, "Marketing Slogans:", false);
            result.PhotosIn = GetTextBySignificantNode(doc, "Photos in:");
            result.DateOfManufacture = GetDateBySignificantNode(doc, "Date Of Manufacture:");
            result.ModelNumber = GetTextBySignificantNode(doc, "Model Number:");
            result.Source = GetTextBySignificantNode(doc, "Source:");
            result.ManufacturerShortName = GetManufacturerShortName(result.Manufacturer);

            Log.Information("{Scraper}: Fetching rulesheets...", _scraperName);
            result.RuleSheetUrls = GetUrlsBySignificantNode(doc, "Rule Sheets:");
            Log.Information("{Scraper}: Fetching ROMS...", _scraperName);
            result.ROMs = GetFileUrlsBySignificantNode(doc, "ROMs:");
            Log.Information("{Scraper}: Fetching Documentation...", _scraperName);
            result.Documentation = GetFileUrlsBySignificantNode(doc, "Documentation:");
            Log.Information("{Scraper}: Fetching Service Bulletins...", _scraperName);
            result.ServiceBulletins = GetFileUrlsBySignificantNode(doc, "Service Bulletins:");
            Log.Information("{Scraper}: Fetching Misc Files...", _scraperName);
            result.Files = GetFileUrlsBySignificantNode(doc, "Files:");
            Log.Information("{Scraper}: Fetching Multimedia Files...", _scraperName);
            result.MultimediaFiles = GetFileUrlsBySignificantNode(doc, "Multimedia Files:");
            Log.Information("{Scraper}: Fetching Image Files...", _scraperName);
            result.ImageFiles = GetImageUrlsBySignificantNode(doc, "Images:");

            Log.Information("{Scraper}: Finished fetching data for machine id {id} ({title})...", _scraperName, machineId, result.Title);
            return result;
        }

        /// <summary>
        /// Get the short version of the trade name
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        private string GetManufacturerShortName(string manufacturer)
        {
            if (manufacturer.ToLower().Contains("alvin g."))
                return "Alvin G.";
            if (manufacturer.ToLower().Contains("atari"))
                return "Atari";
            if (manufacturer.ToLower().Contains("bally"))
                return "Bally";
            if (manufacturer.ToLower().Contains("capcom"))
                return "Capcom";
            if (manufacturer.ToLower().Contains("chicago coin"))
                return "Chicago Coin";
            if (manufacturer.ToLower().Contains("data east"))
                return "Data East";
            if (manufacturer.ToLower().Contains("game plan"))
                return "Game Plan";
            if (manufacturer.ToLower().Contains("gottlieb"))
                return "Gottlieb";
            if (manufacturer.ToLower().Contains("midway"))
                return "Midway";
            if (manufacturer.ToLower().Contains("premier technology"))
                return "Premier";
            if (manufacturer.ToLower().Contains("sega"))
                return "Sega";
            if (manufacturer.ToLower().Contains("stern"))
                return "Stern";
            if (manufacturer.ToLower().Contains("williams"))
                return "Williams";
            return manufacturer;
        }

        private DateTime? GetDateBySignificantNode(HtmlDocument doc, string textToFind, bool retainCarriageReturns = true)
        {
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.NextSibling;
            if (node != null)
            {
                var data = node.InnerHtml.CondenseHtml(retainCarriageReturns).ConvertBreaksToCarriageReturns().ConvertHtmlToPlainText();
                DateTime result;
                bool success = DateTime.TryParse(data, out result);
                if (success)
                    return result;
                else if (data.IsNumeric()) //Could just be a year
                    return new DateTime(Convert.ToInt32(data), 1, 1);
                else
                    Log.Warning("Unable to parse date: " + data);
            }

            return null;
        }

        private string GetTextBySignificantNode(HtmlDocument doc, string textToFind, bool retainCarriageReturns = true)
        {
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.NextSibling;
            if (node != null)
            {
                return node.InnerHtml.CondenseHtml(retainCarriageReturns).ConvertBreaksToCarriageReturns().ConvertHtmlToPlainText().NormalizeCarriageReturns();
            }

            return null;
        }

        private int GetIntBySignificantNode(HtmlDocument doc, string textToFind)
        {
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.NextSibling;
            if (node != null)
            {
                return Convert.ToInt32(node.InnerText.CondenseHtml().ConvertHtmlToPlainText().GetOnlyNumeric());
            }

            return 0;
        }

        private int? GetIntNullableBySignificantNode(HtmlDocument doc, string textToFind)
        {
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.NextSibling;
            if (node != null)
            {
                return Convert.ToInt32(node.InnerText.CondenseHtml().ConvertHtmlToPlainText().GetOnlyNumeric());
            }

            return null;
        }

        private List<IpdbUrl> GetUrlsBySignificantNode(HtmlDocument doc, string textToFind, string urlHint = "")
        {
            List<IpdbUrl> urls = new List<IpdbUrl>();
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.NextSibling;
            if (node != null)
            {
                //Now find all urls
                var urlNodes = node.SelectNodes(".//a");
                if (!string.IsNullOrEmpty(urlHint))
                    urlNodes = node.SelectNodes(".//a[contains(@href,'" + urlHint + "')]");
                foreach (var urlNode in urlNodes)
                {
                    urls.Add(new IpdbUrl() { Name = urlNode.InnerText.CondenseHtml().ConvertHtmlToPlainText(), Url = urlNode.Attributes["href"].Value });
                }
            }

            if (urls.Count == 0)
                return null; //Prevent it from being serialized in the .json so it's smaller
            return urls;
        }

        private List<IpdbUrl> GetFileUrlsBySignificantNode(HtmlDocument doc, string textToFind)
        {
            List<IpdbUrl> urls = new List<IpdbUrl>();
            HtmlNode startOfFileSection = doc.DocumentNode.SelectSingleNode("//b/span[contains(text(),'" + textToFind + "')]")?.ParentNode?.ParentNode?.ParentNode;
            if (startOfFileSection == null) //Try alternate lookup mechanism. Sometimes it's a //b/span and sometimes its just a //b
                startOfFileSection = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.ParentNode;
            if (startOfFileSection != null)
            {
                HtmlNode urlNode = startOfFileSection.SelectSingleNode(".//a[contains(@href,'/files/')]");
                if (urlNode != null) //Sometimes there are no links to the documentation due to copyright and no <a> tag present
                    urls.Add(new IpdbUrl() { Name = urlNode.InnerText.CondenseHtml().ConvertHtmlToPlainText(), Url = urlNode.Attributes["href"].Value });

                //Now find all ROM urls. We stop when we have found a table row, whose first cell is not blank (meaning we went to another bold title of another row)
                var nextRow = doc.DocumentNode.SelectSingleNode("//b/span[contains(text(),'" + textToFind + "')]")
                    ?.ParentNode?.ParentNode?.ParentNode.NextSibling;
                if (nextRow == null) //Try alternate lookup mechanism. Sometimes it's a //b/span and sometimes its just a //b
                    nextRow = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")
                        ?.ParentNode?.ParentNode.NextSibling;

                if (nextRow != null)
                {
                    while (1 == 1)
                    {
                        if (nextRow == null || nextRow?.FirstChild?.FirstChild?.Name == "b")
                            break;
                        var url = nextRow.SelectSingleNode(".//a[contains(@href,'/files/')]");
                        if (url != null)
                        {
                            urls.Add(new IpdbUrl() { Name = url.InnerText.CondenseHtml().ConvertHtmlToPlainText(), Url = url.Attributes["href"].Value });
                            nextRow = nextRow.NextSibling;
                        }
                        else
                        {
                            nextRow = nextRow.NextSibling;
                        }
                    }
                }
            }
            if (urls.Count == 0)
                return null; //Prevent it from being serialized in the .json so it's smaller
            return urls;
        }

        private List<IpdbUrl> GetImageUrlsBySignificantNode(HtmlDocument doc, string textToFind)
        {
            List<IpdbUrl> urls = new List<IpdbUrl>();
            HtmlNode startOfFileSection = doc.DocumentNode.SelectSingleNode("//b/span[contains(text(),'" + textToFind + "')]")?.ParentNode?.ParentNode?.ParentNode;
            if (startOfFileSection == null) //Try alternate lookup mechanism. Sometimes it's a //b/span and sometimes its just a //b
                startOfFileSection = doc.DocumentNode.SelectSingleNode("//b[contains(text(),'" + textToFind + "')]")?.ParentNode?.ParentNode;
            if (startOfFileSection != null)
            {
                var urlNodes = startOfFileSection.SelectNodes(".//img[contains(@src,'/images/')]");
                if (urlNodes != null) 
                {
                    foreach(var urlNode in urlNodes) //The /tn_ is to remove the thumbnail version of hte image and just get the path to the full image
                        urls.Add(new IpdbUrl() { Name = urlNode.Attributes["alt"].Value?.Trim(), Url = urlNode.Attributes["src"].Value?.Replace("/tn_", "/") });
                }
            }
            if (urls.Count == 0)
                return null; //Prevent it from being serialized in the .json so it's smaller
            return urls;
        }
    }
}
