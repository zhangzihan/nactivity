using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api.resources;
using Sys.Workflow.cloud.services.rest.controllers;
using org.springframework.hateoas.mvc;
using System;

namespace Sys.Workflow.cloud.services.rest.assemblers
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
        public override ProcessVariableResource ToResource(ProcessInstanceVariable processInstanceVariable)
        {
            //throw new NotImplementedException();
            //Link processVariables = linkTo(methodOn(typeof(ProcessInstanceVariableControllerImpl)).getVariables(processInstanceVariable.ProcessInstanceId)).withRel("processVariables");
            //Link processInstance = linkTo(methodOn(typeof(ProcessInstanceControllerImpl)).getProcessInstanceById(processInstanceVariable.ProcessInstanceId)).withRel("processInstance");
            //Link homeLink = linkTo(typeof(HomeControllerImpl)).withRel("home");
            return new ProcessVariableResource(processInstanceVariable);//,processVariables,processInstance,homeLink);
        }
    }

}