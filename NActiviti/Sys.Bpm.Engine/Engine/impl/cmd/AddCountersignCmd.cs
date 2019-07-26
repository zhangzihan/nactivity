///////////////////////////////////////////////////////////
//  AddContersignCmd.cs
//  Implementation of the Class AddContersignCmd
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Exceptions;
using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Engine.Bpmn.Rules;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sys.Workflow.Engine.Impl.Cmd
{
    /// <summary>
    /// 加签
    /// </summary>
    public class AddCountersignCmd : ICommand<ITask[]>
    {
        private readonly string taskId;
        private readonly string[] assignees;
        private readonly string tenantId;
        private readonly IUserServiceProxy userService;

        private static readonly Regex VARNAME_PATTERN = new Regex("\\$\\{(.*?)\\}");

        public AddCountersignCmd(string taskId, string[] assignees, string tenantId)
        {
            this.taskId = taskId;
            this.assignees = assignees;
            this.tenantId = tenantId;

            userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();
        }

        private class CreateChildExecutionCmd : ICommand<IExecutionEntity>
        {
            private readonly IExecutionEntity parent;

            public CreateChildExecutionCmd(IExecutionEntity parent)
            {
                this.parent = parent;
            }

            public IExecutionEntity Execute(ICommandContext commandContext)
            {
                IExecutionEntity newExecution = commandContext.ExecutionEntityManager.CreateChildExecution(parent);

                return newExecution;
            }
        }

        public ITask[] Execute(ICommandContext commandContext)
        {
            ProcessEngineConfigurationImpl pec = commandContext.ProcessEngineConfiguration;
            IRuntimeService runtimeService = pec.RuntimeService;
            ITaskService taskService = pec.TaskService;
            IIdGenerator idGenerator = pec.IdGenerator;

            ITask task = taskService.CreateTaskQuery().SetTaskId(taskId).SingleResult();
            IExecutionEntity execution = runtimeService.CreateExecutionQuery().SetExecutionId(task.ExecutionId).SingleResult() as IExecutionEntity;
            IExecutionEntity parent = execution.Parent;

            //查找当前待追加人员是否已经存在在任务列表中,proc_inst_id_
            IList<ITask> assignTasks = taskService.CreateTaskQuery()
                .SetProcessInstanceId(execution.ProcessInstanceId)
                .SetTaskAssigneeIds(assignees)
                .List();

            var users = assignees.Where(x => assignTasks.Any(y => x == y.Assignee) == false).ToList();

            if (users.Count == 0)
            {
                throw new NotFoundAssigneeException();
            }

            if (parent.IsMultiInstanceRoot && parent.CurrentFlowElement is UserTask mTask)
            {
                string varName = mTask.LoopCharacteristics.InputDataItem;
                Match match = VARNAME_PATTERN.Match(varName);
                varName = match.Groups[1].Value;

                IList<string> list = parent.GetLoopVariable<IList<string>>(varName);

                parent.SetLoopVariable(varName, users.Union(list).Distinct().ToArray());
            }

            IList<ITask> tasks = new List<ITask>(users.Count);
            foreach (var assignee in users)
            {
                //创建父活动的子活动
                IExecutionEntity newExecution = pec.CommandExecutor.Execute(new CreateChildExecutionCmd(parent));
                //设置为激活 状态
                newExecution.IsActive = true;
                newExecution.ActivityId = execution.ActivityId;
                newExecution.BusinessKey = execution.BusinessKey;
                //该属性表示创建的newExecution对象为分支，非常重要,不可缺少
                newExecution.IsConcurrent = execution.IsConcurrent;
                newExecution.IsScope = false;
                ITaskEntity taskEntity = pec.CommandExecutor.Execute(new NewTaskCmd(pec.IdGenerator.GetNextId()));
                taskEntity.ProcessDefinitionId = task.ProcessDefinitionId;
                taskEntity.TaskDefinitionKey = task.TaskDefinitionKey;
                taskEntity.ProcessInstanceId = task.ProcessInstanceId;
                taskEntity.ExecutionId = newExecution.Id;
                taskEntity.Name = task.Name;

                if (string.IsNullOrWhiteSpace(taskEntity.Id))
                {
                    string taskId = idGenerator.GetNextId();
                    taskEntity.Id = taskId;
                }
                taskEntity.Execution = newExecution;
                taskEntity.Assignee = assignee;
                if (string.IsNullOrWhiteSpace(assignee) == false)
                {
                    //TODO: 考虑性能问题，暂时不要获取人员信息
                    //taskEntity.AssigneeUser = AsyncHelper.RunSync(() => userService.GetUser(assignee))?.FullName;
                }
                taskEntity.TenantId = task.TenantId;
                taskEntity.FormKey = task.FormKey;
                taskEntity.IsAppend = true;

                taskService.SaveTask(taskEntity);

                tasks.Add(taskEntity);
            }

            //修改执行实例父级实例变量数和活动实例变量数
            int nrOfInstances = parent.GetLoopVariable<int>(MultiInstanceActivityBehavior.NUMBER_OF_INSTANCES);
            int nrOfActiveIntance = parent.GetLoopVariable<int>(MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES);
            int nrOfCompletedInstances = parent.GetLoopVariable<int>(MultiInstanceActivityBehavior.NUMBER_OF_COMPLETED_INSTANCES);

            parent.SetLoopVariable(MultiInstanceActivityBehavior.NUMBER_OF_INSTANCES, nrOfInstances + users.Count);
            parent.SetLoopVariable(MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES, nrOfInstances - nrOfCompletedInstances + users.Count);

            return tasks.ToArray();
        }
    }
}
