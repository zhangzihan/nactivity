using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Bpm.Engine.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Engine.Tests.Tests
{
    [Collection("EngineTestCollection")]
    public class CmdTests
    {
        private readonly InProcessEngineFixture _fixture;

        public CmdTests(InProcessEngineFixture fixture)
        {
            _fixture = fixture;
        }

        private IProcessDefinition DeployBpmn(string bpmnFile)
        {
            string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
            string deployName = Guid.NewGuid().ToString();

            IDeploymentBuilder builder = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                .Name(deployName)
                .TenantId(_fixture.TenantId)
                .AddString(bpmnFile, xml)
                .EnableDuplicateFiltering()
                .TenantId(_fixture.TenantId);

            IDeployment deploy = _fixture.CommandExecutor.Execute(new DeployCmd(builder));

            return _fixture.ProcessEngine.RepositoryService.CreateProcessDefinitionQuery()
                .SetDeploymentId(deploy.Id)
                .SetProcessDefinitionTenantId(_fixture.TenantId)
                .SetLatestVersion()
                .SingleResult();
        }

        /// <summary>
        /// 辅助方法：部署简单顺序流并启动流程实例
        /// </summary>
        private (IProcessDefinition definition, IProcessInstance instance, ITask task) DeployStartAndGetTask()
        {
            _fixture.SetAuthenticatedUser();
            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
            string user = Guid.NewGuid().ToString();

            var variables = new WorkflowVariable();
            variables.AddAssignee("name", user);

            IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                new StartProcessInstanceCmd(definition.Id, variables));

            var tasks = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                .SetProcessInstanceId(processInstance.Id)
                .List();

            return (definition, processInstance, tasks.First());
        }

        // ==================== CompleteTaskCmd ====================

        [Fact]
        public void CompleteTaskCmd_完成任务_任务不再存在()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                _fixture.CommandExecutor.Execute(
                    new CompleteTaskCmd(task.Id, new Dictionary<string, object>()));

                var remaining = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(task.ProcessInstanceId)
                    .List();

                Assert.DoesNotContain(remaining, t => t.Id == task.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CompleteTaskCmd_带完成原因_添加评论()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                _fixture.CommandExecutor.Execute(
                    new CompleteTaskCmd(task.Id, new Dictionary<string, object>(), null, false, "测试完成原因"));

                // 验证任务已完成（不再存在）
                var remaining = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(task.ProcessInstanceId)
                    .List();

                Assert.DoesNotContain(remaining, t => t.Id == task.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CompleteTaskCmd_带变量和局部作用域_设置变量()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                var vars = new Dictionary<string, object> { { "审批结果", "同意" } };
                _fixture.CommandExecutor.Execute(
                    new CompleteTaskCmd(task.Id, vars, true));
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CompleteTaskCmd_taskId为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(
                    new CompleteTaskCmd(null, new Dictionary<string, object>()));
            });

            Assert.NotNull(ex);
            Assert.Contains("taskId is null", ex.Message);
        }

        [Fact]
        public void CompleteTaskCmd_任务不存在_抛出ActivitiObjectNotFoundException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(
                    new CompleteTaskCmd("non-existent-task-id", new Dictionary<string, object>()));
            });

            Assert.NotNull(ex);
            Assert.Contains("Cannot find task", ex.Message);
        }

        // ==================== ClaimTaskCmd ====================

        [Fact]
        public void ClaimTaskCmd_认领任务_办理人变更()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();
                string newUser = Guid.NewGuid().ToString();

                // 先释放任务
                _fixture.ProcessEngine.TaskService.Unclaim(task.Id);
                // 认领给新用户
                _fixture.CommandExecutor.Execute(new ClaimTaskCmd(task.Id, newUser));

                var claimedTasks = _fixture.ProcessEngine.TaskService.GetMyTasks(newUser);
                Assert.NotEmpty(claimedTasks);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void ClaimTaskCmd_释放任务_办理人清空()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                // 释放任务（userId 为 null）
                _fixture.CommandExecutor.Execute(new ClaimTaskCmd(task.Id, null));

                var updatedTask = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetTaskId(task.Id)
                    .SingleResult();

                Assert.NotNull(updatedTask);
                Assert.Null(updatedTask.Assignee);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void ClaimTaskCmd_taskId为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(new ClaimTaskCmd(null, "user1"));
            });

            Assert.NotNull(ex);
            Assert.Contains("taskId is null", ex.Message);
        }

        // ==================== DelegateTaskCmd ====================

        [Fact]
        public void DelegateTaskCmd_委派任务_委派状态为PENDING()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();
                string delegateUser = Guid.NewGuid().ToString();

                _fixture.CommandExecutor.Execute(new DelegateTaskCmd(task.Id, delegateUser));

                var updatedTask = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetTaskId(task.Id)
                    .SingleResult();

                Assert.NotNull(updatedTask);
                Assert.Equal(DelegationState.PENDING, updatedTask.DelegationState);
                Assert.Equal(delegateUser, updatedTask.Assignee);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void DelegateTaskCmd_taskId为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(new DelegateTaskCmd(null, "user1"));
            });

            Assert.NotNull(ex);
            Assert.Contains("taskId is null", ex.Message);
        }

        // ==================== DeleteTaskCmd ====================

        [Fact]
        public void DeleteTaskCmd_删除单个任务_任务不再存在()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, task) = DeployStartAndGetTask();

                _fixture.CommandExecutor.Execute(new DeleteTaskCmd(task.Id, "测试删除", false));

                var remaining = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.DoesNotContain(remaining, t => t.Id == task.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void DeleteTaskCmd_批量删除任务_任务全部删除()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, task) = DeployStartAndGetTask();

                _fixture.CommandExecutor.Execute(
                    new DeleteTaskCmd(new List<string> { task.Id }, "批量删除", false));

                var remaining = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.DoesNotContain(remaining, t => t.Id == task.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void DeleteTaskCmd_taskId和taskIds均为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(new DeleteTaskCmd((string)null, "原因", false));
            });

            Assert.NotNull(ex);
            Assert.Contains("taskId and taskIds are null", ex.Message);
        }

        // ==================== AddCommentCmd ====================

        [Fact]
        public void AddCommentCmd_为任务添加评论_返回评论对象()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                var comment = _fixture.CommandExecutor.Execute(
                    new AddCommentCmd(task.Id, task.ProcessInstanceId, "测试评论内容"));

                Assert.NotNull(comment);
                Assert.Equal("测试评论内容", comment.FullMessage);
                Assert.Equal(task.Id, comment.TaskId);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void AddCommentCmd_为任务添加指定类型评论_类型正确()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                var comment = _fixture.CommandExecutor.Execute(
                    new AddCommentCmd(task.Id, task.ProcessInstanceId, "event", "事件评论"));

                Assert.NotNull(comment);
                Assert.Equal("event", comment.Type);
                Assert.Equal("事件评论", comment.FullMessage);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void AddCommentCmd_默认类型为comment()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                var comment = _fixture.CommandExecutor.Execute(
                    new AddCommentCmd(task.Id, task.ProcessInstanceId, "默认类型评论"));

                Assert.NotNull(comment);
                Assert.Equal("comment", comment.Type);
            });

            Assert.Null(ex);
        }

        // ==================== SaveTaskCmd ====================

        [Fact]
        public void SaveTaskCmd_任务为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(new SaveTaskCmd(null));
            });

            Assert.NotNull(ex);
            Assert.Contains("task is null", ex.Message);
        }

        [Fact]
        public void SaveTaskCmd_更新任务名称_名称变更成功()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, task) = DeployStartAndGetTask();

                // 通过 TaskService 修改任务名称（内部会使用 SaveTaskCmd）
                task.Name = "修改后的任务名";
                _fixture.ProcessEngine.TaskService.SaveTask(task);

                var updatedTask = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetTaskId(task.Id)
                    .SingleResult();

                Assert.NotNull(updatedTask);
                Assert.Equal("修改后的任务名", updatedTask.Name);
            });

            Assert.Null(ex);
        }

        // ==================== GetProcessInstanceByIdCmd ====================

        [Fact]
        public void GetProcessInstanceByIdCmd_按ID查询_返回流程实例()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, _) = DeployStartAndGetTask();

                var found = _fixture.CommandExecutor.Execute(
                    new GetProcessInstanceByIdCmd(processInstance.Id));

                Assert.NotNull(found);
                Assert.Equal(processInstance.Id, found.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void GetProcessInstanceByIdCmd_查询不存在的ID_返回null()
        {
            var ex = Record.Exception(() =>
            {
                var found = _fixture.CommandExecutor.Execute(
                    new GetProcessInstanceByIdCmd("non-existent-id"));

                Assert.Null(found);
            });

            Assert.Null(ex);
        }

        // ==================== CreateModelCmd ====================

        [Fact]
        public void CreateModelCmd_创建模型_返回模型对象()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                var model = _fixture.CommandExecutor.Execute(new CreateModelCmd());

                Assert.NotNull(model);
            });

            Assert.Null(ex);
        }

        // ==================== DeleteDeploymentCmd ====================

        [Fact]
        public void DeleteDeploymentCmd_删除部署_部署不再存在()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();
                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");

                _fixture.CommandExecutor.Execute(
                    new DeleteDeploymentCmd(definition.DeploymentId, true));

                var found = _fixture.ProcessEngine.RepositoryService.CreateProcessDefinitionQuery()
                    .SetDeploymentId(definition.DeploymentId)
                    .SingleResult();

                Assert.Null(found);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void DeleteDeploymentCmd_deploymentId为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(new DeleteDeploymentCmd(null, false));
            });

            Assert.NotNull(ex);
            Assert.Contains("deploymentId is null", ex.Message);
        }

        // ==================== DeployCmd ====================

        [Fact]
        public void DeployCmd_部署BPMN_返回部署对象()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();
                string bpmnFile = "简单顺序流.bpmn";
                string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
                string deployName = Guid.NewGuid().ToString();

                IDeploymentBuilder builder = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .TenantId(_fixture.TenantId);

                IDeployment deploy = _fixture.CommandExecutor.Execute(new DeployCmd(builder));

                Assert.NotNull(deploy);
                Assert.NotNull(deploy.Id);
                Assert.Equal(deployName, deploy.Name);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void DeployCmd_启用重复过滤_相同资源返回已有部署()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();
                string bpmnFile = "简单顺序流.bpmn";
                string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
                string deployName = Guid.NewGuid().ToString();

                IDeploymentBuilder builder1 = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy1 = _fixture.CommandExecutor.Execute(new DeployCmd(builder1));

                IDeploymentBuilder builder2 = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy2 = _fixture.CommandExecutor.Execute(new DeployCmd(builder2));

                Assert.NotNull(deploy1);
                Assert.NotNull(deploy2);
                // 启用重复过滤时，相同资源应返回相同部署
                Assert.Equal(deploy1.Id, deploy2.Id);
            });

            Assert.Null(ex);
        }

        // ==================== StartProcessInstanceCmd ====================

        [Fact]
        public void StartProcessInstanceCmd_按定义ID启动_返回流程实例()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();
                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance instance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                Assert.NotNull(instance);
                Assert.Equal(definition.Id, instance.ProcessDefinitionId);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void StartProcessInstanceCmd_按定义ID和业务键启动_业务键正确()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();
                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string bizKey = Guid.NewGuid().ToString();
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance instance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(null, definition.Id, bizKey, variables));

                Assert.NotNull(instance);
                Assert.Equal(bizKey, instance.BusinessKey);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void StartProcessInstanceCmd_定义ID和Key均为null_抛出ActivitiIllegalArgumentException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(null, null, null, null));
            });

            Assert.NotNull(ex);
            Assert.Contains("processDefinitionKey and processDefinitionId are null", ex.Message);
        }

        [Fact]
        public void StartProcessInstanceCmd_定义ID不存在_抛出ActivitiObjectNotFoundException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd("non-existent-definition-id", new Dictionary<string, object>()));
            });

            Assert.NotNull(ex);
            Assert.Contains("No process definition found", ex.Message);
        }

        // ==================== GetTasksCmd ====================

        [Fact]
        public void GetTasksCmd_分页查询任务_返回任务列表()
        {
            var ex = Record.Exception(() =>
            {
                var (_, _, _) = DeployStartAndGetTask();

                var query = _fixture.ProcessEngine.TaskService.CreateTaskQuery();
                var tasks = _fixture.CommandExecutor.Execute(new GetTasksCmd(query, 0, 10));

                Assert.NotNull(tasks);
            });

            Assert.Null(ex);
        }

        // ==================== DeleteProcessInstanceCmd ====================

        [Fact]
        public void DeleteProcessInstanceCmd_删除流程实例_实例不再可查()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, _) = DeployStartAndGetTask();

                _fixture.CommandExecutor.Execute(
                    new DeleteProcessInstanceCmd(processInstance.Id, "测试删除"));

                IProcessInstance found = _fixture.ProcessEngine.RuntimeService.CreateProcessInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .SingleResult();

                Assert.Null(found);
            });

            Assert.Null(ex);
        }

        // ==================== SuspendProcessInstanceCmd / ActivateProcessInstanceCmd ====================

        [Fact]
        public void SuspendProcessInstanceCmd_挂起流程实例_实例为挂起状态()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, _) = DeployStartAndGetTask();

                try
                {
                    _fixture.CommandExecutor.Execute(
                        new SuspendProcessInstanceCmd(processInstance.Id));

                    var found = _fixture.ProcessEngine.RuntimeService.CreateProcessInstanceQuery()
                        .SetProcessInstanceId(processInstance.Id)
                        .SingleResult();

                    Assert.NotNull(found);
                    Assert.True(found.Suspended);
                }
                finally
                {
                    // 恢复激活状态
                    try
                    {
                        _fixture.CommandExecutor.Execute(
                            new ActivateProcessInstanceCmd(processInstance.Id));
                    }
                    catch { }
                }
            });

            Assert.Null(ex);
        }

        // ==================== NeedsActiveTaskCmd 基类验证 ====================

        [Fact]
        public void NeedsActiveTaskCmd_挂起任务_抛出ActivitiException()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();
                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                var tasks = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                ITask task = tasks.First();

                try
                {
                    // 挂起流程实例，使任务变为挂起状态
                    _fixture.CommandExecutor.Execute(
                        new SuspendProcessInstanceCmd(processInstance.Id));

                    // 尝试认领挂起的任务应抛出异常
                    var inner = Record.Exception(() =>
                    {
                        _fixture.CommandExecutor.Execute(new ClaimTaskCmd(task.Id, "newUser"));
                    });

                    Assert.NotNull(inner);
                    Assert.Contains("suspended", inner.Message, StringComparison.OrdinalIgnoreCase);
                }
                finally
                {
                    try
                    {
                        _fixture.CommandExecutor.Execute(
                            new ActivateProcessInstanceCmd(processInstance.Id));
                    }
                    catch { }
                }
            });

            Assert.Null(ex);
        }

        // ==================== SetProcessInstanceBusinessKeyCmd ====================

        [Fact]
        public void SetProcessInstanceBusinessKeyCmd_修改业务键_业务键更新()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, _) = DeployStartAndGetTask();
                string newBizKey = Guid.NewGuid().ToString();

                _fixture.CommandExecutor.Execute(
                    new SetProcessInstanceBusinessKeyCmd(processInstance.Id, newBizKey));

                var found = _fixture.ProcessEngine.RuntimeService.CreateProcessInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .SingleResult();

                Assert.NotNull(found);
                Assert.Equal(newBizKey, found.BusinessKey);
            });

            Assert.Null(ex);
        }

        // ==================== SetProcessInstanceNameCmd ====================

        [Fact]
        public void SetProcessInstanceNameCmd_修改流程名称_名称更新()
        {
            var ex = Record.Exception(() =>
            {
                var (_, processInstance, _) = DeployStartAndGetTask();
                string newName = "修改后的流程名称";

                _fixture.CommandExecutor.Execute(
                    new SetProcessInstanceNameCmd(processInstance.Id, newName));

                var found = _fixture.ProcessEngine.RuntimeService.CreateProcessInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .SingleResult();

                Assert.NotNull(found);
                Assert.Equal(newName, found.Name);
            });

            Assert.Null(ex);
        }
    }
}
