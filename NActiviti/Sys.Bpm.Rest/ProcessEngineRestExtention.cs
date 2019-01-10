using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            mvcBuilder.Services.AddTransient<ProcessInstanceSortApplier>();

            mvcBuilder.Services.AddSingleton<PageRetriever>();

            mvcBuilder.Services.AddTransient<PageableProcessInstanceService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableProcessInstanceService(sp.GetService<PageRetriever>(),
                    engine.RuntimeService,
                    sp.GetService<ProcessInstanceSortApplier>(),
                    sp.GetService<ProcessInstanceConverter>());
            });

            mvcBuilder.Services.AddTransient<ListConverter>();

            mvcBuilder.Services.AddTransient<TaskConverter>(sp => new TaskConverter(sp.GetService<ListConverter>()));

            mvcBuilder.Services.AddTransient<TaskSortApplier>();

            //mvcBuilder.Services.AddTransient<MessageProducerActivitiEventListener>();

            mvcBuilder.Services.AddTransient<PageableTaskService>(sp =>
            {
                IProcessEngine engine = sp.GetService<IProcessEngine>();

                return new PageableTaskService(engine.TaskService, sp.GetService<TaskConverter>(), sp.GetService<PageRetriever>(), sp.GetService<TaskSortApplier>());
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
                    sp.GetService<PageableProcessInstanceService>(),
                    engine.TaskService,
                    sp.GetService<TaskConverter>(),
                    sp.GetService<PageableTaskService>(),
                    null,
                    null,
                    engine.RepositoryService,
                    null,
                    null);
            });

            mvcBuilder.Services.AddTransient<ProcessDefinitionResourceAssembler>()
                .AddTransient<PageableRepositoryService>(sp =>
                {
                    var pe = sp.GetService<IProcessEngine>();
                    return new PageableRepositoryService(pe.RepositoryService,
                        new PageRetriever(),
                        new ProcessDefinitionConverter(new ListConverter()),
                        new ProcessDefinitionSortApplier(),
                        new SecurityPoliciesApplicationService());
                });

            mvcBuilder.AddApplicationPart(typeof(IHomeController).Assembly);

            return mvcBuilder;
        }
    }
}
