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

        public virtual void InsertProcessDefinitionInfo(IProcessDefinitionInfoEntity processDefinitionInfo)
        {
            Insert(processDefinitionInfo);
        }

        public virtual void UpdateProcessDefinitionInfo(IProcessDefinitionInfoEntity updatedProcessDefinitionInfo)
        {
            Update(updatedProcessDefinitionInfo, true);
        }

        public virtual void DeleteProcessDefinitionInfo(string processDefinitionId)
        {
            IProcessDefinitionInfoEntity processDefinitionInfo = FindProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
            if (processDefinitionInfo != null)
            {
                Delete(processDefinitionInfo);
                DeleteInfoJson(processDefinitionInfo);
            }
        }

        public virtual void UpdateInfoJson(string id, byte[] json)
        {
            IProcessDefinitionInfoEntity processDefinitionInfo = FindById<IProcessDefinitionInfoEntity>(id);
            if (processDefinitionInfo != null)
            {
                ByteArrayRef @ref = new ByteArrayRef(processDefinitionInfo.InfoJsonId);
                @ref.SetValue("json", json);

                if (processDefinitionInfo.InfoJsonId is null)
                {
                    processDefinitionInfo.InfoJsonId = @ref.Id;
                    UpdateProcessDefinitionInfo(processDefinitionInfo);
                }
            }
        }

        public virtual void DeleteInfoJson(IProcessDefinitionInfoEntity processDefinitionInfo)
        {
            if (!(processDefinitionInfo.InfoJsonId is null))
            {
                ByteArrayRef @ref = new ByteArrayRef(processDefinitionInfo.InfoJsonId);
                @ref.Delete();
            }
        }

        public virtual IProcessDefinitionInfoEntity FindProcessDefinitionInfoByProcessDefinitionId(string processDefinitionId)
        {
            return processDefinitionInfoDataManager.FindProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
        }

        public virtual byte[] FindInfoJsonById(string infoJsonId)
        {
            ByteArrayRef @ref = new ByteArrayRef(infoJsonId);
            return @ref.Bytes;
        }
    }
}