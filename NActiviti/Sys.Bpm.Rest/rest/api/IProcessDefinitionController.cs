using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessDefinitionController
    {
        ProcessDefinitionResource GetProcessDefinitions(Pageable pageable);

        ProcessDefinitionResource GetProcessDefinition(string id);

        string GetProcessModel(string id);

        string GetBpmnModel(string id);

        string GetProcessDiagram(string id);
    }
}