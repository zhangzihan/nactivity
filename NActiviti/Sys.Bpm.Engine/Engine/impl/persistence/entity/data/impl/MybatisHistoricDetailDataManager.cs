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

        public override IHistoricDetailEntity Create()
        {
            // Superclass is abstract
            throw new System.NotSupportedException();
        }

        public virtual IHistoricDetailAssignmentEntity CreateHistoricDetailAssignment()
        {
            return new HistoricDetailAssignmentEntityImpl();
        }

        public virtual IHistoricDetailTransitionInstanceEntity CreateHistoricDetailTransitionInstance()
        {
            return new HistoricDetailTransitionInstanceEntityImpl();
        }

        public virtual IHistoricDetailVariableInstanceUpdateEntity CreateHistoricDetailVariableInstanceUpdate()
        {
            return new HistoricDetailVariableInstanceUpdateEntityImpl();
        }

        public virtual IList<IHistoricDetailEntity> FindHistoricDetailsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<HistoricDetailEntityImpl, IHistoricDetailEntity>("selectHistoricDetailByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<IHistoricDetailEntity> FindHistoricDetailsByTaskId(string taskId)
        {
            return DbSqlSession.SelectList<HistoricDetailEntityImpl, IHistoricDetailEntity>("selectHistoricDetailByTaskId", new { taskId });
        }

        public virtual long FindHistoricDetailCountByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery)
        {
            return DbSqlSession.SelectOne<HistoricDetailEntityImpl, long?>("selectHistoricDetailCountByQueryCriteria", historicVariableUpdateQuery).GetValueOrDefault();
        }

        public virtual IList<IHistoricDetail> FindHistoricDetailsByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery, Page page)
        {
            return DbSqlSession.SelectList<HistoricDetailEntityImpl, IHistoricDetail>("selectHistoricDetailsByQueryCriteria", historicVariableUpdateQuery, page);
        }

        public virtual IList<IHistoricDetail> FindHistoricDetailsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<HistoricDetailEntityImpl, IHistoricDetail>("selectHistoricDetailByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricDetailCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<HistoricDetailEntityImpl, long?>("selectHistoricDetailCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

    }

}