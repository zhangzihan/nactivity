using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using Sys.Workflow;
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
                .UseKestrel(config =>
                {
                    config.AllowSynchronousIO = true;
                    config.Limits.MaxConcurrentConnections = null;
                    config.Limits.MaxRequestHeaderCount = 200;
                    config.Limits.MinRequestBodyDataRate = null;
                    config.Limits.MinResponseDataRate = null;
                    config.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(3);
                    config.Limits.Http2.MaxStreamsPerConnection = 1000;
                })
                .UseStartup<Startup>();
        }
    }
}
