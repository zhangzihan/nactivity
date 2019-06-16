using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Controllers;
using Sys.Workflow.Hateoas.Mvc;
using System;

namespace Sys.Workflow.Cloud.Services.Rest.Assemblers
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