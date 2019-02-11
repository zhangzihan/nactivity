///////////////////////////////////////////////////////////
//  AddContersignCmd.cs
//  Implementation of the Class AddContersignCmd
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

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

namespace org.activiti.engine.impl.cmd
{
    /// <summary>
    /// 加签
    /// </summary>
    public class AddCountersignCmd : ICommand<object>
    {
        private readonly string executionId;
        private readonly string assignee;

        public AddCountersignCmd(string executionId, string assignee)
        {
            this.executionId = executionId;
            this.assignee = assignee;
        }

        public object execute(ICommandContext commandContext)
        {
            ProcessEngineConfigurationImpl pec = commandContext.ProcessEngineConfiguration;
            IRuntimeService runtimeService = pec.RuntimeService;
            ITaskService taskService = pec.TaskService;
            IIdGenerator idGenerator = pec.IdGenerator;
            IExecution execution = runtimeService.createExecutionQuery().executionId(executionId).singleResult();
            IExecutionEntity ee = (IExecutionEntity)execution;
            IExecutionEntity parent = ee.Parent;
            //创建父活动的子活动
            IExecutionEntity newExecution = pec.ExecutionEntityManager.createChildExecution(parent);//.createExecution();

            //设置 为 激活 状态
            newExecution.IsActive = true;
            //该 属性 表示 创建 的 newExecution 对象 为 分支， 非常 重要, 不可缺少
            newExecution.IsConcurrent = true;
            newExecution.IsScope = false;
            ITask newTask = taskService.createTaskQuery().executionId(executionId).singleResult();
            ITaskEntity t = (ITaskEntity)newTask;
            ITaskEntity taskEntity = pec.CommandExecutor.execute(new NewTaskCmd(pec.IdGenerator.NextId));
            //taskEntity.TaskDefinitionKey = t.TaskDefinitionKey;
            taskEntity.ProcessDefinitionId = t.ProcessDefinitionId;
            taskEntity.TaskDefinitionKey = t.TaskDefinitionKey;
            taskEntity.ProcessInstanceId = t.ProcessInstanceId;
            taskEntity.ExecutionId = newExecution.Id;
            taskEntity.Name = newTask.Name;

            string taskId = idGenerator.NextId;
            taskEntity.Id = taskId;
            taskEntity.Execution = newExecution;
            taskEntity.Assignee = assignee;
            taskService.saveTask(taskEntity);

            //修改执行实例父级实例变量数和活动实例变量数
            int loopCounter = newExecution.GetLoopVariable<int>("nrOfInstances");
            int nrOfCompletedInstances = newExecution.GetLoopVariable<int>("nrOfActiveInstances");
            parent.SetLoopVariable("nrOfInstances", loopCounter + 1);
            parent.SetLoopVariable("nrOfActiveInstances", loopCounter - nrOfCompletedInstances + 1);

            return null;
        }
    }
}
