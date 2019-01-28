using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
                    .AddJsonFile("Resources\\activiti.cfg.json", true, true)
                    .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(cr)
                .UseStartup<Startup>();
        }
    }
}
