using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessInstanceController
    {
        Task<PagedResources<ProcessInstanceResource>> GetProcessInstances(Pageable pageable);

        Task<ProcessInstance> Start(StartProcessInstanceCmd cmd);

        Task<Resource<ProcessInstance>> GetProcessInstanceById(string processInstanceId);

        Task<string> GetProcessDiagram(string processInstanceId);

        Task<IActionResult> SendSignal(SignalCmd cmd);

        Task<IActionResult> Suspend(string processInstanceId);

        Task<IActionResult> Activate(string processInstanceId);

        System.Threading.Tasks.Task DeleteProcessInstance(string processInstanceId);
    }

}