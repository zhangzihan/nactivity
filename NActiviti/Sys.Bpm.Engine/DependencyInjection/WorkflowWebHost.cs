///////////////////////////////////////////////////////////
//  WorkflowWebHost.cs
//  Created on:      19-2月-2019 8:32:00
//  Original author: 张楠
///////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Sys.Workflow.Options;
using System;
using System.IO;

namespace Sys.Bpm
{
    /// <summary>
    /// 工作流主机
    /// </summary>
    public static class WorkflowWebHost
    {
        /// <summary>
        /// 加载工作流主机配置文件
        /// </summary>
        /// <param name="webHostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder ConfigurationWorkflowWebHost(this IWebHostBuilder webHostBuilder)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;

            string jsonFile = Path.Combine(root, "resources", "activiti.cfg.json");

            string appName = new ConfigurationBuilder().AddJsonFile(jsonFile).Build().GetValue<string>("applicationName");

            if (!string.IsNullOrWhiteSpace(appName))
            {
                string appKey = webHostBuilder.GetSetting(WebHostDefaults.ApplicationKey);

                webHostBuilder.UseSetting(WebHostDefaults.ApplicationKey, appName);
            }

            return webHostBuilder.ConfigureAppConfiguration((ctx, builder) =>
            {
                ProcessEngineOption.ConfigFileName = jsonFile;

                builder.AddJsonFile(new PhysicalFileProvider(Path.Combine(root, "resources")), "activiti.cfg.json", false, true);
            });
        }
    }
}
