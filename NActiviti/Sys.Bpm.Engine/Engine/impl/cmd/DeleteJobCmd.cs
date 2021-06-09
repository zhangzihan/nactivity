using System;

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;
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

        public virtual object Execute(ICommandContext commandContext)
        {
            IJobEntity jobToDelete = GetJobToDelete(commandContext);

            SendCancelEvent(jobToDelete);

            commandContext.JobEntityManager.Delete(jobToDelete);
            return null;
        }

        protected internal virtual void SendCancelEvent(IJobEntity jobToDelete)
        {
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, jobToDelete));
            }
        }

        protected internal virtual IJobEntity GetJobToDelete(ICommandContext commandContext)
        {
            if (jobId is null)
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Deleting job {jobId}");
            }

            IJobEntity job = commandContext.JobEntityManager.FindById<IJobEntity>(new KeyValuePair<string, object>("id", jobId));
            if (job is null)
            {
                throw new ActivitiObjectNotFoundException("No job found with id '" + jobId + "'", typeof(IJob));
            }

            // We need to check if the job was locked, ie acquired by the job acquisition thread
            // This happens if the the job was already acquired, but not yet executed.
            // In that case, we can't allow to delete the job.
            if (job.LockOwner is object)
            {
                throw new ActivitiException("Cannot delete job when the job is being executed. Try again later.");
            }
            return job;
        }

    }

}