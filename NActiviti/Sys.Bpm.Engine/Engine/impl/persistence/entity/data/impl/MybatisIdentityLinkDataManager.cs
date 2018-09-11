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
    public class MybatisIdentityLinkDataManager : AbstractDataManager<IIdentityLinkEntity>, IdentityLinkDataManager
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

        public override IIdentityLinkEntity create()
        {
            return new IdentityLinkEntityImpl();
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinksByTaskId(string taskId)
        {
            return DbSqlSession.selectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinksByTask", new KeyValuePair<string, object>("taskId", taskId));
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinksByProcessInstanceId(string processInstanceId)
        {
            return (IList<IIdentityLinkEntity>)getList("selectIdentityLinksByProcessInstance", new KeyValuePair<string, object>("processInstanceId", processInstanceId), identityLinkByProcessInstanceMatcher, true);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinksByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.selectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinksByProcessDefinition", new KeyValuePair<string, object>("processDefinitionId", processDefinitionId));
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinkByTaskUserGroupAndType(string taskId, string userId, string groupId, string type)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["taskId"] = taskId;
            parameters["userId"] = userId;
            parameters["groupId"] = groupId;
            parameters["type"] = type;
            return DbSqlSession.selectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinkByTaskUserGroupAndType", parameters);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinkByProcessInstanceUserGroupAndType(string processInstanceId, string userId, string groupId, string type)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["processInstanceId"] = processInstanceId;
            parameters["userId"] = userId;
            parameters["groupId"] = groupId;
            parameters["type"] = type;
            return DbSqlSession.selectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinkByProcessInstanceUserGroupAndType", parameters);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinkByProcessDefinitionUserAndGroup(string processDefinitionId, string userId, string groupId)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["processDefinitionId"] = processDefinitionId;
            parameters["userId"] = userId;
            parameters["groupId"] = groupId;
            return DbSqlSession.selectList<IdentityLinkEntityImpl, IIdentityLinkEntity>("selectIdentityLinkByProcessDefinitionUserAndGroup", parameters);
        }

        public virtual void deleteIdentityLinksByProcDef(string processDefId)
        {
            DbSqlSession.delete("deleteIdentityLinkByProcDef", new KeyValuePair<string, object>("processDefId", processDefId), typeof(IdentityLinkEntityImpl));
        }

    }

}