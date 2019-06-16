using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.JobExecutors;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Runtime;
using System;

namespace Sys.Workflow.Engine.Impl.Cmd
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

        public IJobEntity Execute(ICommandContext commandContext)
        {
            IJobEntity timer = commandContext.JobEntityManager.Create();
            timer.Id = commandContext.ProcessEngineConfiguration.IdGenerator.GetNextId();
            timer.Duedate = this.startDate;
            timer.Exclusive = JobFields.DEFAULT_EXCLUSIVE;
            timer.JobHandlerConfiguration = startActivityId;
            timer.JobHandlerType = TimerStartEventJobHandler.TYPE;
            timer.JobType = JobFields.JOB_TYPE_TIMER;

            IProcessDefinitionEntity pd = commandContext.ProcessDefinitionEntityManager.FindLatestProcessDefinitionByKey(processDefinitionId);
            if (pd == null)
            {
                throw new ActivitiException("Could not find process definition needed for timer start event");
            }
            timer.ProcessDefinitionId = pd.Id;

            commandContext.JobEntityManager.Insert(timer);

            return timer;
        }
    }
}
