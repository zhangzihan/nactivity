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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Cfg;
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

        public override IHistoricProcessInstanceEntity Create()
        {
            return new HistoricProcessInstanceEntityImpl();
        }

        public virtual IHistoricProcessInstanceEntity Create(IExecutionEntity processInstanceExecutionEntity)
        {
            return new HistoricProcessInstanceEntityImpl(processInstanceExecutionEntity);
        }

        public virtual IList<string> FindHistoricProcessInstanceIdsByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.SelectList<HistoricProcessInstanceEntityImpl, string>("selectHistoricProcessInstanceIdsByProcessDefinitionId", new { processDefinitionId });
        }

        public virtual IList<IHistoricProcessInstanceEntity> FindHistoricProcessInstancesBySuperProcessInstanceId(string superProcessInstanceId)
        {
            return DbSqlSession.SelectList<HistoricProcessInstanceEntityImpl, IHistoricProcessInstanceEntity>("selectHistoricProcessInstanceIdsBySuperProcessInstanceId", new { superProcessInstanceId });
        }

        public virtual long FindHistoricProcessInstanceCountByQueryCriteria(IHistoricProcessInstanceQuery historicProcessInstanceQuery)
        {
            return DbSqlSession.SelectOne<HistoricProcessInstanceEntityImpl, long?>("selectHistoricProcessInstanceCountByQueryCriteria", historicProcessInstanceQuery).GetValueOrDefault();
        }

        public virtual IList<IHistoricProcessInstance> FindHistoricProcessInstancesByQueryCriteria(IHistoricProcessInstanceQuery historicProcessInstanceQuery)
        {
            return DbSqlSession.SelectList<HistoricProcessInstanceEntityImpl, IHistoricProcessInstance>("selectHistoricProcessInstancesByQueryCriteria", historicProcessInstanceQuery);
        }

        public virtual IList<IHistoricProcessInstance> FindHistoricProcessInstancesAndVariablesByQueryCriteria(IHistoricProcessInstanceQuery historicProcessInstanceQuery)
        {
            var hisQuery = historicProcessInstanceQuery as HistoricProcessInstanceQueryImpl;
            // paging doesn't work for combining process instances and variables
            // due to an outer join, so doing it in-memory
            if (hisQuery.FirstResult < 0 || hisQuery.MaxResults <= 0)
            {
                return new List<IHistoricProcessInstance>();
            }

            int firstResult = hisQuery.FirstResult;
            int maxResults = hisQuery.MaxResults;

            // setting max results, limit to 20000 results for performance reasons
            if (hisQuery.ProcessInstanceVariablesLimit is object)
            {
                hisQuery.MaxResults = hisQuery.ProcessInstanceVariablesLimit.GetValueOrDefault();
            }
            else
            {
                hisQuery.MaxResults = ProcessEngineConfiguration.HistoricProcessInstancesQueryLimit;
            }
            hisQuery.FirstResult = 0;

            IList<IHistoricProcessInstance> instanceList = DbSqlSession.SelectListWithRawParameterWithoutFilter<HistoricProcessInstanceEntityImpl, IHistoricProcessInstance>("selectHistoricProcessInstancesWithVariablesByQueryCriteria", hisQuery, hisQuery.FirstResult, hisQuery.MaxResults);

            if (instanceList is object && instanceList.Count > 0)
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

        public virtual IList<IHistoricProcessInstance> FindHistoricProcessInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<HistoricProcessInstanceEntityImpl, IHistoricProcessInstance>("selectHistoricProcessInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricProcessInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<HistoricProcessInstanceEntityImpl, long?>("selectHistoricProcessInstanceCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

    }

}