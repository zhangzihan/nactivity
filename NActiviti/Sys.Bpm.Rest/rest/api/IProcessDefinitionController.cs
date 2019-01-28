using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessDefinitionController
    {
        Task<IList<ProcessDefinitionResource>> GetLatestProcessDefinitions(Pageable pageable);

        Task<IList<ProcessDefinitionResource>> GetProcessDefinitions(Pageable pageable);

        ProcessDefinitionResource GetProcessDefinition(string id);

        Task<ContentResult> GetProcessModel(string id);

        string GetBpmnModel(string id);

        string GetProcessDiagram(string id);
    }
}