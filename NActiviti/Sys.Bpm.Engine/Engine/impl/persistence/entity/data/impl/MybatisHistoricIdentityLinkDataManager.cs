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
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher;

    /// 
    public class MybatisHistoricIdentityLinkDataManager : AbstractDataManager<IHistoricIdentityLinkEntity>, IHistoricIdentityLinkDataManager
    {

        protected internal ICachedEntityMatcher<IHistoricIdentityLinkEntity> historicIdentityLinksByProcInstMatcher = new HistoricIdentityLinksByProcInstMatcher();

        public MybatisHistoricIdentityLinkDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(HistoricIdentityLinkEntityImpl);
            }
        }

        public override IHistoricIdentityLinkEntity Create()
        {
            return new HistoricIdentityLinkEntityImpl();
        }

        public virtual IList<IHistoricIdentityLinkEntity> FindHistoricIdentityLinksByTaskId(string taskId)
        {
            return DbSqlSession.SelectList<HistoricIdentityLinkEntityImpl, IHistoricIdentityLinkEntity>("selectHistoricIdentityLinksByTask", new { taskId });
        }

        public virtual IList<IHistoricIdentityLinkEntity> FindHistoricIdentityLinksByProcessInstanceId(string processInstanceId)
        {
            return (IList<IHistoricIdentityLinkEntity>)GetList("selectHistoricIdentityLinksByProcessInstance", new { processInstanceId }, historicIdentityLinksByProcInstMatcher, true);
        }

    }

}