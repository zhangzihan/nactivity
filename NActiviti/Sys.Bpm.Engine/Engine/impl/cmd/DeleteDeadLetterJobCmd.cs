using System;

namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
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

        public virtual object execute(ICommandContext commandContext)
        {
            IDeadLetterJobEntity jobToDelete = getJobToDelete(commandContext);

            sendCancelEvent(jobToDelete);

            commandContext.DeadLetterJobEntityManager.delete(jobToDelete);
            return null;
        }

        protected internal virtual void sendCancelEvent(IDeadLetterJobEntity jobToDelete)
        {
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, jobToDelete));
            }
        }

        protected internal virtual IDeadLetterJobEntity getJobToDelete(ICommandContext commandContext)
        {
            if (string.ReferenceEquals(timerJobId, null))
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }
            //if (log.DebugEnabled)
            //{
            //    log.debug("Deleting job {}", timerJobId);
            //}

            IDeadLetterJobEntity job = commandContext.DeadLetterJobEntityManager.findById<IDeadLetterJobEntity>(new KeyValuePair<string, object>("id", timerJobId));
            if (job == null)
            {
                throw new ActivitiObjectNotFoundException("No dead letter job found with id '" + timerJobId + "'", typeof(IJob));
            }

            return job;
        }

    }

}