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

        public override ITaskEntity create()
        {
            return new TaskEntityImpl();
        }

        public virtual IList<ITaskEntity> findTasksByExecutionId(string executionId)
        {
            return (IList<ITaskEntity>)getList("selectTasksByExecutionId", new KeyValuePair<string, object>("executionId", executionId), tasksByExecutionIdMatcher, true);
        }

        public virtual IList<ITaskEntity> findTasksByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<TaskEntityImpl, ITaskEntity>("selectTasksByProcessInstanceId", new KeyValuePair<string, object>("processInstanceId", processInstanceId));
        }

        public virtual IList<ITask> findTasksByQueryCriteria(TaskQueryImpl taskQuery)
        {
            const string query = "selectTaskByQueryCriteria";
            return DbSqlSession.selectList<TaskEntityImpl, ITask>(query, taskQuery);
        }

        public virtual IList<ITask> findTasksAndVariablesByQueryCriteria(TaskQueryImpl taskQuery)
        {
            const string query = "selectTaskWithVariablesByQueryCriteria";
            // paging doesn't work for combining task instances and variables due to
            // an outer join, so doing it in-memory
            if (taskQuery.FirstResult < 0 || taskQuery.MaxResults <= 0)
            {
                return new List<ITask>();
            }

            int firstResult = taskQuery.FirstResult;
            int maxResults = taskQuery.MaxResults;

            // setting max results, limit to 20000 results for performance reasons
            if (taskQuery.TaskVariablesLimit != null)
            {
                taskQuery.MaxResults = taskQuery.TaskVariablesLimit.GetValueOrDefault();
            }
            else
            {
                taskQuery.MaxResults = ProcessEngineConfiguration.TaskQueryLimit;
            }
            taskQuery.FirstResult = 0;

            IList<ITask> instanceList = DbSqlSession.selectListWithRawParameterWithoutFilter<TaskEntityImpl, ITask>(query, taskQuery, taskQuery.FirstResult, taskQuery.MaxResults);

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

        public virtual long findTaskCountByQueryCriteria(TaskQueryImpl taskQuery)
        {
            return ((long?)DbSqlSession.selectOne<TaskEntityImpl, long?>("selectTaskCountByQueryCriteria", taskQuery)).GetValueOrDefault();
        }

        public virtual IList<ITask> findTasksByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<TaskEntityImpl, ITask>("selectTaskByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findTaskCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<TaskEntityImpl, long?>("selectTaskCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

        public virtual IList<ITask> findTasksByParentTaskId(string parentTaskId)
        {
            return DbSqlSession.selectList<TaskEntityImpl, ITask>("selectTasksByParentTaskId", new KeyValuePair<string, object>("parentTaskId", parentTaskId));
        }

        public virtual void updateTaskTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["deploymentId"] = deploymentId;
            @params["tenantId"] = newTenantId;
            DbSqlSession.update<TaskEntityImpl>("updateTaskTenantIdForDeployment", @params);
        }

    }

}