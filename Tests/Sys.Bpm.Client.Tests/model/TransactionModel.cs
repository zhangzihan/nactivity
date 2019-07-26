//using Sys.Workflow;
//using Sys.Workflow.Bpmn.Converters;
//using Sys.Workflow.Bpmn.Models;
//using Sys.Workflow.Cloud.Services.Core.Commands;
//using Sys.Workflow.Cloud.Services.Rest.Api;
//using Sys.Workflow.Engine;
//using Sys.Workflow.Engine.Impl.Cfg;
//using Sys.Workflow.Engine.Impl.Cmd;
//using Sys.Workflow.Engine.Impl.Identities;
//using Sys.Workflow.Engine.Impl.Interceptor;
//using Sys.Workflow.Engine.Repository;
//using Sys.Workflow.Engine.Runtime;
//using Sys.Workflow.Engine.Tasks;
//using Sys.Workflow.Services.Api.Commands;
//using Sys.Workflow.Test;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using Xunit;

//namespace Sys.Bpm.Client.Tests.model
//{
//    public class TransactionModel
//    {
//        private readonly InProcessWorkflowEngine context;
//        private readonly IProcessEngine processEngine;

//        public TransactionModel()
//        {
//            context = new InProcessWorkflowEngine();
//            context.Create();
//            processEngine = ProcessEngineFactory.Instance.GetProcessEngine("InProcess");
//        }


//        //[Theory]
//        [InlineData("事务块.bpmn")]
//        public void 执行事务子流程(string bpmnFile)
//        {
//            string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

//            ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

//            Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
//            {
//                Id = "评审员",
//                FullName = "评审员",
//                TenantId = context.TenantId
//            };

//            IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
//                .Name(Path.GetFileNameWithoutExtension(bpmnFile))
//                .TenantId(context.TenantId)
//                .AddString(bpmnFile, xml)
//                .EnableDuplicateFiltering()
//                .TenantId(context.TenantId);

//            IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

//            IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
//                .SetDeploymentId(deploy.Id)
//                .SetProcessDefinitionTenantId(context.TenantId)
//                .SetLatestVersion()
//                .SingleResult();

//            IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, null));

//            IList<ITask> tasks = processEngine.TaskService.GetMyTasks("评审员");
//            commandExecutor.Execute(new CompleteTaskCmd(tasks[0].Id, null));
//        }


//        //[Theory]
//        [InlineData("主流程.bpmn")]
//        public void 商品审核流程(string bpmnFile)
//        {
//            //string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

//            ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

//            Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
//            {
//                Id = "评审员",
//                FullName = "评审员",
//                TenantId = context.TenantId
//            };

//            //IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
//            //    .Name(Path.GetFileNameWithoutExtension(bpmnFile))
//            //    .TenantId(context.TenantId)
//            //    .AddString(bpmnFile, xml)
//            //    .EnableDuplicateFiltering()
//            //    .TenantId(context.TenantId);

//            //IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

//            var defs = processEngine.RepositoryService.CreateProcessDefinitionQuery()
//                .SetProcessDefinitionKey("Process_9PoKARBVU")
//                .SetProcessDefinitionTenantId(context.TenantId)
//                .SetLatestVersion()
//                .List();

//            IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
//                .SetProcessDefinitionKey("Process_9PoKARBVU")
//                .SetProcessDefinitionTenantId(context.TenantId)
//                .SetLatestVersion()
//                .SingleResult();

//            IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, new WorkflowVariable
//            {
//                { "商品s", new string[]{ "1", "2"} }
//            }));

//            IList<ITask> tasks = processEngine.TaskService.GetMyTasks("评审员");
//            commandExecutor.Execute(new CompleteTaskCmd(tasks[0].Id, null));
//        }
//    }
//}
