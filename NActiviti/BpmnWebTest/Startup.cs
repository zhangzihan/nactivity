using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.Services.Rest;
using Sys.Workflow;
using Sys.Workflow.Options;
using App.Metrics.AspNetCore;
using App.Metrics;
using BpmnWebTest.Hubs;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Sys.Workflow.Cloud.Services.Core;

namespace BpmnWebTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var metrics = AppMetrics.CreateDefaultBuilder().Build(); // configure a reporter

            //services.AddMetrics(metrics);
            //services.AddMetricsReportingHostedService();
            //services.AddMetricsTrackingMiddleware();

            services.AddSingleton<WorkflowDebuggerEventListenerProvider>();

            services.AddCors(opts =>
            {
                opts.AddPolicy("WorkflowCorsPolicy", builder =>
                {
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                    builder.SetIsOriginAllowed(_ => true);
                    builder.AllowCredentials();
                    builder.Build();
                });
            });

            services.AddHttpContextAccessor();

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            //注入流程引擎
            services.AddProcessEngine(Configuration);

            services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
            //.AddMetrics();

#if DEBUG
            services.AddOpenApiDocument(doc =>
            {
                doc.DocumentName = "Workflow OpenAPI v1";
                doc.Title = "工作流API";
            });
            services.AddSwaggerDocument(doc =>
            {
                doc.DocumentName = "Workflow Swagger v1";
                doc.Title = "工作流API";
            });
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("WorkflowCorsPolicy");

            app.UseProcessEngine(lifetime);

            //app.UseMetricsAllMiddleware();

            app.UseStaticFiles();

            app.UseSignalRError();

            app.UseWorkflow();

            var wsOpts = new WebSocketOptions();
            wsOpts.AllowedOrigins.Add("*");
            wsOpts.ReceiveBufferSize = 65535;

            app.UseWebSockets(wsOpts);

            app.UseSignalR(routes =>
            {
                routes.MapHub<WorkflowDebuggerHub>("/api/v1/debugger");
            });

            app.UseAuthentication();

            app.UseMvc();

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUi3();
#endif
        }
    }
}
