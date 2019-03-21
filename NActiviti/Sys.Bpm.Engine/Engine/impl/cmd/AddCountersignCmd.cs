///////////////////////////////////////////////////////////
//  AddContersignCmd.cs
//  Implementation of the Class AddContersignCmd
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using org.activiti.bpmn.model;
using org.activiti.engine.exceptions;
using org.activiti.engine.impl.bpmn.behavior;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.context;
using org.activiti.engine.impl.interceptor;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.impl.util;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace org.activiti.engine.impl.cmd
{
    /// <summary>
    /// 加签
    /// </summary>
    public class AddCountersignCmd : ICommand<ITask[]>
    {
        private readonly string taskId;
        private readonly string[] assignees;
        private readonly string tenantId;

        public AddCountersignCmd(string taskId, string[] assignees, string tenantId)
        {
            this.taskId = taskId;
            this.assignees = assignees;
            this.tenantId = tenantId;
        }

        public ITask[] execute(ICommandContext commandContext)
        {
            ProcessEngineConfigurationImpl pec = commandContext.ProcessEngineConfiguration;
            IRuntimeService runtimeService = pec.RuntimeService;
            ITaskService taskService = pec.TaskService;
            IIdGenerator idGenerator = pec.IdGenerator;

            ITask task = taskService.createTaskQuery().taskId(taskId).singleResult();
            IExecutionEntity execution = runtimeService.createExecutionQuery().executionId(task.ExecutionId).singleResult() as IExecutionEntity;
            IExecutionEntity parent = execution.Parent;

            IList<ITask> assignTasks = taskService.createTaskQuery().taskAssigneeIds(assignees).taskTenantId(execution.TenantId).list();

            var users = assignees.Where(x => assignTasks.Any(y => x == y.Assignee) == false);

            if (users?.Count() == 0)
            {
                throw new NotFoundAssigneeException();
            }

            if (parent.IsMultiInstanceRoot && parent.CurrentFlowElement is UserTask mTask && users.Count() > 0)
            {
                string varName = mTask.LoopCharacteristics.InputDataItem;
                var match = new Regex("\\$\\{(.*?)\\}").Match(varName);
                varName = match.Groups[1].Value;

                IList<string> list = parent.GetLoopVariable<IList<string>>(varName);

                parent.SetLoopVariable(varName, users.Union(list).Distinct().ToArray());
            }

            IList<ITask> tasks = new List<ITask>(users.Count());
            foreach (var assignee in users)
            {
                //创建父活动的子活动
                IExecutionEntity newExecution = pec.ExecutionEntityManager.createChildExecution(parent);//.createExecution();
                //设置为激活 状态
                newExecution.IsActive = true;
                newExecution.ActivityId = execution.ActivityId;
                newExecution.BusinessKey = execution.BusinessKey;
                //该属性表示创建的newExecution对象为分支，非常重要,不可缺少
                newExecution.IsConcurrent = execution.IsConcurrent;
                newExecution.IsScope = false;
                ITaskEntity taskEntity = pec.CommandExecutor.execute(new NewTaskCmd(pec.IdGenerator.NextId));
                taskEntity.ProcessDefinitionId = task.ProcessDefinitionId;
                taskEntity.TaskDefinitionKey = task.TaskDefinitionKey;
                taskEntity.ProcessInstanceId = task.ProcessInstanceId;
                taskEntity.ExecutionId = newExecution.Id;
                taskEntity.Name = task.Name;

                string taskId = idGenerator.NextId;
                taskEntity.Id = taskId;
                taskEntity.Execution = newExecution;
                taskEntity.Assignee = assignee;
                taskEntity.TenantId = task.TenantId;
                taskEntity.FormKey = task.FormKey;
                taskEntity.IsAppend = true;

                taskService.saveTask(taskEntity);

                tasks.Add(taskEntity);
            }

            if (users.Count() > 0)
            {
                //修改执行实例父级实例变量数和活动实例变量数
                int loopCounter = parent.GetLoopVariable<int>(MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES);
                int nrOfCompletedInstances = parent.GetLoopVariable<int>(MultiInstanceActivityBehavior.NUMBER_OF_COMPLETED_INSTANCES);
                parent.SetLoopVariable(MultiInstanceActivityBehavior.NUMBER_OF_INSTANCES, loopCounter + 1);
                parent.SetLoopVariable(MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES, loopCounter - nrOfCompletedInstances + users.Count());
            }

            return tasks.ToArray();
        }
    }
}
