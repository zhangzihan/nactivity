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
    public class TaskVariableResourceAssembler : ResourceAssemblerSupport<TaskVariable, TaskVariableResource>
    {

        /// <summary>
        /// 
        /// </summary>
        public TaskVariableResourceAssembler() : base(typeof(TaskVariableControllerImpl), typeof(TaskVariableResource))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskVariable"></param>
        /// <returns></returns>
        public override TaskVariableResource ToResource(TaskVariable taskVariable)
        {
            throw new NotImplementedException();
            //Link globalVariables = linkTo(methodOn(typeof(TaskVariableControllerImpl)).getVariables(taskVariable.TaskId)).withRel("globalVariables");
            //Link localVariables = linkTo(methodOn(typeof(TaskVariableControllerImpl)).getVariablesLocal(taskVariable.TaskId)).withRel("localVariables");
            //Link taskRel = linkTo(methodOn(typeof(TaskControllerImpl)).getTaskById(taskVariable.TaskId)).withRel("task");
            //Link homeLink = linkTo(typeof(HomeControllerImpl)).withRel("home");
            //return new TaskVariableResource(taskVariable,globalVariables,localVariables,taskRel,homeLink);
        }
    }

}