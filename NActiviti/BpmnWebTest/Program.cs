using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using Sys.Bpm;
using System.Reflection;
using Microsoft.Extensions.Logging;
using App.Metrics.AspNetCore;
using App.Metrics;

namespace BpmnWebTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;

            IConfiguration cr = new ConfigurationBuilder()
                    .SetBasePath(root)
                    .AddJsonFile("hosting.json", optional: true)
                    .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(cr)
                .ConfigurationWorkflowWebHost()
                .ConfigureLogging((host, logging) =>
                {
                    logging.AddFile("bin/Logs/nactiviti-{Date}.json", LogLevel.Error, isJson: true);
                })
                //.ConfigureMetricsWithDefaults(
                //    builder =>
                //    {
                //        //builder.Report.ToConsole(TimeSpan.FromSeconds(2));
                //        //builder.Report.ToTextFile(Path.Combine(Directory.GetCurrentDirectory(), "logs", "metrics.txt"), TimeSpan.FromSeconds(20));
                //    })
                //.UseMetrics()
                //.UseMetricsWebTracking()
                .UseStartup<Startup>();
        }
    }
}
