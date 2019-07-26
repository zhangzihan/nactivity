using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Exceptions;
using Sys.Workflow.Rest.Client;
using Sys.Workflow.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using Xunit.Extensions.Ordering;
using Task = System.Threading.Tasks.Task;
using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Engine.Runtime;

namespace Sys.Workflow.Client.Tests.Rest.Client
{
    //[Order(3)]
    public class ProcessCallActivityClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        IProcessInstanceController client = null;

        public ProcessCallActivityClientTest()
        {
            client = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
        }

        [Theory]
        [InlineData("主活动.bpmn", "主活动_子活动.bpmn")]
        public void 调用子活动(string bpmnFile, string subBpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string teachers = Guid.NewGuid().ToString();
                string students = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId));

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = AsyncHelper.RunSync(() => taskClient.MyTasks(teachers)).List.FirstOrDefault();

                _ = AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        ids = bizIds,
                        完成 = true
                    }),
                    NotFoundThrowError = true
                }));

                Resources<TaskModel> tasks = AsyncHelper.RunSync(() => ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient()
                    .GetTasks(new ProcessInstanceTaskQuery()
                    {
                        IncludeCompleted = false,
                        ProcessInstanceId = instances[0].Id
                    }));

                foreach (var uid in subUsers)
                {
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(uid)).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                foreach (var uid in subUsers1)
                {
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(uid)).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                var sTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(students)).List.ToList();
                sTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));

                AsyncHelper.RunSync(() => ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient().GetProcessInstanceById(instances[0].Id));
            });

            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData("主活动并行子活动.bpmn", "主活动_并行子活动.bpmn")]
        public void 强制终止子活动(string bpmnFile, string subBpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string teachers = Guid.NewGuid().ToString();
                string students = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId));

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = AsyncHelper.RunSync(() => taskClient.MyTasks(teachers)).List.FirstOrDefault();

                _ = AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        ids = bizIds,
                        完成 = true
                    }),
                    NotFoundThrowError = true
                }));

                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(uid)).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                {
                    //完成子活动1的所有任务
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(subUsers1[0])).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));

                    string subInstanceId = myTasks[0].ProcessInstanceId;
                    var pic = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
                    AsyncHelper.RunSync(() => pic.Terminate(new TerminateProcessInstanceCmd[]
                    {
                        new TerminateProcessInstanceCmd
                        {
                            ProcessInstanceId = subInstanceId,
                            Reason = "终止并行子活动"
                        }
                    }));
                }

                var sTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(students)).List;

                Assert.NotEmpty(sTasks);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("主活动并行子活动.bpmn", "主活动_并行子活动.bpmn")]
        public void 完成并行子活动(string bpmnFile, string subBpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string teachers = Guid.NewGuid().ToString();
                string students = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId));

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = AsyncHelper.RunSync(() => taskClient.MyTasks(teachers)).List.FirstOrDefault();

                _ = AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        ids = bizIds,
                        完成 = true
                    }),
                    NotFoundThrowError = true
                }));

                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(uid)).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                {
                    //完成子活动1的所有任务
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(subUsers1[0])).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));

                    //完成子活动2的所有任务
                    myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(subUsers1[1])).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                var sTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(students)).List;

                Assert.NotEmpty(sTasks);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("主活动并行子活动.bpmn", "主活动_并行子活动.bpmn")]
        public void 部分完成并行子活动(string bpmnFile, string subBpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string teachers = Guid.NewGuid().ToString();
                string students = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId));

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = AsyncHelper.RunSync(() => taskClient.MyTasks(teachers)).List.FirstOrDefault();

                _ = AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        ids = bizIds,
                        完成 = true
                    }),
                    NotFoundThrowError = true
                }));

                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(uid)).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                {
                    //任务节点1仅完成子用户1的任务，子活动1应该停在任务节点2
                    var myTask = AsyncHelper.RunSync(() => taskClient.MyTasks(subUsers1[0])).List.First();
                    AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = myTask.Id }));

                    //完成子活动2的所有任务
                    var myTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(subUsers1[1])).List.ToList();
                    myTasks.ForEach(tsk => AsyncHelper.RunSync(() => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id })));
                }

                var sTasks = AsyncHelper.RunSync(() => taskClient.MyTasks(students)).List;

                Assert.Empty(sTasks);
            });

            Assert.Null(ex);
        }
    }
}
