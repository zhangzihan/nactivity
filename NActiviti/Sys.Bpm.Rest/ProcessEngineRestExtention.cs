using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.core.pageable.sort;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using Sys.Net.Http;
using System.Net.Http;

namespace Sys.Bpm.Services.Rest
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

            mvcBuilder.AddMvcOptions(opts =>
            {
                opts.ModelBinderProviders.Insert(0, new PageableModelBinderProvider());
            });
            services.AddTransient<ProcessInstanceSortApplier>();

            services.AddSingleton<PageRetriever>();

            services.AddTransient<HistoricInstanceConverter>();

            services.AddTransient<HistoryInstanceSortApplier>();

            services.AddTransient<PageableProcessHistoryRepositoryService>(sp =>
            {
                return new PageableProcessHistoryRepositoryService(
                    sp.GetService<PageRetriever>(),
                    sp.GetService<IProcessEngine>().HistoryService,
                    sp.GetService<HistoryInstanceSortApplier>(),
                    sp.GetService<HistoricInstanceConverter>(),
                    sp.GetService<SecurityPoliciesApplicationService>()
                    );
            });

            services.AddTransient<PageableProcessInstanceRepositoryService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableProcessInstanceRepositoryService(sp.GetService<PageRetriever>(),
                    engine.RuntimeService,
                    sp.GetService<ProcessInstanceSortApplier>(),
                    sp.GetService<ProcessInstanceConverter>());
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
                    null,
                    engine,
                    sp.GetService<HistoricInstanceConverter>());
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

            mvcBuilder.AddApplicationPart(typeof(IHomeController).Assembly);

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

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseMiddleware<SecurityPoliciesApplicationMiddle>(Options.Create(options));

            return app;
        }
    }
}
