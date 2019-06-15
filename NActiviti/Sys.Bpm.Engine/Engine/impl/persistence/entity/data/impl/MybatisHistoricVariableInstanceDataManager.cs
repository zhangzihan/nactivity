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
    public class MybatisHistoricVariableInstanceDataManager : AbstractDataManager<IHistoricVariableInstanceEntity>, IHistoricVariableInstanceDataManager
    {

        protected internal ICachedEntityMatcher<IHistoricVariableInstanceEntity> historicVariableInstanceByTaskIdMatcher = new HistoricVariableInstanceByTaskIdMatcher();

        protected internal ICachedEntityMatcher<IHistoricVariableInstanceEntity> historicVariableInstanceByProcInstMatcher = new HistoricVariableInstanceByProcInstMatcher();

        public MybatisHistoricVariableInstanceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(HistoricVariableInstanceEntityImpl);
            }
        }

        public override IHistoricVariableInstanceEntity Create()
        {
            return new HistoricVariableInstanceEntityImpl();
        }

        public override void Insert(IHistoricVariableInstanceEntity entity)
        {
            base.Insert(entity);
        }

        public virtual IList<IHistoricVariableInstanceEntity> FindHistoricVariableInstancesByProcessInstanceId(string processInstanceId)
        {
            return GetList("selectHistoricVariableInstanceByProcessInstanceId", new { processInstanceId }, historicVariableInstanceByProcInstMatcher, true) as IList<IHistoricVariableInstanceEntity>;
        }

        public virtual IList<IHistoricVariableInstanceEntity> FindHistoricVariableInstancesByTaskId(string taskId)
        {
            return GetList("selectHistoricVariableInstanceByTaskId", new { taskId }, historicVariableInstanceByTaskIdMatcher, true) as IList<IHistoricVariableInstanceEntity>;
        }

        public virtual long FindHistoricVariableInstanceCountByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery)
        {
            return DbSqlSession.SelectOne<HistoricVariableInstanceEntityImpl, long?>("selectHistoricVariableInstanceCountByQueryCriteria", historicProcessVariableQuery).GetValueOrDefault();
        }

        public virtual IList<IHistoricVariableInstance> FindHistoricVariableInstancesByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery, Page page)
        {
            return DbSqlSession.SelectList<HistoricVariableInstanceEntityImpl, IHistoricVariableInstance>("selectHistoricVariableInstanceByQueryCriteria", historicProcessVariableQuery, page);
        }

        public virtual IHistoricVariableInstanceEntity FindHistoricVariableInstanceByVariableInstanceId(string variableInstanceId)
        {
            return (IHistoricVariableInstanceEntity)DbSqlSession.SelectOne<HistoricVariableInstanceEntityImpl, IHistoricVariableInstanceEntity>("selectHistoricVariableInstanceByVariableInstanceId", new { variableInstanceId });
        }

        public virtual IList<IHistoricVariableInstance> FindHistoricVariableInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<HistoricVariableInstanceEntityImpl, IHistoricVariableInstance>("selectHistoricVariableInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricVariableInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<HistoricVariableInstanceEntityImpl, long?>("selectHistoricVariableInstanceCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

    }

}