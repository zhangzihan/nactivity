using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sys.Workflow;
using System;

namespace BpmnWebTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "工作流";

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
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", false, true);
                    config.AddJsonFile("appsettings." + builderContext.HostingEnvironment.EnvironmentName + ".json", false, true);
                })
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
                    config.Limits.MaxRequestHeaderCount = 200;
                    config.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
                    config.Limits.Http2.MaxStreamsPerConnection = 1000;
                    config.Limits.MaxConcurrentConnections = null;
                    config.Limits.MinRequestBodyDataRate = null;
                    config.Limits.MinResponseDataRate = null;
                    //config.ApplicationSchedulingMode = SchedulingMode.ThreadPool;
                })
                .UseIIS()
                .UseIISIntegration()
                .UseStartup<Startup>();
        }
    }
}
