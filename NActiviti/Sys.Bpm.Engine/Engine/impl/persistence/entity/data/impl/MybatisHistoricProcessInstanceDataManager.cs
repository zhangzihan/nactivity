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
    public class MybatisHistoricProcessInstanceDataManager : AbstractDataManager<IHistoricProcessInstanceEntity>, IHistoricProcessInstanceDataManager
    {

        public MybatisHistoricProcessInstanceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(HistoricProcessInstanceEntityImpl);
            }
        }

        public override IHistoricProcessInstanceEntity create()
        {
            return new HistoricProcessInstanceEntityImpl();
        }

        public virtual IHistoricProcessInstanceEntity create(IExecutionEntity processInstanceExecutionEntity)
        {
            return new HistoricProcessInstanceEntityImpl(processInstanceExecutionEntity);
        }

        public virtual IList<string> findHistoricProcessInstanceIdsByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.selectList<HistoricProcessInstanceEntityImpl, string>("selectHistoricProcessInstanceIdsByProcessDefinitionId", new KeyValuePair<string, object>("processDefinitionId", processDefinitionId));
        }

        public virtual IList<IHistoricProcessInstanceEntity> findHistoricProcessInstancesBySuperProcessInstanceId(string superProcessInstanceId)
        {
            return DbSqlSession.selectList<HistoricProcessInstanceEntityImpl, IHistoricProcessInstanceEntity>("selectHistoricProcessInstanceIdsBySuperProcessInstanceId", new KeyValuePair<string, object>("superProcessInstanceId", superProcessInstanceId));
        }

        public virtual long findHistoricProcessInstanceCountByQueryCriteria(HistoricProcessInstanceQueryImpl historicProcessInstanceQuery)
        {
            return ((long?)DbSqlSession.selectOne<HistoricProcessInstanceEntityImpl, long?>("selectHistoricProcessInstanceCountByQueryCriteria", historicProcessInstanceQuery)).GetValueOrDefault();
        }

        public virtual IList<IHistoricProcessInstance> findHistoricProcessInstancesByQueryCriteria(HistoricProcessInstanceQueryImpl historicProcessInstanceQuery)
        {
            return DbSqlSession.selectList<HistoricProcessInstanceEntityImpl, IHistoricProcessInstance>("selectHistoricProcessInstancesByQueryCriteria", historicProcessInstanceQuery);
        }

        public virtual IList<IHistoricProcessInstance> findHistoricProcessInstancesAndVariablesByQueryCriteria(HistoricProcessInstanceQueryImpl historicProcessInstanceQuery)
        {
            // paging doesn't work for combining process instances and variables
            // due to an outer join, so doing it in-memory
            if (historicProcessInstanceQuery.FirstResult < 0 || historicProcessInstanceQuery.MaxResults <= 0)
            {
                return new List<IHistoricProcessInstance>();
            }

            int firstResult = historicProcessInstanceQuery.FirstResult;
            int maxResults = historicProcessInstanceQuery.MaxResults;

            // setting max results, limit to 20000 results for performance reasons
            if (historicProcessInstanceQuery.ProcessInstanceVariablesLimit != null)
            {
                historicProcessInstanceQuery.MaxResults = historicProcessInstanceQuery.ProcessInstanceVariablesLimit.GetValueOrDefault();
            }
            else
            {
                historicProcessInstanceQuery.MaxResults = ProcessEngineConfiguration.HistoricProcessInstancesQueryLimit;
            }
            historicProcessInstanceQuery.FirstResult = 0;

            IList<IHistoricProcessInstance> instanceList = DbSqlSession.selectListWithRawParameterWithoutFilter<HistoricProcessInstanceEntityImpl, IHistoricProcessInstance>("selectHistoricProcessInstancesWithVariablesByQueryCriteria", historicProcessInstanceQuery, historicProcessInstanceQuery.FirstResult, historicProcessInstanceQuery.MaxResults);

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
                        return new List<IHistoricProcessInstance>();
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

        public virtual IList<IHistoricProcessInstance> findHistoricProcessInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<HistoricProcessInstanceEntityImpl, IHistoricProcessInstance>("selectHistoricProcessInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricProcessInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<HistoricProcessInstanceEntityImpl, long?>("selectHistoricProcessInstanceCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

    }

}