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

        public override IHistoricVariableInstanceEntity create()
        {
            return new HistoricVariableInstanceEntityImpl();
        }

        public override void insert(IHistoricVariableInstanceEntity entity)
        {
            base.insert(entity);
        }

        public virtual IList<IHistoricVariableInstanceEntity> findHistoricVariableInstancesByProcessInstanceId(string processInstanceId)
        {
            return getList("selectHistoricVariableInstanceByProcessInstanceId", new KeyValuePair<string, object>("processInstanceId", processInstanceId), historicVariableInstanceByProcInstMatcher, true) as IList<IHistoricVariableInstanceEntity>;
        }

        public virtual IList<IHistoricVariableInstanceEntity> findHistoricVariableInstancesByTaskId(string taskId)
        {
            return getList("selectHistoricVariableInstanceByTaskId", new KeyValuePair<string, object>("taskId", taskId), historicVariableInstanceByTaskIdMatcher, true) as IList<IHistoricVariableInstanceEntity>;
        }

        public virtual long findHistoricVariableInstanceCountByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery)
        {
            return ((long?)DbSqlSession.selectOne<HistoricVariableInstanceEntityImpl, long?>("selectHistoricVariableInstanceCountByQueryCriteria", historicProcessVariableQuery)).GetValueOrDefault();
        }

        public virtual IList<IHistoricVariableInstance> findHistoricVariableInstancesByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery, Page page)
        {
            return DbSqlSession.selectList<HistoricVariableInstanceEntityImpl, IHistoricVariableInstance>("selectHistoricVariableInstanceByQueryCriteria", historicProcessVariableQuery, page);
        }

        public virtual IHistoricVariableInstanceEntity findHistoricVariableInstanceByVariableInstanceId(string variableInstanceId)
        {
            return (IHistoricVariableInstanceEntity)DbSqlSession.selectOne<HistoricVariableInstanceEntityImpl, IHistoricVariableInstanceEntity>("selectHistoricVariableInstanceByVariableInstanceId", new KeyValuePair<string, object>("variableInstanceId", variableInstanceId));
        }

        public virtual IList<IHistoricVariableInstance> findHistoricVariableInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<HistoricVariableInstanceEntityImpl, IHistoricVariableInstance>("selectHistoricVariableInstanceByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricVariableInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<HistoricVariableInstanceEntityImpl, long?>("selectHistoricVariableInstanceCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

    }

}