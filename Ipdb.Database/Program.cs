using Ipdb.Models;
using Ipdb.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace Ipdb.Database
{
    class Program
    {
        static void Main(string[] args)
        {
            //var env = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                //.AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();
            var config = builder.Build();

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

            var cfg = config.Get<AppSettings>();

            try
            {
                if (args.Any(p => p.Contains("-resume")))
                {
                    if (File.Exists(cfg.TempFileLocation))
                    {
                        database = JsonConvert.DeserializeObject<IpdbDatabase>(File.ReadAllText(cfg.TempFileLocation));
                        //Find where we left off.
                        var lastIpdb = database.Data.Max(c => c.IpdbId) + 1;
                        database = scraper.ScrapeAllResume(database, cfg.TempFileLocation, lastIpdb, 10000);
                    }
                    else
                    {
                        Log.Information("Temp file not found: {file}", cfg.TempFileLocation);
                        return;
                    }

                    SaveDatabase(cfg.FinalFileLocation, database);

                    //Delete the temp file
                    if (File.Exists(cfg.TempFileLocation))
                        File.Delete(cfg.TempFileLocation);
                }
                else //Full scraping
                {
                    database = scraper.ScrapeAll(cfg.TempFileLocation);

                    SaveDatabase(cfg.FinalFileLocation, database);

                    //Delete the temp file
                    if (File.Exists(cfg.TempFileLocation))
                        File.Delete(cfg.TempFileLocation);
                }
            }
            catch (Exception ex)
            {
                Log.Error("{error}", ex);
                throw;
            }

            Log.Information("Scraping Finished.");
        }

        private static void SaveDatabase(string location, IpdbDatabase database)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            //serializer.Error += Serializer_Error; //Ignore errors
            using (StreamWriter sw = new StreamWriter(location, false))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, database);
            }
        }
    }
}
