using System;

namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
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
            if (job is null)
            {
                throw new ActivitiObjectNotFoundException("No dead letter job found with id '" + timerJobId + "'", typeof(IJob));
            }

            return job;
        }

    }

}