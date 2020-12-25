using Ipdb.Utilities;
using Serilog;
using System;

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

            UserAgentStrings.InitializeList("Data\\UserAgentStrings.txt");

            Console.WriteLine("Hello World!");
        }
    }
}
