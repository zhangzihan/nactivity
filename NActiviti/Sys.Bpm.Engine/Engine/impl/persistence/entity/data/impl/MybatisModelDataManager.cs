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

    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Repository;

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

        public override IModelEntity Create()
        {
            return new ModelEntityImpl();
        }

        public virtual IList<IModel> FindModelsByQueryCriteria(IModelQuery query, Page page)
        {
            return DbSqlSession.SelectList<ModelEntityImpl, IModel>("selectModelsByQueryCriteria", query, page);
        }

        public virtual long FindModelCountByQueryCriteria(IModelQuery query)
        {
            return DbSqlSession.SelectOne<ModelEntityImpl, long?>("selectModelCountByQueryCriteria", query).GetValueOrDefault();
        }

        public virtual IList<IModel> FindModelsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<ModelEntityImpl, IModel>("selectModelByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindModelCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<ModelEntityImpl, long?>("selectModelCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

    }

}