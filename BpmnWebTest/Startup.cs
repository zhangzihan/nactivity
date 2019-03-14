﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sys;
using Sys.Bpm.Services.Rest;

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
            services.AddHttpContextAccessor();

            //注入流程引擎
            services.AddProcessEngine();

            services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(cors =>
                cors.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

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
