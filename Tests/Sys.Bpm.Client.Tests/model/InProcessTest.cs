using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartSql.Abstractions;
using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Impl.Variable;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Sys.Workflow.Client.Tests.Models
{
	public class InProcessTest
	{
		private readonly InProcessWorkflowEngine context;
		private readonly IProcessEngine processEngine;

		public InProcessTest()
		{
			context = new InProcessWorkflowEngine();
			context.Create();

			ProcessEngineConfiguration processEngineConfig = new StandaloneProcessEngineConfiguration(
					new HistoryServiceImpl(),
					new TaskServiceImpl(),
					new DynamicBpmnServiceImpl(),
					new RepositoryServiceImpl(),
					new RuntimeServiceImpl(),
					new ManagementServiceImpl(),
					//sp.GetService<IAsyncExecutor>(),
					null,
					ProcessEngineServiceProvider.Resolve<IConfiguration>())
			{
				ProcessEngineName = "InProcess",
				AsyncExecutorActivate = false,
				DatabaseSchemaUpdate = "false",
				EnableVerboseExecutionTreeLogging = false,
				History = "audit"
			};

			processEngine = ProcessEngineFactory.CreateProcessEngine(processEngineConfig);
		}

		[InlineData(70)]
		[Theory]
		public void SmartSqlTest(int counter)
		{
			var ex = Record.Exception(() =>
			{
				ISmartSqlMapper sqlMapper = ProcessEngineServiceProvider.Resolve<ISmartSqlMapper>();
				var type = typeof(HistoricVariableInstanceEntityImpl);
				var dbFactory = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).DbSqlSessionFactory;
				var statement = dbFactory.GetBulkUpdateStatement(type, dbFactory.DatabaseType, ref type);
				HistoricVariableInstanceEntityImpl[] items = new HistoricVariableInstanceEntityImpl[counter];
				for (var idx = 0; idx < counter; idx++)
				{
					items[idx] = new HistoricVariableInstanceEntityImpl
					{
						Id = Guid.NewGuid().ToString(),
						Name = $"Name_{idx}",
						VariableType = new NullType(),
						Revision = idx,
					};
				}
				var reqContext = new RequestContext()
				{
					Scope = "Sys.Workflow.Engine.Impl.Persistence.Entity.HistoricVariableInstanceEntityImpl",
					SqlId = statement,
					Request = new
					{
						Items = items
					}
				};
				reqContext.Setup(sqlMapper.SmartSqlOptions.SmartSqlContext, sqlMapper.SqlBuilder);
				var dbsession = sqlMapper.BeginTransaction(reqContext, System.Data.IsolationLevel.RepeatableRead);
				var dbCommand = sqlMapper.SmartSqlOptions.PreparedCommand.Prepare(dbsession, reqContext);
				try
				{
					dbCommand.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					throw;
				}
				finally
				{
					dbsession.RollbackTransaction();
					dbCommand.Connection.Close();
				}
			});

		}

		[Theory]
		[InlineData("分支合并.bpmn")]
		public void 无条件分支合并_所有分支都完成后执行下一流程(string bpmnFile)
		{
			var ex = Record.Exception(() =>
			{
				string user1 = Guid.NewGuid().ToString();
				string user2 = Guid.NewGuid().ToString();
				string waiter = Guid.NewGuid().ToString();
				string bizId = Guid.NewGuid().ToString();

				WorkflowVariable vars = new WorkflowVariable();
				vars.AddAssignee(nameof(user1), user1)
					.AddAssignee(nameof(user2), user2)
					.AddAssignee(nameof(waiter), waiter);

				ProcessEngineConfigurationImpl processEngineConfig = processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl;

				ICommandExecutor commandExecutor = processEngineConfig.CommandExecutor;

				Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
				{
					Id = "评审员",
					FullName = "评审员",
					TenantId = context.TenantId
				};
				string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

				IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
					.Name(Path.GetFileNameWithoutExtension(bpmnFile))
					.TenantId(context.TenantId)
					.AddString(bpmnFile, xml)
					.EnableDuplicateFiltering()
					.TenantId(context.TenantId);

				IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

				IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
					.SetDeploymentId(deploy.Id)
					.SetProcessDefinitionTenantId(context.TenantId)
					.SetLatestVersion()
					.SingleResult();

				IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(new Cloud.Services.Api.Commands.StartProcessInstanceCmd()
				{
					ProcessDefinitionId = definition.Id,
					BusinessKey = bizId,
					Variables = vars
				}));

				var tasks = processEngine.TaskService.CreateTaskQuery().SetProcessInstanceId(processInstance.Id).List();
				var task = tasks.First();

				//commandExecutor.Execute(new CompleteTaskCmd(task.Id, vars));

				commandExecutor.Execute(new ReturnToActivityCmd(task.Id, "Task_085qd6k", "流程1", vars));
				tasks = processEngine.TaskService.CreateTaskQuery().SetProcessInstanceId(processInstance.Id).List();

				commandExecutor.Execute(new ReturnToActivityCmd(task.Id, "ExclusiveGateway_1y6gcmn", "并行", vars));
				tasks = processEngine.TaskService.CreateTaskQuery().SetProcessInstanceId(processInstance.Id).List();

				//var execution = processEngineConfig.ExecutionEntityManager.CreateChildExecution(processInstance as IExecutionEntity);
				//var task = processEngineConfig.TaskEntityManager.Create();
				//task.

				//ITask ntask = commandExecutor.Execute(new CreateNewTaskCmd("Task_0opugm1", "流程2", null, null, task.ExecutionId, null, task.TenantId));				

				tasks = processEngine.TaskService.CreateTaskQuery().SetProcessInstanceId(processInstance.Id).List();

				//IList<ITask> u2tasks = processEngine.TaskService.GetMyTasks(user2);

				//IList<ITask> u1tasks = processEngine.TaskService.GetMyTasks(user1);

				//commandExecutor.Execute(new CompleteTaskCmd(u1tasks[0].Id, vars));

				//commandExecutor.Execute(new CompleteTaskCmd(u2tasks[0].Id, vars));

				//u2tasks = processEngine.TaskService.GetMyTasks(user2);

				//u1tasks = processEngine.TaskService.GetMyTasks(user1); 

				//tasks = processEngine.TaskService.CreateTaskQuery().SetProcessInstanceId(processInstance.Id).List();

				//完成用户1的任务
				//_ = taskClient.CompleteTask(new CompleteTaskCmd
				//{
				//	Assignee = user1,
				//	BusinessKey = bizId,
				//	OutputVariables = WorkflowVariable.FromObject(new
				//	{
				//		完成 = true
				//	})
				//}).GetAwaiter().GetResult();

				//Assert.Empty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);

				////完成用户2的任务
				//_ = taskClient.CompleteTask(new CompleteTaskCmd
				//{
				//	Assignee = user2,
				//	BusinessKey = bizId,
				//	OutputVariables = WorkflowVariable.FromObject(new
				//	{
				//		完成 = true
				//	})
				//}).GetAwaiter().GetResult();

				//Assert.NotEmpty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);
			});

			Assert.Null(ex);
		}

		[Theory]
		[InlineData("指定节点启动.bpmn")]
		public void 指定节点启动(string bpmnFile)
		{
			var ex = Record.Exception(() =>
			{
				string uid1 = Guid.NewGuid().ToString();
				string uid2 = Guid.NewGuid().ToString();

				ProcessEngineConfigurationImpl processEngineConfig = processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl;

				ICommandExecutor commandExecutor = processEngineConfig.CommandExecutor;

				Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
				{
					Id = "评审员",
					FullName = "评审员",
					TenantId = context.TenantId
				};
				string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

				IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
					.Name(Path.GetFileNameWithoutExtension(bpmnFile))
					.TenantId(context.TenantId)
					.AddString(bpmnFile, xml)
					.EnableDuplicateFiltering()
					.TenantId(context.TenantId);

				IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

				IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
					.SetDeploymentId(deploy.Id)
					.SetProcessDefinitionTenantId(context.TenantId)
					.SetLatestVersion()
					.SingleResult();

				var variables = new WorkflowVariable();
				variables.AddAssignee("节点1", uid1)
				 .AddAssignee("节点2", uid2);

				IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(new Cloud.Services.Api.Commands.StartProcessInstanceCmd()
				{
					InitialFlowElementId = "Task_0a5war4",
					ProcessDefinitionId = definition.Id,
					Variables = variables
				}));

				IList<ITask> tasks = processEngine.TaskService.GetMyTasks(uid2);
				Assert.True(tasks.Count > 0);
			});

			Assert.Null(ex);
		}

		[Theory]
		[InlineData("角色组一人通过.bpmn")]
		public void 协同审批角色组一人通过(string bpmnFile)
		{
			var ex = Record.Exception(() =>
			{
				string uid = Guid.NewGuid().ToString();

				ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

				Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
				{
					Id = "评审员",
					FullName = "评审员",
					TenantId = context.TenantId
				};
				string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

				IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
					.Name(Path.GetFileNameWithoutExtension(bpmnFile))
					.TenantId(context.TenantId)
					.AddString(bpmnFile, xml)
					.EnableDuplicateFiltering()
					.TenantId(context.TenantId);

				IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

				IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
					.SetDeploymentId(deploy.Id)
					.SetProcessDefinitionTenantId(context.TenantId)
					.SetLatestVersion()
					.SingleResult();

				string next = Guid.NewGuid().ToString();
				WorkflowVariable variables = new WorkflowVariable();
				variables.Add("roles",
						JsonConvert.SerializeObject(new[]
						{
							new { id = uid, name ="角色1"},
							new { id = "65735bb8-9b42-4723-b135-4187a0072714", name="角色2"},
							new { id = "91ad9782-3eb1-48e7-b794-8fa8d95101b0", name="角色3"}
						}));
				variables.AddAssignee("next", next);

				IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, variables));

				IList<ITask> tasks = processEngine.TaskService.GetMyTasks(uid);

				processEngine.TaskService.Complete(tasks[0].Id, new Dictionary<string, object>
				{
					{ "同意", true}
				});

				tasks = processEngine.TaskService.GetMyTasks(next);
				Assert.True(tasks.Count == 1);
			});

			Assert.Null(ex);
		}

		[Theory]
		[InlineData("会签节点回退.bpmn")]
		public void 会签节点回退时到任意已完成节点终止当前节点(string bpmnFile)
		{
			string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

			ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

			Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
			{
				Id = "评审员",
				FullName = "评审员",
				TenantId = context.TenantId
			};

			IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
				.Name(Path.GetFileNameWithoutExtension(bpmnFile))
				.TenantId(context.TenantId)
				.AddString(bpmnFile, xml)
				.EnableDuplicateFiltering()
				.TenantId(context.TenantId);

			IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

			IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
				.SetDeploymentId(deploy.Id)
				.SetProcessDefinitionTenantId(context.TenantId)
				.SetLatestVersion()
				.SingleResult();

			string[] users = new string[]
			{
				Guid.NewGuid().ToString(),
				Guid.NewGuid().ToString()
			};

			IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, new Dictionary<string, object>
			{
				{ "users", users }
			}));

			try
			{
				IList<ITask> tasks = processEngine.TaskService.GetMyTasks("节点1");

				ITaskEntity task = tasks[0] as ITaskEntity;
				IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

				processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
				{
					{ "流程变量", "变量值" }
				});

				tasks = processEngine.TaskService.GetMyTasks("节点2");
				task = tasks[0] as ITaskEntity;
				processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
				{
					{ "流程变量", "变量值" }
				});

				var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

				tasks = processEngine.TaskService.GetMyTasks(users[0]);
				task = tasks[0] as ITaskEntity;

				commandExecutor.Execute(new ReturnToActivityCmd(task.Id, list[0].Id, $"回退测试到{list[0].Name}"));
			}
			catch (Exception ex)
			{

			}

			System.Diagnostics.Debugger.Break();
		}

		[Theory]
		[InlineData("回退到任意已完成节点.bpmn")]
		public void 回退到任务已完成节点(string bpmnFile)
		{
			string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

			ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

			Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
			{
				Id = "评审员",
				FullName = "评审员",
				TenantId = context.TenantId
			};

			IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
				.Name(Path.GetFileNameWithoutExtension(bpmnFile))
				.TenantId(context.TenantId)
				.AddString(bpmnFile, xml)
				.EnableDuplicateFiltering()
				.TenantId(context.TenantId);

			IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

			IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
				.SetDeploymentId(deploy.Id)
				.SetProcessDefinitionTenantId(context.TenantId)
				.SetLatestVersion()
				.SingleResult();
			_ = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, null));

			IList<ITask> tasks = processEngine.TaskService.GetMyTasks("节点1");

			ITaskEntity task = tasks[0] as ITaskEntity;
			IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

			processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
				{
					{ "流程变量", "变量值" }
				});

			tasks = processEngine.TaskService.GetMyTasks("节点2");
			task = tasks[0] as ITaskEntity;
			processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
				{
					{ "流程变量", "变量值" }
				});

			var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

			tasks = processEngine.TaskService.GetMyTasks("节点3");
			task = tasks[0] as ITaskEntity;

			commandExecutor.Execute(new ReturnToActivityCmd(task.Id, list[0].Id, $"回退测试到{list[0].Name}"));
		}

		[Theory]
		[InlineData("边界信号事件.bpmn")]
		public void 获取已完成流程节点(string bpmnFile)
		{
			string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

			ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

			Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
			{
				Id = "评审员",
				FullName = "评审员",
				TenantId = context.TenantId
			};

			IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
				.Name(Path.GetFileNameWithoutExtension(bpmnFile))
				.TenantId(context.TenantId)
				.AddString(bpmnFile, xml)
				.EnableDuplicateFiltering()
				.TenantId(context.TenantId);

			IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

			IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
				.SetDeploymentId(deploy.Id)
				.SetProcessDefinitionTenantId(context.TenantId)
				.SetLatestVersion()
				.SingleResult();

			BpmnModel model = commandExecutor.Execute(new GetBpmnModelCmd(definition.Id));

			IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, null));

			IList<ITask> tasks = processEngine.TaskService.GetMyTasks("用户1");

			ITaskEntity task = tasks[0] as ITaskEntity;
			IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

			processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
			{
				{ "流程变量", "变量值" }
			});

			var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

			Assert.Contains(model.MainProcess.FlowElements, x => list.Any(y => x.Id == y.Id));

			tasks = processEngine.TaskService.GetMyTasks("用户2");
			task = tasks[0] as ITaskEntity;
			processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
			{
				{ "流程变量", "变量值" }
			});

			list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

			Assert.Contains(model.MainProcess.FlowElements, x => list.Any(y => x.Id == y.Id));
		}

		[Theory]
		[InlineData("征文评审.bpmn")]
		public void ToXml(string bpmnFile)
		{
			string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

			MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
			var bxc = new BpmnXMLConverter();
			BpmnModel model = bxc.ConvertToBpmnModel(new XMLStreamReader(ms));
			model.MainProcess.Id = "新的流程名称";
			model.MainProcess.Name = "新的流程名称";

			//生成新的流程xml
			xml = Encoding.UTF8.GetString(bxc.ConvertToXML(model));
		}
	}
}
