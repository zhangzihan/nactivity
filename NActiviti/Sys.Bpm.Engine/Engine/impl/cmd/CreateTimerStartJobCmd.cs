using Newtonsoft.Json.Linq;
using org.activiti.bpmn.model;
using org.activiti.engine.impl.agenda;
using org.activiti.engine.impl.interceptor;
using org.activiti.engine.impl.jobexecutor;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.engine.impl.cmd
{
    /// <summary>
    /// 流程定时启动命令
    /// </summary>
    public class CreateTimerStartJobCmd : ICommand<IJobEntity>
    {
        private readonly DateTime startDate;
        private readonly string processDefinitionId;
        private readonly string startActivityId;

        /// <summary>
        /// 流程定时启动命令
        /// </summary>
        /// <param name="processDefinitionId">流程定义id</param>
        /// <param name="startDate">启动时间</param>
        /// <param name="startActivityId">流程启动开始节点id,如果为空从start节点开始启动流程.</param>
        public CreateTimerStartJobCmd(string processDefinitionId, DateTime startDate, string startActivityId = null)
        {
            this.processDefinitionId = processDefinitionId;
            this.startDate = startDate;
            this.startActivityId = startActivityId;
        }

        public IJobEntity execute(ICommandContext commandContext)
        {
            IJobEntity timer = commandContext.JobEntityManager.create();
            timer.Id = commandContext.ProcessEngineConfiguration.IdGenerator.NextId;
            timer.Duedate = this.startDate;
            timer.Exclusive = Job_Fields.DEFAULT_EXCLUSIVE;
            timer.JobHandlerConfiguration = startActivityId;
            timer.JobHandlerType = TimerStartEventJobHandler.TYPE;
            timer.JobType = Job_Fields.JOB_TYPE_TIMER;

            IProcessDefinitionEntity pd = commandContext.ProcessDefinitionEntityManager.findLatestProcessDefinitionByKey(processDefinitionId);
            if (pd == null)
            {
                throw new ActivitiException("Could not find process definition needed for timer start event");
            }
            timer.ProcessDefinitionId = pd.Id;

            commandContext.JobEntityManager.insert(timer);

            return timer;
        }
    }
}
