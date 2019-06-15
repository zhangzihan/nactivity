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
    using org.activiti.engine.impl.persistence.entity.data.impl.cachematcher;

    /// 
    public class MybatisIdentityLinkDataManager : AbstractDataManager<IIdentityLinkEntity>, IIdentityLinkDataManager
    {

        protected internal ICachedEntityMatcher<IIdentityLinkEntity> identityLinkByProcessInstanceMatcher = new IdentityLinksByProcInstMatcher();

        public MybatisIdentityLinkDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(IdentityLinkEntityImpl);
            }
        }

        public override IIdentityLinkEntity Create()
        {
            return new IdentityLinkEntityImpl();
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinksByTaskId(string taskId)
        {
            return DbSqlSession.SelectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinksByTask", new { taskId });
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinksByProcessInstanceId(string processInstanceId)
        {
            return (IList<IIdentityLinkEntity>)GetList("selectIdentityLinksByProcessInstance", new { processInstanceId }, identityLinkByProcessInstanceMatcher, true);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinksByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.SelectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinksByProcessDefinition", new { processDefinitionId });
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinkByTaskUserGroupAndType(string taskId, string userId, string groupId, string type)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                ["taskId"] = taskId,
                ["userId"] = userId,
                ["groupId"] = groupId,
                ["type"] = type
            };
            return DbSqlSession.SelectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinkByTaskUserGroupAndType", parameters);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinkByProcessInstanceUserGroupAndType(string processInstanceId, string userId, string groupId, string type)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                ["processInstanceId"] = processInstanceId,
                ["userId"] = userId,
                ["groupId"] = groupId,
                ["type"] = type
            };
            return DbSqlSession.SelectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinkByProcessInstanceUserGroupAndType", parameters);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinkByProcessDefinitionUserAndGroup(string processDefinitionId, string userId, string groupId)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                ["processDefinitionId"] = processDefinitionId,
                ["userId"] = userId,
                ["groupId"] = groupId
            };
            return DbSqlSession.SelectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinkByProcessDefinitionUserAndGroup", parameters);
        }

        public virtual void DeleteIdentityLinksByProcDef(string processDefId)
        {
            DbSqlSession.Delete("deleteIdentityLinkByProcDef", new { processDefId }, typeof(IdentityLinkEntityImpl));
        }

    }

}