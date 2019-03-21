using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.controllers;
using org.springframework.hateoas.mvc;
using System;

namespace org.activiti.cloud.services.rest.assemblers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessInstanceVariableResourceAssembler : ResourceAssemblerSupport<ProcessInstanceVariable, ProcessVariableResource>
    {

        /// <summary>
        /// 
        /// </summary>
        public ProcessInstanceVariableResourceAssembler() : base(typeof(ProcessInstanceVariableControllerImpl), typeof(ProcessVariableResource))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceVariable"></param>
        /// <returns></returns>
        public override ProcessVariableResource toResource(ProcessInstanceVariable processInstanceVariable)
        {
            //throw new NotImplementedException();
            //Link processVariables = linkTo(methodOn(typeof(ProcessInstanceVariableControllerImpl)).getVariables(processInstanceVariable.ProcessInstanceId)).withRel("processVariables");
            //Link processInstance = linkTo(methodOn(typeof(ProcessInstanceControllerImpl)).getProcessInstanceById(processInstanceVariable.ProcessInstanceId)).withRel("processInstance");
            //Link homeLink = linkTo(typeof(HomeControllerImpl)).withRel("home");
            return new ProcessVariableResource(processInstanceVariable);//,processVariables,processInstance,homeLink);
        }
    }

}