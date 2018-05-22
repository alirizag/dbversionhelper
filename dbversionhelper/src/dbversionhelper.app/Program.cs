using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Configuration;
namespace dbversionhelper.app
{
    public class Program
    {
        #region | Declaraitons |
        public static IConfiguration Configuration { get; set; }
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Db migration started..");


            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string CONNECTION_STRING = Configuration["AppSettings:CONNECTION_STRING"];

            var watch = new Stopwatch();
            watch.Start();


            //database validation
            EnsureDatabase.For.SqlDatabase(CONNECTION_STRING);

            var upgrader = DeployChanges.To
                .SqlDatabase(CONNECTION_STRING)
                .JournalToSqlTable("dbo", "ScriptJournals")
                .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransactionPerScript()
                .LogToConsole()
                .WithExecutionTimeout(new TimeSpan(0, 0, 90))
                .Build();


            var result = upgrader.PerformUpgrade();

            watch.Stop();
            Display("SqlServer", result, watch.Elapsed);
        }

        static void Display(string dbType, DatabaseUpgradeResult result, TimeSpan ts)
        {
            // Display the result
            if (result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success!");
                Console.WriteLine("{0} Database Upgrade Runtime: {1}", dbType,
                    $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
                Console.ReadKey();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ReadKey();
                Console.WriteLine("Failed!");
            }
        }
    }

}
