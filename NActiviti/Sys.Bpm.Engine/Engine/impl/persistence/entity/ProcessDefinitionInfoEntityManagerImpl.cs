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

namespace org.activiti.engine.impl.persistence.entity
{
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using System.Collections.Generic;


    /// 
    public class ProcessDefinitionInfoEntityManagerImpl : AbstractEntityManager<IProcessDefinitionInfoEntity>, IProcessDefinitionInfoEntityManager
    {

        protected internal IProcessDefinitionInfoDataManager processDefinitionInfoDataManager;

        public ProcessDefinitionInfoEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IProcessDefinitionInfoDataManager processDefinitionInfoDataManager) : base(processEngineConfiguration)
        {

            this.processDefinitionInfoDataManager = processDefinitionInfoDataManager;
        }

        protected internal override IDataManager<IProcessDefinitionInfoEntity> DataManager
        {
            get
            {
                return processDefinitionInfoDataManager;
            }
        }

        public virtual void insertProcessDefinitionInfo(IProcessDefinitionInfoEntity processDefinitionInfo)
        {
            insert(processDefinitionInfo);
        }

        public virtual void updateProcessDefinitionInfo(IProcessDefinitionInfoEntity updatedProcessDefinitionInfo)
        {
            update(updatedProcessDefinitionInfo, true);
        }

        public virtual void deleteProcessDefinitionInfo(string processDefinitionId)
        {
            IProcessDefinitionInfoEntity processDefinitionInfo = findProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
            if (processDefinitionInfo != null)
            {
                delete(processDefinitionInfo);
                deleteInfoJson(processDefinitionInfo);
            }
        }

        public virtual void updateInfoJson(string id, byte[] json)
        {
            IProcessDefinitionInfoEntity processDefinitionInfo = findById<IProcessDefinitionInfoEntity>(new KeyValuePair<string, object>("id", id));
            if (processDefinitionInfo != null)
            {
                ByteArrayRef @ref = new ByteArrayRef(processDefinitionInfo.InfoJsonId);
                @ref.setValue("json", json);

                if (string.ReferenceEquals(processDefinitionInfo.InfoJsonId, null))
                {
                    processDefinitionInfo.InfoJsonId = @ref.Id;
                    updateProcessDefinitionInfo(processDefinitionInfo);
                }
            }
        }

        public virtual void deleteInfoJson(IProcessDefinitionInfoEntity processDefinitionInfo)
        {
            if (!string.ReferenceEquals(processDefinitionInfo.InfoJsonId, null))
            {
                ByteArrayRef @ref = new ByteArrayRef(processDefinitionInfo.InfoJsonId);
                @ref.delete();
            }
        }

        public virtual IProcessDefinitionInfoEntity findProcessDefinitionInfoByProcessDefinitionId(string processDefinitionId)
        {
            return processDefinitionInfoDataManager.findProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
        }

        public virtual byte[] findInfoJsonById(string infoJsonId)
        {
            ByteArrayRef @ref = new ByteArrayRef(infoJsonId);
            return @ref.Bytes;
        }
    }
}