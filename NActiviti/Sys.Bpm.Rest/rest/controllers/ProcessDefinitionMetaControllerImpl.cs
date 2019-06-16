using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route("v1/process-definitions/{id}/meta")]
    [ApiController]
    public class ProcessDefinitionMetaControllerImpl : ControllerBase, IProcessDefinitionMetaController
    {
        private readonly IRepositoryService repositoryService;
        private readonly ProcessDefinitionMetaResourceAssembler resourceAssembler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repositoryService"></param>
        /// <param name="resourceAssembler"></param>
        public ProcessDefinitionMetaControllerImpl(IRepositoryService repositoryService, ProcessDefinitionMetaResourceAssembler resourceAssembler)
        {
            this.repositoryService = repositoryService;
            this.resourceAssembler = resourceAssembler;
        }

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}

        //public ProcessDefinitionMetaControllerImpl(IRepositoryService repositoryService, ProcessDefinitionMetaResourceAssembler resourceAssembler)
        //{
        //    this.repositoryService = repositoryService;
        //    this.resourceAssembler = resourceAssembler;
        //}


        /// <inheritdoc />
        [HttpGet]
        public virtual ProcessDefinitionMetaResource GetProcessDefinitionMetadata(string id)
        {
            IProcessDefinition processDefinition = repositoryService.CreateProcessDefinitionQuery().SetProcessDefinitionId(id).SingleResult();
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + id + "'");
            }

            IList<Process> processes = repositoryService.GetBpmnModel(id).Processes;
            ISet<ProcessDefinitionVariable> variables = new HashSet<ProcessDefinitionVariable>();
            ISet<string> users = new HashSet<string>();
            ISet<string> groups = new HashSet<string>();
            ISet<ProcessDefinitionUserTask> userTasks = new HashSet<ProcessDefinitionUserTask>();
            ISet<ProcessDefinitionServiceTask> serviceTasks = new HashSet<ProcessDefinitionServiceTask>();

            foreach (Process process in processes)
            {
                var vs = GetVariables(process).ToList();
                vs.ForEach(v => variables.Add(v));
                IList<FlowElement> flowElementList = (IList<FlowElement>)process.FlowElements;
                foreach (FlowElement flowElement in flowElementList)
                {
                    if (flowElement.GetType().Equals(typeof(UserTask)))
                    {
                        UserTask userTask = (UserTask)flowElement;
                        ProcessDefinitionUserTask task = new ProcessDefinitionUserTask(userTask.Name, userTask.Documentation);
                        userTasks.Add(task);
                        userTask.CandidateUsers.ToList().ForEach(u => users.Add(u));
                        userTask.CandidateGroups.ToList().ForEach(ug => groups.Add(ug));
                    }
                    if (flowElement.GetType().Equals(typeof(ServiceTask)))
                    {
                        ServiceTask serviceTask = (ServiceTask)flowElement;
                        ProcessDefinitionServiceTask task = new ProcessDefinitionServiceTask(serviceTask.Name, serviceTask.Implementation);
                        serviceTasks.Add(task);
                    }
                }
            }

            return resourceAssembler.ToResource(new ProcessDefinitionMeta(processDefinition.Id, processDefinition.Name, processDefinition.Description, processDefinition.Version, users, groups, variables, userTasks, serviceTasks));
        }


        /// <inheritdoc />
        private IList<ProcessDefinitionVariable> GetVariables(Process process)
        {
            IList<ProcessDefinitionVariable> variables = new List<ProcessDefinitionVariable>();
            if (process.ExtensionElements.Count > 0)
            {
                IEnumerator<IList<ExtensionElement>> it = process.ExtensionElements.Values.GetEnumerator();
                while (it.MoveNext())
                {
                    IList<ExtensionElement> extensionElementList = it.Current;
                    IEnumerator<ExtensionElement> it2 = extensionElementList.GetEnumerator();
                    while (it2.MoveNext())
                    {
                        ExtensionElement ee = it2.Current;
                        string name = ee.GetAttributeValue(ee.Namespace, "variableName");
                        string type = ee.GetAttributeValue(ee.Namespace, "variableType");
                        ProcessDefinitionVariable variable = new ProcessDefinitionVariable(name, type);
                        variables.Add(variable);
                    }
                }
            }
            return variables;
        }
    }

}