using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Runtime;

    public class JobEntityManagerImpl : AbstractEntityManager<IJobEntity>, IJobEntityManager
    {

        protected internal IJobDataManager jobDataManager;

        public JobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        protected internal override IDataManager<IJobEntity> DataManager
        {
            get
            {
                return jobDataManager;
            }
        }

        public virtual bool InsertJobEntity(IJobEntity timerJobEntity)
        {
            return DoInsert(timerJobEntity, true);
        }

        public override void Insert(IJobEntity jobEntity, bool fireCreateEvent)
        {
            DoInsert(jobEntity, fireCreateEvent);
        }

        protected internal virtual bool DoInsert(IJobEntity jobEntity, bool fireCreateEvent)
        {
            // add link to execution
            if (!(jobEntity.ExecutionId is null))
            {
                IExecutionEntity execution = ExecutionEntityManager.FindById<IExecutionEntity>(jobEntity.ExecutionId);
                if (execution != null)
                {
                    execution.Jobs.Add(jobEntity);

                    // Inherit tenant if (if applicable)
                    if (!(execution.TenantId is null))
                    {
                        jobEntity.TenantId = execution.TenantId;
                    }

                    if (IsExecutionRelatedEntityCountEnabled(execution))
                    {
                        ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                        countingExecutionEntity.JobCount += 1;
                    }
                }
                else
                {
                    return false;
                }
            }

            base.Insert(jobEntity, fireCreateEvent);
            return true;
        }

        public virtual IList<IJobEntity> FindJobsToExecute(Page page)
        {
            return jobDataManager.FindJobsToExecute(page);
        }

        public virtual IList<IJobEntity> FindJobsByExecutionId(string executionId)
        {
            return jobDataManager.FindJobsByExecutionId(executionId);
        }

        public virtual IList<IJobEntity> FindJobsByProcessDefinitionId(string processDefinitionId)
        {
            return jobDataManager.FindJobsByProcessDefinitionId(processDefinitionId);
        }

        public virtual IList<IJobEntity> FindJobsByTypeAndProcessDefinitionId(string jobTypeTimer, string id)
        {
            return jobDataManager.FindJobsByTypeAndProcessDefinitionId(jobTypeTimer, id);
        }

        public virtual IList<IJobEntity> FindJobsByProcessInstanceId(string processInstanceId)
        {
            return jobDataManager.FindJobsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IJobEntity> FindExpiredJobs(Page page)
        {
            return jobDataManager.FindExpiredJobs(page);
        }

        public virtual void ResetExpiredJob(string jobId)
        {
            jobDataManager.ResetExpiredJob(jobId);
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(IJobQuery jobQuery, Page page)
        {
            return jobDataManager.FindJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(IJobQuery jobQuery)
        {
            return jobDataManager.FindJobCountByQueryCriteria(jobQuery);
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.UpdateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public override void Delete(IJobEntity jobEntity)
        {
            base.Delete(jobEntity);

            DeleteExceptionByteArrayRef(jobEntity);

            RemoveExecutionLink(jobEntity);

            // Send event
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, this));
            }
        }

        public override void Delete(IJobEntity entity, bool fireDeleteEvent)
        {
            if (!(entity.ExecutionId is null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = ExecutionEntityManager.FindById<ICountingExecutionEntity>(entity.ExecutionId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.JobCount -= 1;
                }
            }
            base.Delete(entity, fireDeleteEvent);
        }

        /// <summary>
        /// Removes the job's execution's reference to this job, if the job has an associated execution.
        /// Subclasses may override to provide custom implementations.
        /// </summary>
        protected internal virtual void RemoveExecutionLink(IJobEntity jobEntity)
        {
            if (!(jobEntity.ExecutionId is null))
            {
                IExecutionEntity execution = ExecutionEntityManager.FindById<IExecutionEntity>(jobEntity.ExecutionId);
                if (execution != null)
                {
                    execution.Jobs.Remove(jobEntity);
                }
            }
        }

        /// <summary>
        /// Deletes a the byte array used to store the exception information.  Subclasses may override
        /// to provide custom implementations.
        /// </summary>
        protected internal virtual void DeleteExceptionByteArrayRef(IJobEntity jobEntity)
        {
            if (jobEntity.ExceptionByteArrayRef is ByteArrayRef exceptionByteArrayRef)
            {
                exceptionByteArrayRef.Delete();
            }
        }

        public virtual IJobDataManager JobDataManager
        {
            get
            {
                return jobDataManager;
            }
            set
            {
                this.jobDataManager = value;
            }
        }

    }

}