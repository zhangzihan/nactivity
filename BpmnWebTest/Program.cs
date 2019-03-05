using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using Sys.Bpm;

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
            string root = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.CodeBase).AbsolutePath);

            IConfiguration cr = new ConfigurationBuilder()
                    .SetBasePath(root)
                    .AddJsonFile("hosting.json", optional: true)
                    .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(cr)
                .ConfigurationWorkflowWebHost()
                .UseStartup<Startup>();
        }
    }
}
