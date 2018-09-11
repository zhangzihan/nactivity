using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api
{
    public interface ITaskVariableController
    {
        Resources<TaskVariableResource> getVariables(string taskId);

        Resources<TaskVariableResource> getVariablesLocal(string taskId);

        IActionResult setVariables(string taskId, SetTaskVariablesCmd setTaskVariablesCmd); //(required = true)

        IActionResult setVariablesLocal(string taskId, SetTaskVariablesCmd setTaskVariablesCmd);
    }
}