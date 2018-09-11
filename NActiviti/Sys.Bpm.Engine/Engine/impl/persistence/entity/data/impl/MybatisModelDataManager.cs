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
    using org.activiti.engine.repository;

    /// 
    public class MybatisModelDataManager : AbstractDataManager<IModelEntity>, IModelDataManager
    {

        public MybatisModelDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(ModelEntityImpl);
            }
        }

        public override IModelEntity create()
        {
            return new ModelEntityImpl();
        }

        public virtual IList<IModel> findModelsByQueryCriteria(ModelQueryImpl query, Page page)
        {
            return DbSqlSession.selectList<ModelEntityImpl, IModel>("selectModelsByQueryCriteria", query, page);
        }

        public virtual long findModelCountByQueryCriteria(ModelQueryImpl query)
        {
            return ((long?)DbSqlSession.selectOne<ModelEntityImpl, long?>("selectModelCountByQueryCriteria", query)).GetValueOrDefault();
        }

        public virtual IList<IModel> findModelsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<ModelEntityImpl, IModel>("selectModelByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findModelCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<ModelEntityImpl, long?>("selectModelCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

    }

}