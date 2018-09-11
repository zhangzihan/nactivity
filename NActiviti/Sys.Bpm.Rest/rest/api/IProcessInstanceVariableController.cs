using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessInstanceVariableController
    {
        Resources<ProcessVariableResource> getVariables(string processInstanceId);

        Resources<ProcessVariableResource> getVariablesLocal(string processInstanceId);

        IActionResult setVariables(string processInstanceId, SetProcessVariablesCmd setTaskVariablesCmd);

        IActionResult removeVariables(string processInstanceId, RemoveProcessVariablesCmd removeProcessVariablesCmd);
    }
}