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
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher;

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

        public override IHistoricActivityInstanceEntity Create()
        {
            return new HistoricActivityInstanceEntityImpl();
        }

        public virtual IList<IHistoricActivityInstanceEntity> FindUnfinishedHistoricActivityInstancesByExecutionAndActivityId(string executionId, string activityId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>
            {
                ["executionId"] = executionId,
                ["activityId"] = activityId
            };
            return (IList<IHistoricActivityInstanceEntity>)GetList("selectUnfinishedHistoricActivityInstanceExecutionIdAndActivityId", @params, unfinishedHistoricActivityInstanceMatcher, true);
        }

        public virtual IList<IHistoricActivityInstanceEntity> FindUnfinishedHistoricActivityInstancesByProcessInstanceId(string processInstanceId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>
            {
                ["processInstanceId"] = processInstanceId
            };
            return (IList<IHistoricActivityInstanceEntity>)GetList("selectUnfinishedHistoricActivityInstanceExecutionIdAndActivityId", @params, unfinishedHistoricActivityInstanceMatcher, true);
        }

        public virtual void DeleteHistoricActivityInstancesByProcessInstanceId(string historicProcessInstanceId)
        {
            DbSqlSession.Delete("deleteHistoricActivityInstancesByProcessInstanceId", new { historicProcessInstanceId }, typeof(HistoricActivityInstanceEntityImpl));
        }

        public virtual long FindHistoricActivityInstanceCountByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery)
        {
            return DbSqlSession.SelectOne<HistoricActivityInstanceEntityImpl, long?>("selectHistoricActivityInstanceCountByQueryCriteria", historicActivityInstanceQuery).GetValueOrDefault();
        }

        public virtual IList<IHistoricActivityInstance> FindHistoricActivityInstancesByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery, Page page)
        {
            return DbSqlSession.SelectList<HistoricActivityInstanceEntityImpl, IHistoricActivityInstance>("selectHistoricActivityInstancesByQueryCriteria", historicActivityInstanceQuery, page);
        }

        public virtual IList<IHistoricActivityInstance> FindHistoricActivityInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<HistoricActivityInstanceEntityImpl, IHistoricActivityInstance>("selectHistoricActivityInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricActivityInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<EventSubscriptionEntityImpl, long?>("selectHistoricActivityInstanceCountByNativeQuery", parameterMap).GetValueOrDefault();
        }
    }

}