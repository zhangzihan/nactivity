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
    using Sys.Bpm.Engine;


    /// 
    public class MybatisAttachmentDataManager : AbstractDataManager<IAttachmentEntity>, IAttachmentDataManager
    {

        public MybatisAttachmentDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(AttachmentEntityImpl);
            }
        }

        public override IAttachmentEntity create()
        {
            return new AttachmentEntityImpl();
        }

        public virtual IList<IAttachmentEntity> findAttachmentsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<AttachmentEntityImpl, IAttachmentEntity>("selectAttachmentsByProcessInstanceId", new KeyValuePair<string, object>("processInstanceId", processInstanceId));
        }

        public virtual IList<IAttachmentEntity> findAttachmentsByTaskId(string taskId)
        {
            return DbSqlSession.selectList<AttachmentEntityImpl, IAttachmentEntity>("selectAttachmentsByTaskId", new KeyValuePair<string, object>("taskId", taskId));
        }
    }
}