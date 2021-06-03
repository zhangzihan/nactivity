using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Core.Pageables.Sorts;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Engine;
using Serilog;
using System.Linq;
using Sys.Workflow.Contexts;
using Microsoft.AspNetCore.Authorization;
using Sys.Workflow.Cloud.Services.Rest.Api;

namespace Sys.Workflow.Services.Rest
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineRestExtention
    {
        /// <summary>
        /// 注册Workflow Rest服务
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IMvcBuilder AddProcessEngineRestServices(this IMvcBuilder mvcBuilder, IConfiguration config)
        {
            IServiceCollection services = mvcBuilder.Services;

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(WorkflowConstants.WORKFLOW_AUTHORIZE_POLICY, policy =>
                {
                    policy.Requirements.Add(new InternaWorkflowAuthorizationRequirement());
                });
            });

            services.AddSingleton<IAuthorizationHandler, InternalWorkflowAuthorizationHandler>();

            services.UseInMemoryBus();

            mvcBuilder.AddMvcOptions(opts =>
            {
#if !NETCORE3
                JsonOutputFormatter jsonFormatter = opts.OutputFormatters.FirstOrDefault(x => x.GetType() == typeof(JsonOutputFormatter)) as JsonOutputFormatter;

                if (jsonFormatter != null)
                {
                    jsonFormatter.PublicSerializerSettings.ReferenceLoopHandling =
                        ReferenceLoopHandling.Ignore;
                }
#endif
                opts.ModelBinderProviders.Insert(0, new PageableModelBinderProvider());
#if NETCORE3
                SystemTextJsonOutputFormatter jsonFormatter = opts.OutputFormatters.FirstOrDefault(x => x.GetType() == typeof(SystemTextJsonOutputFormatter)) as SystemTextJsonOutputFormatter;
                opts.EnableEndpointRouting = false;
#endif
            });

#if NETCORE3
            mvcBuilder.AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
#endif

            services.AddTransient<ProcessInstanceSortApplier>();

            services.AddSingleton<PageRetriever>();

            services.AddTransient<HistoricInstanceConverter>();

            services.AddTransient<HistoryInstanceSortApplier>();

            services.AddTransient<PageableProcessHistoryRepositoryService>();

            services.AddTransient<PageableProcessInstanceRepositoryService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableProcessInstanceRepositoryService(sp.GetService<PageRetriever>(),
                    engine.RuntimeService,
                    sp.GetService<ProcessInstanceSortApplier>(),
                    sp.GetService<ProcessInstanceConverter>(),
                    sp.GetService<ILoggerFactory>());
            });

            services.AddTransient<ListConverter>();

            services.AddTransient<TaskConverter>(sp => new TaskConverter(sp.GetService<ListConverter>()));

            services.AddTransient<HistoricTaskInstanceConverter>(sp => new HistoricTaskInstanceConverter(sp.GetService<ListConverter>()));

            services.AddTransient<TaskSortApplier>();

            services.AddTransient<HistoryTaskSortApplier>();

            //services.AddTransient<MessageProducerActivitiEventListener>();

            services.AddTransient<PageableTaskRepositoryService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableTaskRepositoryService(
                    engine.TaskService,
                    sp.GetService<TaskConverter>(),
                    sp.GetService<HistoricTaskInstanceConverter>(),
                    sp.GetService<IHistoryService>(),
                    sp.GetService<PageRetriever>(),
                    sp.GetService<TaskSortApplier>(),
                    sp.GetService<HistoryTaskSortApplier>());
            });

            services.AddTransient<ProcessInstanceConverter>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new ProcessInstanceConverter(sp.GetService<ListConverter>());
            });

            services.AddTransient<ProcessInstanceResourceAssembler>();

            services.AddScoped<ProcessEngineWrapper>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                IHttpContextAccessor httpContext = sp.GetService<IHttpContextAccessor>();

                return new ProcessEngineWrapper(sp.GetService<ProcessInstanceConverter>(),
                    sp.GetService<PageableProcessInstanceRepositoryService>(),
                    sp.GetService<TaskConverter>(),
                    sp.GetService<PageableTaskRepositoryService>(),
                    null,
                    sp.GetService<SecurityPoliciesApplicationService>(),
                    null,
                    sp.GetService<IApplicationEventPublisher>(),
                    engine,
                    sp.GetService<HistoricInstanceConverter>(),
                    sp.GetService<ILoggerFactory>());
            });

            services.AddScoped<SecurityPoliciesApplicationService>();

            services
                .AddTransient<PageRetriever>()
                .AddTransient<ProcessDefinitionConverter>()
                .AddTransient<ProcessDefinitionSortApplier>()
                .AddTransient<ProcessDefinitionResourceAssembler>()
                .AddTransient<ProcessDefinitionMetaResourceAssembler>()
                .AddTransient<DeploymentConverter>()
                .AddTransient<DeploymentSortApplier>()
                .AddTransient<PageableProcessDefinitionRepositoryService>()
                .AddTransient<PageableDeploymentRespositoryService>();

            services.AddTransient<TaskVariableResourceAssembler>();

            services.AddTransient<TaskResourceAssembler>();

            services.AddTransient<ProcessInstanceVariableResourceAssembler>();

            services.AddTransient<AuthenticationWrapper>();

            services.AddTransient<IMvcControllerDiscovery, MvcControllerDiscovery>();

            mvcBuilder.AddApplicationPart(typeof(ProcessEngineRestExtention).Assembly);

            return mvcBuilder;
        }

        /// <summary>
        /// 挂接异常处理中间件和安全校验Token中间件,app.UseWorkflow需要写在app.UseMvc之前
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWorkflow(this IApplicationBuilder app)
        {
            IConfiguration config = app.ApplicationServices.GetService<IConfiguration>();

            SecurityPoliciesProviderOptions options = new SecurityPoliciesProviderOptions(config.GetSection("SecurityPoliciesProvider"));

            _ = app.UseMiddleware<SecurityPoliciesApplicationMiddle>(Microsoft.Extensions.Options.Options.Create(options));

            string enableErrorHandling = config["applicationSettings:workflow:enableErrorHandling"];
            if (!bool.TryParse(enableErrorHandling, out var b) || !b)
            {
                app.UseMiddleware<ErrorHandlingMiddleware>();
            }

            return app;
        }
    }
}
