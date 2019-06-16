using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.bpmn.constants;
using Sys.Workflow.bpmn.model;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow.engine.repository;
using Sys.Workflow.services.api.commands;
using org.springframework.hateoas;
using Sys.Bpm.Exceptions;
using Sys.Bpm.Rest.Client;
using Sys.Bpmn.Test;
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

namespace Sys.Bpm.Client.Tests.rest.client
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
        [InlineData("条件分支流程.bpmn")]
        public void Start_启动一个流程实例(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                {
                    ["isTecher"] = false,
                    ["手工分配"] = false,
                    ["是否终审"] = true
                }));

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["同意"] = false
                }));

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
                    instances.AddRange(AsyncHelper.RunSync(() => ctx.StartUseFile(process, new Dictionary<string, object>
                    {
                        ["同意"] = false
                    })));
                    count += 1;
                }

                Thread.Sleep(20000);

                foreach (var instance in instances)
                {
                    var pitc = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();
                    var tasks = AsyncHelper.RunSync(() => pitc.GetTasks(new ProcessInstanceTaskQuery
                    {
                        ProcessInstanceId = instance.Id,
                        IncludeCompleted = false,
                    }));

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["同意"] = false
                }));

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Thread.Sleep(1000);

                var tasks = AsyncHelper.RunSync(() => tc.MyTasks("8e45aba4-c648-4d71-a1c3-3d15b5518b84"));

                var task = tasks.List.ElementAt(0);

                tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);
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

                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }));

                var action = new Action<string, bool>((uid, app) =>
                {
                    ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                    var tasks = AsyncHelper.RunSync(() => tc.MyTasks(uid));

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

                var variables = AsyncHelper.RunSync(() => pvc.GetVariables(new ProcessVariablesQuery
                {
                    ProcessInstanceId = instances[0].Id,
                    ExcludeTaskVariables = true,
                    VariableName = WorkflowVariable.GLOBAL_APPROVALED_VARIABLE
                }));
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

                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }));

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                var tasks = AsyncHelper.RunSync(() => tc.MyTasks(users[0]));

                var task = tasks.List.ElementAt(0);

                tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                Thread.Sleep(2000);

                tasks = AsyncHelper.RunSync(() => tc.MyTasks(users[1]));

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

                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }));

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                var tasks = AsyncHelper.RunSync(() => tc.MyTasks(users[0]));

                var task = tasks.List.ElementAt(0);

                tc.Approvaled(new ApprovaleTaskCmd { TaskId = task.Id });

                Thread.Sleep(1000);

                tasks = AsyncHelper.RunSync(() => tc.MyTasks(users[1]));

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

                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, null, new Dictionary<string, object>
                {
                    ["UserTask_11w47k9s"] = users
                }));

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                foreach (var uid in users)
                {
                    var tasks = AsyncHelper.RunSync(() => tc.MyTasks(uid));

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

                while (count < 10)
                {
                    ProcessDefinition process = ctx.GetOrAddProcessDefinition(bpmnFile);

                    ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(process, new Dictionary<string, object>
                    {
                        ["UserTask_11w47k9s"] = users
                    }));

                    count += 1;
                }

                count = 0;
                while (count < 10)
                {
                    ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                    bool approvaled = true;
                    foreach (var uid in users)
                    {
                        var tasks = AsyncHelper.RunSync(() => tc.MyTasks(uid));

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { sid }));

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
            });

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { sid }));

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
                XDocument doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), LoadOptions.PreserveWhitespace);
                var elem = doc.Descendants(XName.Get("startEvent", BpmnXMLConstants.BPMN2_NAMESPACE)).First();
                elem.Attribute(XName.Get("formKey", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE)).Value = formKey;

                Deployment deployment = await pdc.Deploy(new ProcessDefinitionDeployer
                {
                    BpmnXML = doc.ToString(),
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

                ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() => client.Start(new StartProcessInstanceCmd[] { cmd }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Resources<TaskModel> myTasks = null;

                while (true)
                {
                    myTasks = await Complete(tc, uid, new Dictionary<string, object>
                        {
                            { "gender", 1 }
                        }).ConfigureAwait(false);

                    if (myTasks == null || myTasks.TotalCount <= 0)
                    {
                        break;
                    }
                }
            }).ConfigureAwait(false);

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
                XDocument doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), LoadOptions.PreserveWhitespace);
                var elem = doc.Descendants(XName.Get("startEvent", BpmnXMLConstants.BPMN2_NAMESPACE)).First();
                elem.Attribute(XName.Get("formKey", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE)).Value = formKey;

                Deployment deployment = await pdc.Deploy(new ProcessDefinitionDeployer
                {
                    BpmnXML = doc.ToString(),
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

                ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() => client.Start(new StartProcessInstanceCmd[] { cmd }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ITaskController tc = ctx.CreateWorkflowHttpProxy().GetTaskClient();

                Resources<TaskModel> myTasks = null;

                while (true)
                {
                    myTasks = await Complete(tc, uid, new Dictionary<string, object>
                        {
                            { "gender", 2 }
                        }).ConfigureAwait(false);

                    if (myTasks == null || myTasks.TotalCount <= 0)
                    {
                        break;
                    }
                }
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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[0], new Dictionary<string, object>
                    {
                        { "starter", new string[]{ uid } }
                    }));

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

                ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() => client.Start(new StartProcessInstanceCmd[] { cmd }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                var pit = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();

                var tasks = AsyncHelper.RunSync<Resources<TaskModel>>(() => pit.GetTasks(new ProcessInstanceTaskQuery() { ProcessInstanceId = instances[0].Id }));

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

                ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() => client.Start(new StartProcessInstanceCmd[] { cmd }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                var pit = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();

                var tasks = AsyncHelper.RunSync<Resources<TaskModel>>(() => pit.GetTasks(new ProcessInstanceTaskQuery() { ProcessInstanceId = instances[0].Id }));

                TaskModel task = tasks.List.FirstOrDefault(x => x.Status == "ASSIGNED");

                Assert.NotNull(task);

                AsyncHelper.RunSync(() => ctx.CreateWorkflowHttpProxy().GetTaskClient().CompleteTask(new CompleteTaskCmd
                {
                    TaskId = task.Id
                }));
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

                ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() => client.Start(new StartProcessInstanceCmd[] { cmd }));

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ProcessInstance inst = AsyncHelper.RunSync<ProcessInstance>(() => client.GetProcessInstanceById(instances[0].Id));

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ProcessInstance inst = AsyncHelper.RunSync<ProcessInstance>(() => client.Suspend(instances[0].Id));

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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                ProcessInstance inst = AsyncHelper.RunSync<ProcessInstance>(() => client.Suspend(instances[0].Id));

                Assert.NotNull(inst);
                Assert.Equal("SUSPENDED", inst.Status, ignoreCase: true);

                inst = AsyncHelper.RunSync<ProcessInstance>(() => client.Activate(instances[0].Id));

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
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.NotNull(instances);
                Assert.True(instances.Count() > 0);

                TerminateProcessInstanceCmd cmd = new TerminateProcessInstanceCmd(instances[0].Id, "测试终止流程");

                AsyncHelper.RunSync<ActionResult>(() => client.Terminate(cmd));

                ProcessInstance inst = AsyncHelper.RunSync<ProcessInstance>(() => client.GetProcessInstanceById(instances[0].Id));

                Assert.Null(inst);
            });

            Assert.NotNull(ex);
            Assert.IsType<Http400Exception>(ex);
        }

        [Theory]
        [InlineData("WebApi.bpmn")]
        public void 测试WebApi节点服务流程(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                {
                    ["startTimer"] = DateTime.Now.AddSeconds(4)
                }));
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("定时执行.bpmn")]
        [Order(2000)]
        public async Task 定时10秒后执行用户任务(string bpmnFile)
        {
            ProcessInstance[] insts = null;
            DateTime timerDateTime = DateTime.Now.AddSeconds(10);
            string uid = Guid.NewGuid().ToString();

            var ex = Record.Exception(() =>
            {
                insts = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                {
                    { "timerDateTime", timerDateTime}
                }));
            });

            Assert.Null(ex);

            ex = await Record.ExceptionAsync(async () =>
            {
                int retry = 0;
                Resources<TaskModel> tasks = null;
                while (retry != 2)
                {
                    Thread.Sleep(15000);

                    ITaskController client = ctx.CreateWorkflowHttpProxy().GetTaskClient();

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
        [Order(1000)]
        public void 定时10秒后取消用户任务(string bpmnFile)
        {
            ProcessInstance[] insts = null;
            DateTime timerDateTime = DateTime.Now.AddSeconds(10);

            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                insts = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                    {
                        { "timerDateTime", timerDateTime}
                    }));
            });

            Thread.Sleep(15000);

            Assert.Null(ex);

            ex = Record.Exception(() =>
            {
                IProcessInstanceHistoriceController hisClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceHistoriceClient();

                HistoricInstance hi = AsyncHelper.RunSync(() => hisClient.GetProcessInstanceById(insts[0].Id));
            });

            Assert.Null(ex);
        }


        [Theory]
        [InlineData("延时执行任务.bpmn")]
        [Order(4000)]
        public async Task 延时10秒后执行用户任务(string bpmnFile)
        {
            ProcessInstance[] insts = null;
            DateTime timerDateTime = DateTime.Now.AddSeconds(10);

            TimeSpan timespan = TimeSpan.FromTicks(timerDateTime.Ticks);

            //转换为ISO8601 duration格式字符串
            string duration = XmlConvert.ToString(timespan);

            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                insts = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                    {
                        { "timerDateTime", timerDateTime}
                    }));
            });

            Thread.Sleep(15000);

            Assert.Null(ex);

            ex = await Record.ExceptionAsync(async () =>
            {
                IProcessInstanceHistoriceController hisClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceHistoriceClient();

                HistoricInstance hi = await hisClient.GetProcessInstanceById(insts[0].Id).ConfigureAwait(false);
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
                ProcessInstance[] instances = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));
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
                insts = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid },
                    new Dictionary<string, object>
                    {
                        { "count", count },
                        { "start", start}
                    }));
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
                insts = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                    {
                        { "count", count },
                        { "start", start}
                    }));
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

                XDocument doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), LoadOptions.PreserveWhitespace);

                XElement elem = doc.Descendants(XName.Get("startEvent", BpmnXMLConstants.BPMN2_NAMESPACE)).First();

                XAttribute attr = elem.Attribute(XName.Get("formKey", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));
                attr.Value = formKey;
                xml = doc.ToString();

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

    }
}
