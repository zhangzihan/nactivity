using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessInstanceController
    {
        PagedResources<ProcessInstanceResource> getProcessInstances(Pageable pageable);

        Resource<ProcessInstance> startProcess(StartProcessInstanceCmd cmd);

        Resource<ProcessInstance> getProcessInstanceById(string processInstanceId);

        string getProcessDiagram(string processInstanceId);

        IActionResult sendSignal(SignalCmd cmd);

        IActionResult suspend(string processInstanceId);

        IActionResult activate(string processInstanceId);

        void deleteProcessInstance(string processInstanceId);
    }

}