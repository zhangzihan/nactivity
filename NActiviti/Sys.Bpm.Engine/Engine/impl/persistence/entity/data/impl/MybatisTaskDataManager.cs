using System;
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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data.impl.cachematcher;
    using org.activiti.engine.task;
    using System.Linq;

    /// 
    public class MybatisTaskDataManager : AbstractDataManager<ITaskEntity>, ITaskDataManager
    {

        protected internal ICachedEntityMatcher<ITaskEntity> tasksByExecutionIdMatcher = new TasksByExecutionIdMatcher();

        public MybatisTaskDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(TaskEntityImpl);
            }
        }

        public override ITaskEntity Create()
        {
            return new TaskEntityImpl();
        }

        public virtual IList<ITaskEntity> FindTasksByExecutionId(string executionId)
        {
            return (IList<ITaskEntity>)GetList("selectTasksByExecutionId", new { executionId }, tasksByExecutionIdMatcher, true);
        }

        public virtual IList<ITaskEntity> FindTasksByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<TaskEntityImpl, ITaskEntity>("selectTasksByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<ITask> FindTasksByQueryCriteria(ITaskQuery taskQuery)
        {
            const string query = "selectTaskByQueryCriteria";
            return DbSqlSession.SelectList<TaskEntityImpl, ITask>(query, taskQuery);
        }

        public virtual IList<ITask> FindTasksAndVariablesByQueryCriteria(ITaskQuery taskQuery)
        {
            const string query = "selectTaskWithVariablesByQueryCriteria";
            TaskQueryImpl taskQueyImpl = taskQuery as TaskQueryImpl;
            // paging doesn't work for combining task instances and variables due to
            // an outer join, so doing it in-memory
            if (taskQueyImpl.FirstResult < 0 || taskQueyImpl.MaxResults <= 0)
            {
                return new List<ITask>();
            }

            int firstResult = taskQueyImpl.FirstResult;
            int maxResults = taskQueyImpl.MaxResults;

            // setting max results, limit to 20000 results for performance reasons
            if (taskQueyImpl.TaskVariablesLimit != null)
            {
                taskQueyImpl.MaxResults = taskQueyImpl.TaskVariablesLimit.GetValueOrDefault();
            }
            else
            {
                taskQueyImpl.MaxResults = ProcessEngineConfiguration.TaskQueryLimit;
            }
            taskQueyImpl.FirstResult = 0;

            IList<ITask> instanceList = DbSqlSession.SelectListWithRawParameterWithoutFilter<TaskEntityImpl, ITask>(query, taskQueyImpl, taskQueyImpl.FirstResult, taskQueyImpl.MaxResults);

            if (instanceList != null && instanceList.Count > 0)
            {
                if (firstResult > 0)
                {
                    if (firstResult <= instanceList.Count)
                    {
                        int toIndex = firstResult + Math.Min(maxResults, instanceList.Count - firstResult);
                        return instanceList.Skip(firstResult).Take(toIndex).ToList();
                    }
                    else
                    {
                        return new List<ITask>();
                    }
                }
                else
                {
                    int toIndex = Math.Min(maxResults, instanceList.Count);
                    return instanceList.Skip(0).Take(toIndex).ToList();
                }
            }
            return new List<ITask>();
        }

        public virtual long FindTaskCountByQueryCriteria(ITaskQuery taskQuery)
        {
            return DbSqlSession.SelectOne<TaskEntityImpl, long?>("selectTaskCountByQueryCriteria", taskQuery).GetValueOrDefault();
        }

        public virtual IList<ITask> FindTasksByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<TaskEntityImpl, ITask>("selectTaskByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindTaskCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<TaskEntityImpl, long?>("selectTaskCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

        public virtual IList<ITask> FindTasksByParentTaskId(string parentTaskId)
        {
            return DbSqlSession.SelectList<TaskEntityImpl, ITask>("selectTasksByParentTaskId", new { parentTaskId });
        }

        public virtual void UpdateTaskTenantIdForDeployment(string deploymentId, string tenantId)
        {
            DbSqlSession.Update<TaskEntityImpl>("updateTaskTenantIdForDeployment", new { deploymentId, tenantId });
        }
    }
}