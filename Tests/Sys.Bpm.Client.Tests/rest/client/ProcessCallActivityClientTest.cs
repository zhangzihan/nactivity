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
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = taskClient.MyTasks(teachers).GetAwaiter().GetResult().List.FirstOrDefault();

                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        ids = bizIds,
                        完成 = true
                    }),
                    NotFoundThrowError = true
                }).GetAwaiter().GetResult();

                Resources<TaskModel> tasks = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient()
                    .GetTasks(new ProcessInstanceTaskQuery()
                    {
                        IncludeCompleted = false,
                        ProcessInstanceId = instances[0].Id
                    }).GetAwaiter().GetResult();

                foreach (var uid in subUsers)
                {
                    var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List.ToList();
                    myTasks.ForEach(tsk => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id }).GetAwaiter().GetResult());
                }

                foreach (var uid in subUsers1)
                {
                    var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List.ToList();
                    myTasks.ForEach(tsk => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id }).GetAwaiter().GetResult());
                }

                var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List.ToList();
                sTasks.ForEach(tsk => taskClient.CompleteTask(new CompleteTaskCmd { TaskId = tsk.Id }).GetAwaiter().GetResult());

                ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient().GetProcessInstanceById(instances[0].Id).GetAwaiter().GetResult();
            });

            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData("主活动并行子活动.bpmn", "主活动_并行子活动.bpmn")]
        public void 使用信号强制终止子活动(string bpmnFile, string subBpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string teachers = Guid.NewGuid().ToString();
                string students = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);
                vars.Add("ids", bizIds);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List.ToList();
                    myTasks.ForEach(tsk => taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        TaskId = tsk.Id
                    }).GetAwaiter().GetResult());
                }

                {
                    //完成子活动1的所有任务
                    var myTasks = taskClient.MyTasks(subUsers1[0]).GetAwaiter().GetResult().List.ToList();
                    myTasks.ForEach(tsk => taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        TaskId = tsk.Id
                    }).GetAwaiter().GetResult());

                    var pic = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
                    pic.SendSignal(new SignalCmd
                    {
                        Name = "terminate",
                        ExecutionId = myTasks[0].ProcessInstanceId
                    }).GetAwaiter().GetResult();
                }

                var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;

                Assert.NotEmpty(sTasks);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("主活动终止事件同时终止子活动.bpmn", "主活动_并行子活动.bpmn")]
        public void 主活动终止事件同时终止子活动(string bpmnFile, string subBpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string 主任务 = Guid.NewGuid().ToString();
                string 下一任务 = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(主任务), 主任务)
                    .AddAssignee(nameof(下一任务), 下一任务)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);
                vars.Add("ids", bizIds);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Assert.NotEmpty(taskClient.MyTasks(主任务).GetAwaiter().GetResult().List);
                Assert.NotEmpty(taskClient.MyTasks(subUsers[0]).GetAwaiter().GetResult().List);

                ///完成主任务节点的任务
                taskClient.CompleteTask(new CompleteTaskCmd
                {
                    BusinessKey = bizId,
                    Assignee = 主任务
                }).GetAwaiter().GetResult();

                Assert.Empty(taskClient.MyTasks(subUsers[0]).GetAwaiter().GetResult().List);
                Assert.Empty(taskClient.MyTasks(下一任务).GetAwaiter().GetResult().List);

                var pi = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient().GetProcessInstanceById(instances[0].Id).GetAwaiter().GetResult();
            });

            Assert.NotNull(ex);
        }

        /// <summary>
        /// 1、当主任务完成的时候，发送消息终止当前子任务,
        /// 2、如果只是子任务完成需要等待主任务完成流程才能继续执行
        /// </summary>
        /// <param name="bpmnFile"></param>
        /// <param name="subBpmnFile"></param>
        [Theory]
        [InlineData("主活动发信号终止子活动.bpmn", "主活动_并行子活动.bpmn", true)]
        [InlineData("主活动发信号终止子活动.bpmn", "主活动_并行子活动.bpmn", false)]
        public void 主活动发信号终止子活动(string bpmnFile, string subBpmnFile, bool isMaster)
        {
            var ex = Record.Exception(() =>
            {
                string teachers = Guid.NewGuid().ToString();
                string students = Guid.NewGuid().ToString();
                string master = Guid.NewGuid().ToString();
                string[] subUsers = new string[]
                {
                    Guid.NewGuid().ToString()
                };
                string[] subUsers1 = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                var bizIds = new string[]
                {
                    Guid.NewGuid().ToString()
                };

                ctx.Deploy(bpmnFile);
                ctx.Deploy(subBpmnFile);

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1)
                    .AddAssignee("主任务", master);
                vars.Add("ids", bizIds);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                if (isMaster)
                {
                    taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        BusinessKey = bizId,
                        Assignee = master
                    }).GetAwaiter().GetResult();

                    var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;

                    Assert.NotEmpty(sTasks);
                }
                else
                {
                    ///完成子任务节点1的任务
                    foreach (var uid in subUsers)
                    {
                        var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List;
                        Task.WhenAll(Enumerable.Select(myTasks, (task) =>
                        {
                            return taskClient.CompleteTask(new CompleteTaskCmd
                            {
                                TaskId = task.Id
                            });
                        })).GetAwaiter().GetResult();
                    }

                    {
                        //完成子活动1,2的所有任务
                        foreach (var uid in subUsers1)
                        {
                            var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List;
                            taskClient.CompleteTask(myTasks.Select(tsk => new CompleteTaskCmd
                            {
                                TaskId = tsk.Id
                            }).ToArray()).GetAwaiter().GetResult();
                        }
                    }

                    var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;
                    //等待主任务完成
                    Assert.Empty(sTasks);

                    taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        BusinessKey = bizId,
                        Assignee = master
                    }).GetAwaiter().GetResult();

                    sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;

                    Assert.NotEmpty(sTasks);
                }
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
                vars.Add("ids", bizIds);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List;
                    Task.WhenAll(Enumerable.Select(myTasks, (task) =>
                    {
                        return taskClient.CompleteTask(new CompleteTaskCmd
                        {
                            TaskId = task.Id
                        });
                    })).GetAwaiter().GetResult();
                }

                {
                    //完成子活动1,2的所有任务
                    foreach (var uid in subUsers1)
                    {
                        var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List;
                        taskClient.CompleteTask(myTasks.Select(tsk => new CompleteTaskCmd
                        {
                            TaskId = tsk.Id
                        }).ToArray()).GetAwaiter().GetResult();
                    }
                }

                var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;

                Assert.NotEmpty(sTasks);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("主活动并行子活动.bpmn", "主活动_并行子活动.bpmn")]
        public void 使用子活动BUSINEEKEY完成并行子活动(string bpmnFile, string subBpmnFile)
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
                vars.Add("ids", bizIds);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    foreach (var bid in bizIds)
                    {
                        taskClient.CompleteTask(new CompleteTaskCmd
                        {
                            Assignee = uid,
                            BusinessKey = bid
                        }).GetAwaiter().GetResult();
                    }
                }

                //完成子活动1,2的所有任务
                foreach (var uid in subUsers1)
                {
                    foreach (var bid in bizIds)
                    {
                        taskClient.CompleteTask(new CompleteTaskCmd
                        {
                            Assignee = uid,
                            BusinessKey = bid
                        }).GetAwaiter().GetResult();
                    }
                }

                var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;

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
                vars.Add("ids", bizIds);

                string bizId = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                ///完成子任务节点1的任务
                foreach (var uid in subUsers)
                {
                    var myTasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List.ToList();
                    myTasks.ForEach(tsk =>
                    {
                        taskClient.CompleteTask(new CompleteTaskCmd
                        {
                            TaskId = tsk.Id
                        }).GetAwaiter().GetResult();
                    });
                }

                {
                    //任务节点1仅完成子用户1的任务，子活动1应该停在任务节点2
                    var myTask = taskClient.MyTasks(subUsers1[0]).GetAwaiter().GetResult().List.First();
                    _ = taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        TaskId = myTask.Id
                    }).GetAwaiter().GetResult();

                    //完成子活动2的所有任务
                    var myTasks = taskClient.MyTasks(subUsers1[1]).GetAwaiter().GetResult().List.ToList();
                    myTasks.ForEach(tsk => taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        TaskId = tsk.Id
                    }).GetAwaiter().GetResult());

                    Assert.NotEmpty(taskClient.MyTasks(subUsers1[0]).GetAwaiter().GetResult().List);
                }

                var sTasks = taskClient.MyTasks(students).GetAwaiter().GetResult().List;

                Assert.Empty(sTasks);
            });

            Assert.Null(ex);
        }
    }
}
