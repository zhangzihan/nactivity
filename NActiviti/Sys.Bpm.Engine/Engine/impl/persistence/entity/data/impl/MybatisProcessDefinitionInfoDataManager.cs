using System;

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
    using System.Collections.Generic;

    /// 
    public class MybatisProcessDefinitionInfoDataManager : AbstractDataManager<IProcessDefinitionInfoEntity>, IProcessDefinitionInfoDataManager
    {

        public MybatisProcessDefinitionInfoDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(ProcessDefinitionInfoEntityImpl);
            }
        }

        public override IProcessDefinitionInfoEntity Create()
        {
            return new ProcessDefinitionInfoEntityImpl();
        }

        public virtual IProcessDefinitionInfoEntity FindProcessDefinitionInfoByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.SelectOne<ProcessDefinitionInfoEntityImpl, IProcessDefinitionInfoEntity>("selectProcessDefinitionInfoByProcessDefinitionId", new { processDefinitionId });
        }
    }

}