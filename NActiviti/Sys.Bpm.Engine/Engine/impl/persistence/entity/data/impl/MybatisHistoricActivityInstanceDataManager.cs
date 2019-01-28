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
    using org.activiti.engine.impl.persistence.entity.data.impl.cachematcher;

    /// 
    public class MybatisHistoricActivityInstanceDataManager : AbstractDataManager<IHistoricActivityInstanceEntity>, IHistoricActivityInstanceDataManager
    {

        protected internal ICachedEntityMatcher<IHistoricActivityInstanceEntity> unfinishedHistoricActivityInstanceMatcher = new UnfinishedHistoricActivityInstanceMatcher();

        public MybatisHistoricActivityInstanceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(HistoricActivityInstanceEntityImpl);
            }
        }

        public override IHistoricActivityInstanceEntity create()
        {
            return new HistoricActivityInstanceEntityImpl();
        }

        public virtual IList<IHistoricActivityInstanceEntity> findUnfinishedHistoricActivityInstancesByExecutionAndActivityId(string executionId, string activityId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>();
            @params["executionId"] = executionId;
            @params["activityId"] = activityId;
            return (IList<IHistoricActivityInstanceEntity>)getList("selectUnfinishedHistoricActivityInstanceExecutionIdAndActivityId", @params, unfinishedHistoricActivityInstanceMatcher, true);
        }

        public virtual IList<IHistoricActivityInstanceEntity> findUnfinishedHistoricActivityInstancesByProcessInstanceId(string processInstanceId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>();
            @params["processInstanceId"] = processInstanceId;
            return (IList<IHistoricActivityInstanceEntity>)getList("selectUnfinishedHistoricActivityInstanceExecutionIdAndActivityId", @params, unfinishedHistoricActivityInstanceMatcher, true);
        }

        public virtual void deleteHistoricActivityInstancesByProcessInstanceId(string historicProcessInstanceId)
        {
            DbSqlSession.delete("deleteHistoricActivityInstancesByProcessInstanceId", new { historicProcessInstanceId }, typeof(HistoricActivityInstanceEntityImpl));
        }

        public virtual long findHistoricActivityInstanceCountByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery)
        {
            return DbSqlSession.selectOne<HistoricActivityInstanceEntityImpl, long?>("selectHistoricActivityInstanceCountByQueryCriteria", historicActivityInstanceQuery).GetValueOrDefault();
        }

        public virtual IList<IHistoricActivityInstance> findHistoricActivityInstancesByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery, Page page)
        {
            return DbSqlSession.selectList<HistoricActivityInstanceEntityImpl, IHistoricActivityInstance>("selectHistoricActivityInstancesByQueryCriteria", historicActivityInstanceQuery, page);
        }

        public virtual IList<IHistoricActivityInstance> findHistoricActivityInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<HistoricActivityInstanceEntityImpl, IHistoricActivityInstance>("selectHistoricActivityInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricActivityInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.selectOne<EventSubscriptionEntityImpl, long?>("selectHistoricActivityInstanceCountByNativeQuery", parameterMap).GetValueOrDefault();
        }
    }

}