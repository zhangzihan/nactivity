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


    public class MybatisHistoricDetailDataManager : AbstractDataManager<IHistoricDetailEntity>, IHistoricDetailDataManager
    {

        public MybatisHistoricDetailDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(HistoricDetailEntityImpl);
            }
        }

        public override IHistoricDetailEntity create()
        {
            // Superclass is abstract
            throw new System.NotSupportedException();
        }

        public virtual IHistoricDetailAssignmentEntity createHistoricDetailAssignment()
        {
            return new HistoricDetailAssignmentEntityImpl();
        }

        public virtual IHistoricDetailTransitionInstanceEntity createHistoricDetailTransitionInstance()
        {
            return new HistoricDetailTransitionInstanceEntityImpl();
        }

        public virtual IHistoricDetailVariableInstanceUpdateEntity createHistoricDetailVariableInstanceUpdate()
        {
            return new HistoricDetailVariableInstanceUpdateEntityImpl();
        }

        public virtual IList<IHistoricDetailEntity> findHistoricDetailsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<HistoricDetailEntityImpl, IHistoricDetailEntity>("selectHistoricDetailByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<IHistoricDetailEntity> findHistoricDetailsByTaskId(string taskId)
        {
            return DbSqlSession.selectList<HistoricDetailEntityImpl, IHistoricDetailEntity>("selectHistoricDetailByTaskId", new { taskId });
        }

        public virtual long findHistoricDetailCountByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery)
        {
            return DbSqlSession.selectOne<HistoricDetailEntityImpl, long?>("selectHistoricDetailCountByQueryCriteria", historicVariableUpdateQuery).GetValueOrDefault();
        }

        public virtual IList<IHistoricDetail> findHistoricDetailsByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery, Page page)
        {
            return DbSqlSession.selectList<HistoricDetailEntityImpl, IHistoricDetail>("selectHistoricDetailsByQueryCriteria", historicVariableUpdateQuery, page);
        }

        public virtual IList<IHistoricDetail> findHistoricDetailsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<HistoricDetailEntityImpl, IHistoricDetail>("selectHistoricDetailByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricDetailCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<HistoricDetailEntityImpl, long?>("selectHistoricDetailCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

    }

}