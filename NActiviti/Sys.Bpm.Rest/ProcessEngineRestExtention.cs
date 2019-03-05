using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.core.pageable.sort;
using org.activiti.cloud.services.events.listeners;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Services.Rest
{
    public static class ProcessEngineRestExtention
    {
        public static IMvcBuilder AddProcessEngineRestServices(this IMvcBuilder mvcBuilder, IConfiguration config)
        {
            mvcBuilder.AddMvcOptions(opts =>
            {
                opts.ModelBinderProviders.Insert(0, new PageableModelBinderProvider());
            });
            mvcBuilder.Services.AddTransient<ProcessInstanceSortApplier>();

            mvcBuilder.Services.AddSingleton<PageRetriever>();

            mvcBuilder.Services.AddTransient<PageableProcessInstanceRepositoryService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableProcessInstanceRepositoryService(sp.GetService<PageRetriever>(),
                    engine.RuntimeService,
                    sp.GetService<ProcessInstanceSortApplier>(),
                    sp.GetService<ProcessInstanceConverter>());
            });

            mvcBuilder.Services.AddTransient<ListConverter>();

            mvcBuilder.Services.AddTransient<TaskConverter>(sp => new TaskConverter(sp.GetService<ListConverter>()));

            mvcBuilder.Services.AddTransient<TaskSortApplier>();

            //mvcBuilder.Services.AddTransient<MessageProducerActivitiEventListener>();

            mvcBuilder.Services.AddTransient<PageableTaskRepositoryService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableTaskRepositoryService(engine.TaskService, sp.GetService<TaskConverter>(), sp.GetService<PageRetriever>(), sp.GetService<TaskSortApplier>());
            });

            mvcBuilder.Services.AddTransient<ProcessInstanceConverter>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new ProcessInstanceConverter(sp.GetService<ListConverter>());
            });

            mvcBuilder.Services.AddTransient<ProcessInstanceResourceAssembler>();


            mvcBuilder.Services.AddTransient<ProcessEngineWrapper>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                IHttpContextAccessor httpContext = sp.GetService<IHttpContextAccessor>();

                return new ProcessEngineWrapper(sp.GetService<ProcessInstanceConverter>(),
                    engine.RuntimeService,
                    sp.GetService<PageableProcessInstanceRepositoryService>(),
                    engine.TaskService,
                    sp.GetService<TaskConverter>(),
                    sp.GetService<PageableTaskRepositoryService>(),
                    null,
                    null,
                    engine.RepositoryService,
                    null,
                    null);
            });

            mvcBuilder.Services
                .AddTransient<PageRetriever>()
                .AddTransient<ProcessDefinitionConverter>()
                .AddTransient<ProcessDefinitionSortApplier>()
                .AddTransient<SecurityPoliciesApplicationService>()
                .AddTransient<ProcessDefinitionResourceAssembler>()
                .AddTransient<DeploymentConverter>()
                .AddTransient<DeploymentSortApplier>()
                .AddTransient<PageableProcessDefinitionRepositoryService>()
                .AddTransient<PageableDeploymentRespositoryService>();

            mvcBuilder.Services.AddTransient<TaskVariableResourceAssembler>();

            mvcBuilder.Services.AddTransient<TaskResourceAssembler>();

            mvcBuilder.Services.AddTransient<ProcessInstanceVariableResourceAssembler>();

            mvcBuilder.Services.AddTransient<AuthenticationWrapper>();

            mvcBuilder.AddApplicationPart(typeof(IHomeController).Assembly);

            return mvcBuilder;
        }
    }
}
