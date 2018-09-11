using System;

namespace org.activiti.engine.impl.cmd
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using Sys;
    using System.Collections.Generic;

    /// 
    /// 

    [Serializable]
    public class DeleteJobCmd : ICommand<object>
    {
        private const long serialVersionUID = 1L;

        private static readonly ILogger<DeleteJobCmd> log = ProcessEngineServiceProvider.LoggerService<DeleteJobCmd>();

        protected internal string jobId;

        public DeleteJobCmd(string jobId)
        {
            this.jobId = jobId;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            IJobEntity jobToDelete = getJobToDelete(commandContext);

            sendCancelEvent(jobToDelete);

            commandContext.JobEntityManager.delete(jobToDelete);
            return null;
        }

        protected internal virtual void sendCancelEvent(IJobEntity jobToDelete)
        {
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, jobToDelete));
            }
        }

        protected internal virtual IJobEntity getJobToDelete(ICommandContext commandContext)
        {
            if (string.ReferenceEquals(jobId, null))
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Deleting job {jobId}");
            }

            IJobEntity job = commandContext.JobEntityManager.findById<IJobEntity>(new KeyValuePair<string, object>("id", jobId));
            if (job == null)
            {
                throw new ActivitiObjectNotFoundException("No job found with id '" + jobId + "'", typeof(IJob));
            }

            // We need to check if the job was locked, ie acquired by the job acquisition thread
            // This happens if the the job was already acquired, but not yet executed.
            // In that case, we can't allow to delete the job.
            if (!string.ReferenceEquals(job.LockOwner, null))
            {
                throw new ActivitiException("Cannot delete job when the job is being executed. Try again later.");
            }
            return job;
        }

    }

}