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

    using org.activiti.engine.history;
    using org.activiti.engine.impl.cfg;
    using System.Linq;

    /// 
    public class MybatisHistoricTaskInstanceDataManager : AbstractDataManager<IHistoricTaskInstanceEntity>, IHistoricTaskInstanceDataManager
    {
        public MybatisHistoricTaskInstanceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(HistoricTaskInstanceEntityImpl);
            }
        }

        public override IHistoricTaskInstanceEntity create()
        {
            return new HistoricTaskInstanceEntityImpl();
        }

        public virtual IHistoricTaskInstanceEntity create(ITaskEntity task, IExecutionEntity execution)
        {
            return new HistoricTaskInstanceEntityImpl(task, execution);
        }

        public virtual IList<IHistoricTaskInstanceEntity> findHistoricTasksByParentTaskId(string parentTaskId)
        {
            return DbSqlSession.selectList<HistoricTaskInstanceEntityImpl, IHistoricTaskInstanceEntity>("selectHistoricTasksByParentTaskId", new { parentTaskId });
        }

        public virtual IList<IHistoricTaskInstanceEntity> findHistoricTaskInstanceByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<HistoricTaskInstanceEntityImpl, IHistoricTaskInstanceEntity>("selectHistoricTaskInstancesByProcessInstanceId", new { processInstanceId });
        }

        public virtual long findHistoricTaskInstanceCountByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            return ((long?)DbSqlSession.selectOne<HistoricTaskInstanceEntityImpl, long?>("selectHistoricTaskInstanceCountByQueryCriteria", historicTaskInstanceQuery)).GetValueOrDefault();
        }

        public virtual IList<IHistoricTaskInstance> findHistoricTaskInstancesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            return DbSqlSession.selectList<HistoricTaskInstanceEntityImpl, IHistoricTaskInstance>("selectHistoricTaskInstancesByQueryCriteria", historicTaskInstanceQuery);
        }

        public virtual IList<IHistoricTaskInstance> findHistoricTaskInstancesAndVariablesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            var query = historicTaskInstanceQuery as HistoricTaskInstanceQueryImpl;
            // paging doesn't work for combining task instances and variables
            // due to an outer join, so doing it in-memory
            if (query.FirstResult < 0 || query.MaxResults <= 0)
            {
                return new List<IHistoricTaskInstance>();
            }

            int firstResult = query.FirstResult;
            int maxResults = query.MaxResults;

            // setting max results, limit to 20000 results for performance reasons
            if (query.TaskVariablesLimit != null)
            {
                query.MaxResults = query.TaskVariablesLimit.GetValueOrDefault();
            }
            else
            {
                query.MaxResults = ProcessEngineConfiguration.HistoricTaskQueryLimit;
            }
            query.FirstResult = 0;

            IList<IHistoricTaskInstance> instanceList = DbSqlSession.selectListWithRawParameterWithoutFilter<HistoricTaskInstanceEntityImpl, IHistoricTaskInstance>("selectHistoricTaskInstancesWithVariablesByQueryCriteria", historicTaskInstanceQuery, query.FirstResult, query.MaxResults);

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
                        return new List<IHistoricTaskInstance>();
                    }
                }
                else
                {
                    int toIndex = Math.Min(maxResults, instanceList.Count);
                    return instanceList.Skip(0).Take(toIndex).ToList();
                }
            }

            return instanceList;
        }

        public virtual IList<IHistoricTaskInstance> findHistoricTaskInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<HistoricTaskInstanceEntityImpl, IHistoricTaskInstance>("selectHistoricTaskInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricTaskInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<HistoricTaskInstanceEntityImpl, long?>("selectHistoricTaskInstanceCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

    }

}