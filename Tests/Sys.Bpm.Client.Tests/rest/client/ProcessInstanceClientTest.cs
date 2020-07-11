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

namespace Sys.Workflow.Client.Tests.Rest.Client
{
    //[Order(3)]
    public class ProcessInstanceClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        IProcessInstanceController client = null;

        public ProcessInstanceClientTest()
        {
            client = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
        }

        [Fact]
        public void TestWorkflowVariable()
        {
            string str = Newtonsoft.Json.JsonConvert.SerializeObject(new WorkflowVariable
            {
                { "techer", new string[]{ "1"} }
            });
        }

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        public async Task ProcessInstances_分页获取流程实例列表(int pageSize)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                int offset = 1;
                Resources<ProcessInstance> list = null;
                while (true)
                {
                    ProcessInstanceQuery query = new ProcessInstanceQuery
                    {
                        TenantId = ctx.TenantId,
                        Pageable = new Pageable
                        {
                            PageNo = offset,
                            PageSize = pageSize,
                            Sort = new Sort(new Sort.Order[]
                            {
                                new Sort.Order
                                {
                                    Property = "name",
                                    Direction = Sort.Direction.ASC
                                }
                            })
                        }
                    };

                    list = await client.ProcessInstances(query).ConfigureAwait(false);
                    if (list.List.Count() < pageSize)
                    {
                        break;
                    }

                    offset = offset + 1;
                }

