using System;

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;
    using System.Collections.Generic;

    /// 

    [Serializable]
    public class DeleteDeadLetterJobCmd : ICommand<object>
    {
        private const long serialVersionUID = 1L;

        protected internal string timerJobId;

        public DeleteDeadLetterJobCmd(string timerJobId)
        {
            this.timerJobId = timerJobId;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IDeadLetterJobEntity jobToDelete = GetJobToDelete(commandContext);

            SendCancelEvent(jobToDelete);

            commandContext.DeadLetterJobEntityManager.Delete(jobToDelete);
            return null;
        }

        protected internal virtual void SendCancelEvent(IDeadLetterJobEntity jobToDelete)
        {
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, jobToDelete));
            }
        }

        protected internal virtual IDeadLetterJobEntity GetJobToDelete(ICommandContext commandContext)
        {
            if (ReferenceEquals(timerJobId, null))
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }
            //if (log.DebugEnabled)
            //{
            //    log.debug("Deleting job {}", timerJobId);
            //}

            IDeadLetterJobEntity job = commandContext.DeadLetterJobEntityManager.FindById<IDeadLetterJobEntity>(new KeyValuePair<string, object>("id", timerJobId));
            if (job == null)
            {
                throw new ActivitiObjectNotFoundException("No dead letter job found with id '" + timerJobId + "'", typeof(IJob));
            }

            return job;
        }

    }

}