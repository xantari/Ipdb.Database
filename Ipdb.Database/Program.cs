using Ipdb.Models;
using Ipdb.Utilities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;

namespace Ipdb.Database
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Log\\Log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting IPDB Database Creator.");

            UserAgentStrings.InitializeList("Data\\UserAgentStrings.txt");

            Log.Information("User Agent strings initialized.");

            var database = new IpdbDatabase();

            var scraper = new IpdbScraper();

            //var oneResult = scraper.Scrape(1);

            var result = scraper.ScrapeAll();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            //serializer.Error += Serializer_Error; //Ignore errors
            using (StreamWriter sw = new StreamWriter("C:\\TFS\\Ipdb.Database\\Ipdb.Database\\Database\\ipdbdatabase.json", false))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }

            Log.Information("Scraping Finished.");
        }
    }
}
