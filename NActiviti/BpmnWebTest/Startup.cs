using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sys.Bpm.Services.Rest;
using Sys.Workflow;
using Sys.Workflow.Options;
using App.Metrics.AspNetCore;
using App.Metrics;

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

            services.AddHttpContextAccessor();

            //注入流程引擎
            services.AddProcessEngine(Configuration);

            services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                //.AddMetrics();

            services.AddCors();

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

            app.UseProcessEngine(lifetime);

            app.UseCors(cors =>
                cors.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

            //app.UseMetricsAllMiddleware();

            app.UseStaticFiles();

            app.UseWorkflow();

            app.UseMvc();

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUi3();
#endif
        }
    }
}
