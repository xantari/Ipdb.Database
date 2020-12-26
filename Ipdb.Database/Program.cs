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
            scraper.EnableRandomSleepTime = false; //Try do it as fast as possible.
            //string finalFileToSaveTo = "C:\\TFS\\Ipdb.Database\\Ipdb.Database\\Database\\ipdbdatabase.json";
            //string tempFileToSaveTo = "C:\\TFS\\Ipdb.Database\\Ipdb.Database\\Database\\ipdbdatabasetemp.json";

            //database = JsonConvert.DeserializeObject<IpdbDatabase>(File.ReadAllText(tempFileToSaveTo));

            //database = scraper.ScrapeAllResume(database, tempFileToSaveTo, 4001, 10000);
            //var oneResult = scraper.Scrape(1090);
            //var result = scraper.ScrapeAll(fileToSaveTo, 750, 800);
            //var result = scraper.ScrapeAll(tempFileToSaveTo);

            //JsonSerializer serializer = new JsonSerializer();
            //serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            //serializer.NullValueHandling = NullValueHandling.Ignore;
            //serializer.Formatting = Formatting.Indented;
            ////serializer.Error += Serializer_Error; //Ignore errors
            //using (StreamWriter sw = new StreamWriter(finalFileToSaveTo, false))
            //using (JsonWriter writer = new JsonTextWriter(sw))
            //{
            //    serializer.Serialize(writer, database);
            //}

            Log.Information("Scraping Finished.");
        }
    }
}