                Assert.True(offset == 1 && list.TotalCount <= 0 ? true : list.TotalCount / pageSize + 1 == offset);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("启动测试.bpmn")]
        public void 流程启动测试(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                //for (var idx = 0; idx < 5; idx++)
                //{
                ctx.StartUseFile(bpmnFile, null, null).GetAwaiter().GetResult();

                //StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                //{
                //    ProcessDefinitionKey = "启动测试",
                //    Variables = null,
                //    TenantId = ctx.TenantId
                //};

                //StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[]
                //{
                //    cmd, cmd, cmd, cmd, cmd
                //};

                //client.Start(cmds);
                //}
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("条件分支流程.bpmn")]
        public void Start_启动一个流程实例(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                //{
                //List<Task> tasks = new List<Task>();

                //for (int idx = 0; idx < 1200; idx++)
                //{
                //Task task = Task.Factory.StartNew(() =>
                //{
                ctx.StartUseFile(bpmnFile, new string[] {
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString()
                    }, new Dictionary<string, object>
                    {
                        ["isTecher"] = false,
                        ["手工分配"] = false,
                        ["是否终审"] = true
                    }).GetAwaiter().GetResult();
            });
            //Assert.NotNull(instances);
            //Assert.True(instances.Count() > 0);
            //});

            //tasks.Add(task);
            // }

            //Task.WaitAll(tasks.ToArray());
            //});

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("任务补偿.bpmn")]
        public void 任务补偿(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string bid = Guid.NewGuid().ToString();
                ProcessDefinition pd = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient().LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    Key = "任务补偿"
                }).GetAwaiter().GetResult().List.FirstOrDefault();

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, businessKey: bid).GetAwaiter().GetResult();

                ctx.CreateWorkflowHttpProxy().GetTaskClient().CompleteTask(new CompleteTaskCmd
                {
                    BusinessKey = bid,
                    Assignee = "user"
                }).GetAwaiter().GetResult();

                //ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                //{
                //    ["同意"] = false
                //}).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("任务过期.bpmn")]
        public void 任务自动过期(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["同意"] = false
                }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("任务过期.bpmn", 10)]
        public void 任务自动过期_并重新分配人员(string bpmnFile, int length)
        {
            var ex = Record.Exception(() =>
            {
                ProcessDefinition process = ctx.GetOrAddProcessDefinition(bpmnFile);

                int count = 0;
                List<ProcessInstance> instances = new List<ProcessInstance>();
                while (count < length)
                {
                    instances.AddRange(ctx.StartUseFile(process, new Dictionary<string, object>
                    {
                        ["同意"] = false
                    }).GetAwaiter().GetResult());
                    count += 1;
                }

                Thread.Sleep(20000);

                foreach (var instance in instances)
                {
                    var pitc = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();
                    var tasks = pitc.GetTasks(new ProcessInstanceTaskQuery
                    {
                        ProcessInstanceId = instance.Id,
                        IncludeCompleted = false,
                    }).GetAwaiter().GetResult();

                    var expired = tasks.List.Where(x => x.Assignee != "管理员");
                    var tac = ctx.CreateWorkflowHttpProxy().GetTaskAdminClient();
                    tac.ReassignTaskUser(new ReassignTaskUserCmd(expired.Select(x =>
                    {
                        return new ReassignUser
                        {
                            TaskId = x.Id,
                            From = x.Assignee,
                            To = Guid.NewGuid().ToString(),
                            Reason = "任务已过期"
                        };
                    }).ToArray()));
                };
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("协同审批分支流程.bpmn")]
        public void 协同审批分支流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["同意"] = false
                }).GetAwaiter().GetResult();

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Thread.Sleep(1000);

                var tasks = tc.MyTasks("8e45aba4-c648-4d71-a1c3-3d15b5518b84").GetAwaiter().GetResult();

                var task = tasks.List.ElementAt(0);

                tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("多人一人就通过.bpmn")]
        public void 多人一人就通过(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                string uid1 = Guid.NewGuid().ToString();
                string next = Guid.NewGuid().ToString();

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["Users"] = new string[] { uid, uid1 },
                    ["next"] = new string[] { next }
                }).GetAwaiter().GetResult();

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                TaskModel task = tc.MyTasks(uid).GetAwaiter().GetResult().List.First();

                _ = tc.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id
                }).GetAwaiter().GetResult();

                _ = tc.MyTasks(next).GetAwaiter().GetResult().List.First();
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("半数通过FALSE.bpmn")]
        public void 协同审批半数拒绝(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string[] users = new string[]
                {
                    "8e45aba4-c648-4d71-a1c3-3d15b5518b84",
                    "1f387de4-0e26-47dd-b2f1-57771a268bc5",
                    "e561d025-76c7-4674-b799-5eae802a4980"
                };

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }).GetAwaiter().GetResult();

                var action = new Action<string, bool>((uid, app) =>
                {
                    ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                    var tasks = tc.MyTasks(uid).GetAwaiter().GetResult();

                    if (tasks.RecordCount > 0)
                    {
                        var task = tasks.List.ElementAt(0);

                        if (app)
                        {
                            tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });
                        }
                        else
                        {
                            tc.Reject(new RejectTaskCmd
                            {
                                TaskId = task.Id,
                                RejectReason = "半数拒绝"
                            });
                        }
                    }
                });

                action(users[0], false);

                action(users[1], true);

                action(users[2], false);

                IProcessInstanceHistoriceController pvc = ctx.CreateWorkflowHttpProxy().GetProcessInstanceHistoriceClient();

                var variables = pvc.GetVariables(new ProcessVariablesQuery
                {
                    ProcessInstanceId = instances[0].Id,
                    ExcludeTaskVariables = true,
                    VariableName = WorkflowVariable.GLOBAL_APPROVALED_VARIABLE
                }).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("半数通过TRUE.bpmn")]
        public void 协同审批半数通过(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string[] users = new string[]
                {
                    "8e45aba4-c648-4d71-a1c3-3d15b5518b84",
                    "1f387de4-0e26-47dd-b2f1-57771a268bc5",
                    //"e561d025-76c7-4674-b799-5eae802a4980"
                };

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }).GetAwaiter().GetResult();

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                var tasks = tc.MyTasks(users[0]).GetAwaiter().GetResult();

                var task = tasks.List.ElementAt(0);

                tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                Thread.Sleep(2000);

                tasks = tc.MyTasks(users[1]).GetAwaiter().GetResult();

                if (tasks.List.Count() > 0)
                {
                    task = tasks.List.ElementAt(0);

                    tc.Reject(new RejectTaskCmd { TaskId = task.Id, RejectReason = "不同意" });
                }

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("全部通过.bpmn")]
        public void 协同审批一票否决(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string[] users = new string[]
                {
                    "8e45aba4-c648-4d71-a1c3-3d15b5518b84",
                    "1f387de4-0e26-47dd-b2f1-57771a268bc5",
                    //"e561d025-76c7-4674-b799-5eae802a4980"
                };

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }).GetAwaiter().GetResult();

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                var tasks = tc.MyTasks(users[0]).GetAwaiter().GetResult();

                var task = tasks.List.ElementAt(0);

                tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                Thread.Sleep(1000);

                tasks = tc.MyTasks(users[1]).GetAwaiter().GetResult();

                if (tasks.List.Count() > 0)
                {
                    task = tasks.List.ElementAt(0);

                    tc.Reject(new RejectTaskCmd { TaskId = task.Id, RejectReason = "不同意" });
                }

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("全部通过.bpmn")]
        public void 协同审批全部通过(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string[] users = new string[]
                {
                    "8e45aba4-c648-4d71-a1c3-3d15b5518b84",
                    "1f387de4-0e26-47dd-b2f1-57771a268bc5",
                    //"e561d025-76c7-4674-b799-5eae802a4980"
                };

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }).GetAwaiter().GetResult();

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                foreach (var uid in users)
                {
                    var tasks = tc.MyTasks(uid).GetAwaiter().GetResult();

                    if (tasks.List.Count() > 0)
                    {
                        var task = tasks.List.ElementAt(0);

                        tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                        Thread.Sleep(2000);
                    }
                }

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("一人通过.bpmn")]
        public void 协同审批一人通过(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                int count = 0;

                string[] users = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                };

                ProcessDefinition process = ctx.GetOrAddProcessDefinition(bpmnFile);

                while (count < 1)
                {
                    ProcessInstance[] instances = ctx.StartUseFile(process, new Dictionary<string, object>
                    {
                        ["UserTask_11w47k9s"] = users
                    }).GetAwaiter().GetResult();

                    count += 1;
                }

                count = 0;
                while (count < 1)
                {
                    ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                    bool approvaled = true;
                    foreach (var uid in users)
                    {
                        var tasks = tc.MyTasks(uid).GetAwaiter().GetResult();

                        foreach (var task in tasks.List)
                        {
                            //if (approvaled)
                            //{
                            //    tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });
                            //}
                            //else
                            //{
                            tc.Reject(new RejectTaskCmd { TaskId = task.Id, RejectReason = "不同意" });
                            //}
                            approvaled = !approvaled;
                        }
                    }

                    count += 1;
                };
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("转审.bpmn")]
        public async Task 任务转审单人(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string sid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, new string[] { sid }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                var tasks = await tc.MyTasks(sid).ConfigureAwait(false);

                var uid = Guid.NewGuid().ToString();

                TaskModel[] model = await tc.TransferTask(new TransferTaskCmd
                {
                    TaskId = tasks.List.FirstOrDefault().Id,
                    Assignees = new string[] { uid },
                    Description = "转审测试"
                }).ConfigureAwait(false);

                tasks = await tc.MyTasks(uid).ConfigureAwait(false);

                await tc.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = tasks.List.First().Id
                }).ConfigureAwait(false);

                tasks = await tc.MyTasks(uid).ConfigureAwait(false);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("转审.bpmn")]
        [Order(1)]
        public async Task 任务转审多人(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string sid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, new string[] { sid }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                Thread.Sleep(1000);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                var tasks = await tc.MyTasks(sid).ConfigureAwait(false);

                var uids = new string[] {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString() };

                TaskModel[] models = await tc.TransferTask(new TransferTaskCmd
                {
                    TaskId = tasks.List.FirstOrDefault().Id,
                    Assignees = uids,
                    Description = "转审测试"
                }).ConfigureAwait(false);

                foreach (var uid in uids)
                {
                    tasks = await tc.MyTasks(uid).ConfigureAwait(false);

                    await tc.CompleteTask(new CompleteTaskCmd
                    {
                        TaskId = tasks.List.First().Id
                    }).ConfigureAwait(false);

                    tasks = await tc.MyTasks(uid).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }


        private async Task<Resources<TaskModel>> Complete(ITaskController tc, string userId, WorkflowVariable vars)
        {
            Resources<TaskModel> tasks = await tc.MyTasks(userId).ConfigureAwait(false);

            TaskModel task = tasks.List.FirstOrDefault();

            await tc.CompleteTask(new CompleteTaskCmd
            {
                TaskId = task.Id,
                OutputVariables = vars
            }).ConfigureAwait(false);

            tasks = await tc.MyTasks(userId).ConfigureAwait(false);

            return tasks;
        }

        [Theory]
        [InlineData("用户注册路径.bpmn")]
        public async Task Start_完成用户注册路径_男(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                IProcessDefinitionDeployerController pdc = ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

                string formKey = Guid.NewGuid().ToString();

                string xml = IntegrationTestContext.ReadBpmn(bpmnFile);
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                var model = new BpmnXMLConverter().ConvertToBpmnModel(ms);
                var start = model.MainProcess.FindFlowElement("StartEvent_1mbkn3l") as StartEvent;
                start.FormKey = formKey;
                xml = Encoding.UTF8.GetString(new BpmnXMLConverter().ConvertToXML(model));

                Deployment deployment = await pdc.Deploy(new ProcessDefinitionDeployer
                {
                    BpmnXML = xml,
                    Name = Path.GetFileNameWithoutExtension(bpmnFile),
                    TenantId = ctx.TenantId
                }).ConfigureAwait(false);

                string uid = Guid.NewGuid().ToString();
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    StartForm = formKey,
                    Variables = new Dictionary<string, object>
                     {
                         { "User", new string[]{ uid } }
                     },
                    TenantId = ctx.TenantId
                };

                ProcessInstance[] instances = client.Start(new StartProcessInstanceCmd[] { cmd }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Resources<TaskModel> myTasks = null;

                //while (true)
                //{
                myTasks = await Complete(tc, uid, new Dictionary<string, object>
                        {
                            { "gender", 1 }
                        }).ConfigureAwait(false);

                //if (myTasks == null || myTasks.TotalCount <= 0)
                //{
                //    break;
                //}
                //}
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        /// <summary>
        /// 只能触发一个消息开始节点
        /// </summary>
        /// <param name="bpmnFile"></param>
        [Theory]
        [InlineData("使用消息触发流程.bpmn")]
        [Order(2)]
        public void Start_使用消息触发流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                string bizId = Guid.NewGuid().ToString();
                WorkflowVariable values = new WorkflowVariable();
                values.AddAssignee("subuser", uid);

                ctx.Deploy(bpmnFile);

                var pc = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
                pc.Start(new StartProcessInstanceCmd[]
                {
                    new StartProcessInstanceCmd
                    {
                        StartByMessage = "Message_38jm21j",
                        BusinessKey = bizId,
                        Variables = values,
                        TenantId = ctx.TenantId
                    }
                });
            });

            Assert.Null(ex);
        }

        /// <summary>
        /// 使用信号可以触发多个同名信号开始节点流程定义
        /// </summary>
        /// <param name="bpmnFiles"></param>
        [Theory]
        [InlineData("日程支付信号触发酒店预订.bpmn", "日程支付信号触发机票预订.bpmn")]
        [Order(2)]
        public void Start_使用信号触发流程(params string[] bpmnFiles)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                string bizId = Guid.NewGuid().ToString();
                WorkflowVariable values = new WorkflowVariable();
                values.AddAssignee("酒店预订", uid);
                values.AddAssignee("机票预订", uid);

                foreach (var bpmnFile in bpmnFiles)
                {
                    ctx.Deploy(bpmnFile);
                }

                var pc = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
                pc.SendSignal(new SignalCmd
                {
                    TenantId = ctx.TenantId,
                    Name = "日程已支付",
                    InputVariables = values
                }).GetAwaiter().GetResult();


            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("包容网关.bpmn", 100)]
        [InlineData("包容网关.bpmn", 101)]
        public void 包容网关(string bpmnFile, int money)
        {
            var ex = Record.Exception(() =>
            {
                string 项目采购 = Guid.NewGuid().ToString();
                string 项目主管 = Guid.NewGuid().ToString();
                string 项目经理 = Guid.NewGuid().ToString();
                string 财务主管 = Guid.NewGuid().ToString();
                string 项目总监 = Guid.NewGuid().ToString();
                string 财务总监 = Guid.NewGuid().ToString();

                string bizId = Guid.NewGuid().ToString();
                WorkflowVariable values = new WorkflowVariable();
                values.AddAssignee("项目采购", 项目采购)
                    .AddAssignee("项目主管", 项目主管)
                    .AddAssignee("项目经理", 项目经理)
                    .AddAssignee("财务主管", 财务主管)
                    .AddAssignee("项目总监", 项目总监)
                    .AddAssignee("财务总监", 财务总监);

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, values, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                taskClient.CompleteTask(new CompleteTaskCmd
                {
                    BusinessKey = bizId,
                    OutputVariables = new WorkflowVariable
                    {
                        ["金额"] = money
                    }
                });
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("用户注册路径.bpmn")]
        [Order(2)]
        public async Task Start_完成用户注册路径_女(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                IProcessDefinitionDeployerController pdc = ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

                string formKey = Guid.NewGuid().ToString();

                string xml = IntegrationTestContext.ReadBpmn(bpmnFile);
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                var model = new BpmnXMLConverter().ConvertToBpmnModel(ms);
                var start = model.MainProcess.FindFlowElement("StartEvent_1mbkn3l") as StartEvent;
                start.FormKey = formKey;
                xml = Encoding.UTF8.GetString(new BpmnXMLConverter().ConvertToXML(model));

                Deployment deployment = await pdc.Deploy(new ProcessDefinitionDeployer
                {
                    BpmnXML = xml,
                    Name = Path.GetFileNameWithoutExtension(bpmnFile),
                    TenantId = ctx.TenantId
                }).ConfigureAwait(false);

                string uid = Guid.NewGuid().ToString();
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    StartForm = formKey,
                    Variables = new Dictionary<string, object>
                     {
                         { "User", new string[]{ uid } }
                     },
                    TenantId = ctx.TenantId
                };

                ProcessInstance[] instances = client.Start(new StartProcessInstanceCmd[] { cmd }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Resources<TaskModel> myTasks = null;

                //while (true)
                //{
                myTasks = await Complete(tc, uid, new Dictionary<string, object>
                        {
                            { "gender", 2 }
                        }).ConfigureAwait(false);

                //    if (myTasks == null || myTasks.TotalCount <= 0)
                //    {
                //        break;
                //    }
                //}
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("变量异常.bpmn")]
        public async Task 变量抛异常(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = await ctx.StartUseFile(bpmnFile, new string[0], new Dictionary<string, object>
                    {
                        { "starter", new string[]{ uid } }
                    }).ConfigureAwait(false);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Resources<TaskModel> tasks = await tc.MyTasks(uid).ConfigureAwait(false);

                TaskModel task = tasks.List.FirstOrDefault(x => x.Status == "ASSIGNED");

                await tc.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id
                }).ConfigureAwait(false);

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            }).ConfigureAwait(false);

            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData("会签流程.bpmn")]
        public void Start_会签流程_指定用户(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    ProcessDefinitionId = ctx.GetOrAddProcessDefinition(bpmnFile).Id,
                    TenantId = ctx.TenantId
                };

                ProcessInstance[] instances = client.Start(new StartProcessInstanceCmd[] { cmd }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                var pit = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();

                var tasks = pit.GetTasks(new ProcessInstanceTaskQuery() { ProcessInstanceId = instances[0].Id }).GetAwaiter().GetResult();

                Assert.True(tasks.List.Count() > 0);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("会签流程选择节点执行人.bpmn")]
        public void Start_会签流程_指定节点执行人(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    ProcessDefinitionId = ctx.GetOrAddProcessDefinition(bpmnFile).Id,
                    TenantId = ctx.TenantId
                };

                ProcessInstance[] instances = client.Start(new StartProcessInstanceCmd[] { cmd }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                var pit = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();

                var tasks = pit.GetTasks(new ProcessInstanceTaskQuery() { ProcessInstanceId = instances[0].Id }).GetAwaiter().GetResult();

                TaskModel task = tasks.List.FirstOrDefault(x => x.Status == "ASSIGNED");

                Assert.NotNull(task);

                ctx.CreateWorkflowHttpProxy().GetTaskClient().CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id
                }).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }

        public Task<ProcessInstance[]> Start(IProcessInstanceController client, string bpmnFile, string[] users)
        {
            string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
            ctx.Deploy(bpmnFile, bpmnName);

            var vars = new Dictionary<string, object>
            {
                { "name", users }
            };
            StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
            {
                ProcessName = bpmnName,
                Variables = vars,
                TenantId = ctx.TenantId
            };

            StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[] { cmd };

            return client.Start(cmds);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Start_启动多个流程实例(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string[] users = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    ProcessDefinitionId = ctx.GetOrAddProcessDefinition(bpmnFile).Id,
                    Variables = new Dictionary<string, object>
                     {
                         { "name", users }
                     },
                    TenantId = ctx.TenantId
                };

                ProcessInstance[] instances = client.Start(new StartProcessInstanceCmd[] { cmd }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
            });

            Assert.Null(ex);
        }

        public Task<ProcessInstance> StartByActiviti_使用指定的流程活动_启动流程(string processDefinitionId, string businessKey, string activityId, IDictionary<string, object> variables)
        {
            throw new NotImplementedException();
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetProcessInstanceById_查找一个流程实例(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ProcessInstance inst = client.GetProcessInstanceById(instances[0].Id).GetAwaiter().GetResult();

                Assert.NotNull(inst);
            });

            Assert.Null(ex);
        }

        public Task<ActionResult> SendSignal_(SignalCmd cmd)
        {
            throw new NotImplementedException();
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Suspend_挂起一个流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ProcessInstance inst = client.Suspend(instances[0].Id).GetAwaiter().GetResult();

                Assert.NotNull(inst);
                Assert.Equal("SUSPENDED", inst.Status, ignoreCase: true);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Activate_先挂起流程_再激活挂起的流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ProcessInstance inst = client.Suspend(instances[0].Id).GetAwaiter().GetResult();

                Assert.NotNull(inst);
                Assert.Equal("SUSPENDED", inst.Status, ignoreCase: true);

                inst = client.Activate(instances[0].Id).GetAwaiter().GetResult();

                Assert.NotNull(inst);
                Assert.Equal("RUNNING", inst.Status, StringComparer.OrdinalIgnoreCase);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Terminate_先启动流程_然后强制终止流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                IProcessInstanceController client = ctx.CreateWorkflowHttpProxy()
                .GetProcessInstanceClient();

                string processDefinitionId = ctx.GetOrAddProcessDefinition(bpmnFile).Id;

                StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[2];

                WorkflowVariable variables = new WorkflowVariable();
                string uid = Guid.NewGuid().ToString();
                string uid1 = Guid.NewGuid().ToString();
                variables.AddAssignee("name", new string[] { uid, uid1 });

                string bkey1 = Guid.NewGuid().ToString();
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    ProcessDefinitionId = processDefinitionId,
                    Variables = variables,
                    BusinessKey = bkey1,
                    TenantId = ctx.TenantId
                };
                cmds[0] = cmd;

                variables = new WorkflowVariable();
                uid = Guid.NewGuid().ToString();
                uid1 = Guid.NewGuid().ToString();
                variables.AddAssignee("name", new string[] { uid, uid1 });

                string bkey2 = Guid.NewGuid().ToString();
                cmd = new StartProcessInstanceCmd()
                {
                    ProcessDefinitionId = processDefinitionId,
                    Variables = variables,
                    BusinessKey = bkey2,
                    TenantId = ctx.TenantId
                };
                cmds[1] = cmd;

                ProcessInstance[] instances = client.Start(cmds).GetAwaiter().GetResult();

                TerminateProcessInstanceCmd[] tcmds = new TerminateProcessInstanceCmd[]
                {
                    new TerminateProcessInstanceCmd(null, bkey1, "测试终止流程", new WorkflowVariable()),
                    new TerminateProcessInstanceCmd(null, bkey2, "测试终止流程", new WorkflowVariable())
                };

                client.Terminate(tcmds).GetAwaiter().GetResult();

                ProcessInstance inst = client.GetProcessInstanceById(instances[0].Id).GetAwaiter().GetResult();
            });

            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData("WebApi.bpmn")]
        public void 测试WebApi节点服务流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                for (var idx = 0; idx < 10; idx++)
                {
                    string uid = Guid.NewGuid().ToString();
                    ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                    {
                        ["user"] = new string[] { uid },
                        ["startTimer"] = DateTime.Now.AddSeconds(4)
                    }, Guid.NewGuid().ToString()).GetAwaiter().GetResult();

                    ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                    Resources<TaskModel> tasks = taskClient.MyTasks(uid).GetAwaiter().GetResult();
                    Assert.True(tasks.TotalCount == 1);
                }
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("定时执行.bpmn")]
        [Order(2000)]
        public async Task 定时5秒后执行用户任务(string bpmnFile)
        {
            ProcessInstance[] insts = null;
            DateTime timerDateTime = DateTime.Now.AddSeconds(5);
            string uid = Guid.NewGuid().ToString();

            var ex = Record.Exception(() =>
            {
                insts = ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                {
                    { "timerDateTime", timerDateTime}
                }).GetAwaiter().GetResult();
            });

            Assert.Null(ex);

            ex = await Record.ExceptionAsync(async () =>
            {
                int retry = 0;
                Resources<TaskModel> tasks = null;
                ITaskController client = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                while (retry != 2)
                {
                    Thread.Sleep(10000);

                    tasks = await client.MyTasks(uid).ConfigureAwait(false);

                    if (tasks?.List.Count() == 0)
                    {
                        retry += 1;
                        continue;
                    }
                    break;
                }
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("定时取消任务.bpmn")]
        public void 定时5秒后取消用户任务(string bpmnFile)
        {
            ProcessInstance[] insts = null;
            DateTime timerDateTime = DateTime.Now.AddSeconds(5);

            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                insts = ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                    {
                        { "timerDateTime", timerDateTime}
                    }).GetAwaiter().GetResult();
            });

            Thread.Sleep(10000);

            Assert.Null(ex);

            ex = Record.Exception(() =>
            {
                ProcessInstance inst = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient().GetProcessInstanceById(insts[0].Id).GetAwaiter().GetResult();
            });

            Assert.NotNull(ex);
        }


        [Theory]
        [InlineData("延时执行任务.bpmn")]
        [Order(4000)]
        public async Task 延时5秒后执行用户任务(string bpmnFile)
        {
            ProcessInstance[] insts = null;
            DateTime timerDateTime = DateTime.Now.AddSeconds(5);

            TimeSpan timespan = TimeSpan.FromTicks(timerDateTime.Ticks);

            //转换为ISO8601 duration格式字符串
            string duration = XmlConvert.ToString(timespan);

            string uid = Guid.NewGuid().ToString();

            var ex = Record.Exception(() =>
            {
                insts = ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                    {
                        { "timerDateTime", timerDateTime}
                    }).GetAwaiter().GetResult();
            });

            Thread.Sleep(10000);

            Assert.Null(ex);

            ex = await Record.ExceptionAsync(async () =>
            {
                var tasks = await ctx.CreateWorkflowHttpProxy().GetTaskClient().MyTasks(uid).ConfigureAwait(false);

                Assert.NotEmpty(tasks.List);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }


        [Theory]
        [InlineData("消息发送.bpmn")]
        public void 消息模板发送任务(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }


        [Theory]
        [InlineData("催办任务.bpmn", 3)]
        [Order(3000)]
        public void 定时催办任务(string bpmnFile, int count)
        {
            ProcessInstance[] insts = null;
            DateTime start = DateTime.Now.AddSeconds(10);

            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                insts = ctx.StartUseFile(bpmnFile, new string[] { uid },
                    new Dictionary<string, object>
                    {
                        { "count", count },
                        { "start", start}
                    }).GetAwaiter().GetResult();
            });

            Thread.Sleep(30000);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("循环任务.bpmn", 3)]
        [Order(5000)]
        public void 循环任务(string bpmnFile, int count)
        {
            ProcessInstance[] insts = null;
            DateTime start = DateTime.Now.AddSeconds(10);

            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                insts = ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                    {
                        { "count", count },
                        { "start", start}
                    }).GetAwaiter().GetResult();
            });

            Thread.Sleep(30000);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("使用表单启动流程.bpmn")]
        public async void 使用表单启动流程(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string formKey = Guid.NewGuid().ToString();

                IProcessDefinitionDeployerController deployerClient = ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

                string depBpmnName = Path.GetFileNameWithoutExtension(bpmnFile);

                string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                BpmnModel model = new BpmnXMLConverter().ConvertToBpmnModel(ms);
                StartEvent start = model.MainProcess.FindFlowElementsOfType<StartEvent>().First();
                start.FormKey = formKey;
                xml = Encoding.UTF8.GetString(new BpmnXMLConverter().ConvertToXML(model));

                ProcessDefinitionDeployer deployer = new ProcessDefinitionDeployer
                {
                    Name = depBpmnName,
                    BpmnXML = xml,
                    TenantId = ctx.TenantId,
                    EnableDuplicateFiltering = false
                };

                Deployment deployment = await deployerClient.Deploy(deployer).ConfigureAwait(false);

                ProcessInstance[] insts = await client.Start(new StartProcessInstanceCmd[]
                {
                    new StartProcessInstanceCmd
                    {
                        StartForm = formKey,
                        TenantId = ctx.TenantId
                    }
                }).ConfigureAwait(false);

                Assert.Single(insts);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("任务提交节点变量.bpmn")]
        public void 任务提交节点变量(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["users"] = new string[] { uid }
                }, Guid.NewGuid().ToString()).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = taskClient.MyTasks(uid).GetAwaiter().GetResult().List.FirstOrDefault();
                var variabled = new WorkflowVariable
                {
                    Approvaled = true
                };
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    OutputVariables = variabled,
                    LocalScope = false,
                    TaskId = task.Id,
                    NotFoundThrowError = true
                }).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("指定下一节点人员.bpmn")]
        public void 使用当前节点完成时指定下一流程人员(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                //启动时的Teachers
                string[] teachers = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["teachers"] = teachers
                }, Guid.NewGuid().ToString()).GetAwaiter().GetResult();


                //提交完成我的任务
                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = taskClient.MyTasks(teachers[0]).GetAwaiter().GetResult().List.FirstOrDefault();
                var variabled = new WorkflowVariable();
                //从业务端获取任务完成条件 节点teacher的条件 
                //${nrOfActivateInstances == 0 or 完成==true}
                variabled["完成"] = true;
                //运行时业务可以读取student
                string[] students = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                variabled["students"] = students;
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    OutputVariables = variabled,
                    //一定要设置为false，否则OutputVariables变量的作用域仅当前节点可见
                    LocalScope = false,
                    TaskId = task.Id
                }).GetAwaiter().GetResult();

                //查询下一节点是否有任务
                task = taskClient.MyTasks(students[0]).GetAwaiter().GetResult().List.FirstOrDefault();
                Assert.NotNull(task);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("指定下一节点人员.bpmn")]
        public void 使用变量指定下一流程人员(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                //启动时的Teachers
                string[] teachers = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["teachers"] = teachers
                }, Guid.NewGuid().ToString()).GetAwaiter().GetResult();


                //使用变量方式，需要在调用完成任务前对变量赋值，否则流程会报找不到students表达式错误
                //运行时业务可以读取student
                string[] students = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                IProcessInstanceVariableController processInstanceVariable = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();
                _ = processInstanceVariable.SetVariables(new SetProcessVariablesCmd(instances[0].Id,
                     new WorkflowVariable
                     {
                         ["students"] = students
                     })).GetAwaiter().GetResult();


                //提交完成我的任务
                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = taskClient.MyTasks(teachers[0]).GetAwaiter().GetResult().List.FirstOrDefault();
                var variabled = new WorkflowVariable();
                //从业务端获取任务完成条件 节点teacher的条件 
                //${nrOfActivateInstances == 0 or 完成==true}
                variabled["完成"] = true;
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    OutputVariables = variabled,
                    //一定要设置为false，否则OutputVariables变量的作用域仅当前节点可见
                    LocalScope = false,
                    TaskId = task.Id
                }).GetAwaiter().GetResult();

                //查询下一节点是否有任务
                task = taskClient.MyTasks(students[0]).GetAwaiter().GetResult().List.FirstOrDefault();
                Assert.NotNull(task);
            });

            Assert.Null(ex);
        }

        [Theory]
        //全部通过
        [InlineData("主流程_商品审核.bpmn", true, true, false)]
        //一审拒绝
        [InlineData("主流程_商品审核.bpmn", false, null, false)]
        //一审通过，二审拒绝
        [InlineData("主流程_商品审核.bpmn", true, false, false)]
        //一审去修改
        [InlineData("主流程_商品审核.bpmn", null, true, true)]
        //一审通过，二审去修改
        [InlineData("主流程_商品审核.bpmn", true, null, true)]
        public void 主流程_商品审核(string bpmnFile, bool? passed1, bool? passed2, bool needEdit)
        {
            var ex = Record.Exception(() =>
            {
                ctx.Deploy(bpmnFile);

                for (int i = 0; i < 1; i++)
                {
                    StartTest(passed1, passed2, needEdit);
                }
            });

            Assert.Null(ex);
        }

        private void StartTest(bool? passed1, bool? passed2, bool needEdit)
        {
            var attendeeItems = new[]
            {
                //new { Id = Guid.NewGuid().ToString() },
                //new { Id = Guid.NewGuid().ToString() },
                //new { Id = Guid.NewGuid().ToString() },
                //new { Id = Guid.NewGuid().ToString() },
                //new { Id = Guid.NewGuid().ToString() },
                //new { Id = Guid.NewGuid().ToString() },
                //new { Id = Guid.NewGuid().ToString() },
                new { Id = Guid.NewGuid().ToString() }
            };

            string startUser = Guid.NewGuid().ToString();

            var gup1 = new string[]
            {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
            };

            var gup2 = new string[]
            {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
            };

            var gup3 = new string[]
            {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
            };

            IList<StartProcessInstanceCmd> itemCmds = new List<StartProcessInstanceCmd>();
            foreach (var item in attendeeItems)
            {
                //转换为流程变量
                WorkflowVariable variables = WorkflowVariable.FromObject(
                    new
                    {
                        MultiApproval = true,
                        Passed = false,
                        NeedEdit = false,
                        AuditLevel = 2,
                        StartUser = new string[] { startUser }
                    }
                );

                variables.AddAssignee($"GroupId1", gup1);
                variables.AddAssignee($"GroupId2", gup2);
                variables.AddAssignee($"GroupId3", gup3);

                itemCmds.Add(new StartProcessInstanceCmd
                {
                    //使用流程key+tenantid启动一个流程
                    ProcessDefinitionKey = "Process_ProductItem_Approval",
                    TenantId = ctx.TenantId,
                    BusinessKey = item.Id,
                    Variables = variables
                });
            }

            var httpProxy = ctx.CreateWorkflowHttpProxy();
            httpProxy.HttpProxy.SetHttpClientRequestAccessToken(startUser, ctx.TenantId);

            var processClient = httpProxy.GetProcessInstanceClient();

            ProcessInstance[] instances = processClient.Start(itemCmds.ToArray()).GetAwaiter().GetResult();

            Assert.True(instances.Length == itemCmds.Count);

            var taskClient = httpProxy.GetTaskClient();

            //一审
            CompleteTaskCmd[] tsks = CompleteProductApproval(taskClient, attendeeItems[0].Id, gup1[0], passed1, needEdit, 1).GetAwaiter().GetResult();

            Assert.Empty(tsks);

            if (passed1 == false)
            {
                Resources<TaskModel> tasks = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient().GetTasks(new ProcessInstanceTaskQuery
                {
                    ProcessInstanceId = instances[0].Id,
                    IncludeCompleted = false
                }).GetAwaiter().GetResult();

                Assert.Empty(tasks.List);

                return;
            }

            if (passed1 == null && needEdit)
            {
                tsks = CompleteProductApproval(taskClient, attendeeItems[0].Id, startUser, false, false, 1).GetAwaiter().GetResult();

                Assert.Empty(tsks);

                tsks = CompleteProductApproval(taskClient, attendeeItems[0].Id, gup1[0], true, false, 1).GetAwaiter().GetResult();

                Assert.Empty(tsks);

                needEdit = false;
            }

            //二审
            CompleteProductApproval(taskClient, attendeeItems[0].Id, gup2[0], passed2, needEdit, 2).GetAwaiter().GetResult();

            if (passed2 == null && needEdit)
            {
                tsks = CompleteProductApproval(taskClient, attendeeItems[0].Id, startUser, false, false, 2).GetAwaiter().GetResult();

                Assert.Empty(tsks);

                tsks = CompleteProductApproval(taskClient, attendeeItems[0].Id, gup2[0], true, false, 2).GetAwaiter().GetResult();

                Assert.Empty(tsks);
            }

            {
                Resources<TaskModel> tasks = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient().GetTasks(new ProcessInstanceTaskQuery
                {
                    ProcessInstanceId = instances[0].Id,
                    IncludeCompleted = false
                }).GetAwaiter().GetResult();

                Assert.Empty(tasks.List);
            }
        }

        private Task<CompleteTaskCmd[]> CompleteProductApproval(ITaskController taskClient, string businessKey, string assignee, bool? passed, bool needEdit, int level)
        {
            return taskClient.CompleteTask(new CompleteTaskCmd[]
               {
                new CompleteTaskCmd
                {
                    BusinessKey = businessKey,
                    Assignee = assignee,
                    NotFoundThrowError = true,
                    OutputVariables = WorkflowVariable.FromObject(
                        new
                        {
                            Passed = passed,
                            NeedEdit = needEdit,
                            CurrentLevel = level,
                            AuditStep =  "" + level
                        }
                    )
                }
               });
        }

        [Theory]
        [InlineData("子流程.bpmn")]
        public void 子流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                string utask = Guid.NewGuid().ToString();
                string[] ids = new string[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };
                string masterKey = Guid.NewGuid().ToString();

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee("子用户", uid).AddAssignee("主用户", utask).Add("ids", ids);

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: masterKey).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Task.WhenAll(Enumerable.Select(ids, async (businessKey) =>
                {
                    _ = await taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        Assignee = uid,
                        BusinessKey = businessKey
                    }).ConfigureAwait(false);
                })).GetAwaiter().GetResult();

                var tasks = taskClient.MyTasks(utask).GetAwaiter().GetResult().List.ToList();
                Assert.NotEmpty(tasks);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("消息事件子流程.bpmn", 3)]
        public void 消息事件子流程(string bpmnFile, int loopSubProcess)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                string utask = Guid.NewGuid().ToString();
                string eventUser = Guid.NewGuid().ToString();
                string bizId = Guid.NewGuid().ToString();

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee("teachers", uid)
                    .AddAssignee("students", utask)
                    .AddAssignee(nameof(eventUser), eventUser);

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();
                TaskModel task = taskClient.MyTasks(uid).GetAwaiter().GetResult().List.FirstOrDefault();

                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        完成 = true
                    }),
                    NotFoundThrowError = true
                }).GetAwaiter().GetResult();

                Resources<TaskModel> tasks = taskClient.MyTasks(utask).GetAwaiter().GetResult();
                Assert.NotEmpty(tasks.List);

                tasks = taskClient.MyTasks(eventUser).GetAwaiter().GetResult();
                Assert.NotEmpty(tasks.List);

                for (var idx = 0; idx < loopSubProcess; idx++)
                {
                    taskClient.CompleteTask(new CompleteTaskCmd
                    {
                        BusinessKey = bizId,
                        Assignee = eventUser
                    }).GetAwaiter().GetResult();
                }

                taskClient.CompleteTask(new CompleteTaskCmd
                {
                    BusinessKey = bizId,
                    Assignee = utask
                }).GetAwaiter().GetResult();

                tasks = taskClient.MyTasks(utask).GetAwaiter().GetResult();
                Assert.Empty(tasks.List);

                tasks = taskClient.MyTasks(eventUser).GetAwaiter().GetResult();
                Assert.Empty(tasks.List);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("并行多子流程.bpmn")]
        public void 并行多子流程(string bpmnFile)
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

                WorkflowVariable vars = new WorkflowVariable();
                vars.AddAssignee(nameof(teachers), teachers)
                    .AddAssignee(nameof(students), students)
                    .AddAssignee(nameof(subUsers), subUsers)
                    .AddAssignee(nameof(subUsers1), subUsers1);

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars).GetAwaiter().GetResult();

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

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                //完成用户1的任务
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    Assignee = user1,
                    BusinessKey = bizId,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        完成 = true
                    })
                }).GetAwaiter().GetResult();

                Assert.Empty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);

                //完成用户2的任务
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    Assignee = user2,
                    BusinessKey = bizId,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        完成 = true
                    })
                }).GetAwaiter().GetResult();

                Assert.NotEmpty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);
            });

            Assert.Null(ex);
        }

        /// <summary> 
        /// 1、执行人1执行结果为真，等待执行人2执行完成，不管执行人2执行结果如何，流程都会结束
        /// 2、执行人1执行结果为假，执行人2执行结果为真，则流程继续执行
        /// 3、执行人2执行结果为假，等待执行人1执行完成，不管执行人1执行结果如何，流程都会结束
        /// </summary>
        /// <param name="bpmnFile"></param>
        /// <param name="user1Result"></param>
        [Theory]
        [InlineData("条件分支合并.bpmn", true, true)]
        [InlineData("条件分支合并.bpmn", false, true)]
        [InlineData("条件分支合并.bpmn", false, false)]
        public void 条件分支合并_所有分支都完成后执行下一流程(string bpmnFile, bool user1Result, bool user2Result)
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

                ProcessInstance[] instances = ctx.StartUseFile(bpmnFile, null, vars, businessKey: bizId).GetAwaiter().GetResult();

                ITaskController taskClient = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                //完成用户1的任务,USER1==false的时候会执行合并
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    Assignee = user1,
                    BusinessKey = bizId,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        完成 = user1Result
                    })
                }).GetAwaiter().GetResult();

                Assert.Empty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);

                //完成用户2的任务
                _ = taskClient.CompleteTask(new CompleteTaskCmd
                {
                    Assignee = user2,
                    BusinessKey = bizId,
                    OutputVariables = WorkflowVariable.FromObject(new
                    {
                        完成 = user2Result
                    })
                }).GetAwaiter().GetResult();

                if (user1Result && user2Result)
                {
                    Assert.Empty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);
                }
                else if (!user1Result && user2Result)
                {
                    Assert.NotEmpty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);
                }
                else if (!user1Result && !user2Result)
                {
                    Assert.Empty(taskClient.MyTasks(waiter).GetAwaiter().GetResult().List);
                }
            });

            Assert.Null(ex);
        }
    }
}
