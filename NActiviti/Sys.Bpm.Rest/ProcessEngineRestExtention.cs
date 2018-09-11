using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.core.pageable.sort;
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
        public static IMvcBuilder AddProcessEngineResServices(this IMvcBuilder mvcBuilder, IConfiguration config)
        {
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
