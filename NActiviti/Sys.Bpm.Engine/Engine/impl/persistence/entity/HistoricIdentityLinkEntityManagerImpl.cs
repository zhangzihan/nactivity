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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;

    /// 
    /// 
    public class HistoricIdentityLinkEntityManagerImpl : AbstractEntityManager<IHistoricIdentityLinkEntity>, IHistoricIdentityLinkEntityManager
    {

        protected internal IHistoricIdentityLinkDataManager historicIdentityLinkDataManager;


        public HistoricIdentityLinkEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IHistoricIdentityLinkDataManager historicIdentityLinkDataManager) : base(processEngineConfiguration)
        {
            this.historicIdentityLinkDataManager = historicIdentityLinkDataManager;
        }

        protected internal override IDataManager<IHistoricIdentityLinkEntity> DataManager
        {
            get
            {
                return historicIdentityLinkDataManager;
            }
        }

        public virtual IList<IHistoricIdentityLinkEntity> FindHistoricIdentityLinksByTaskId(string taskId)
        {
            return historicIdentityLinkDataManager.FindHistoricIdentityLinksByTaskId(taskId);
        }

        public virtual IList<IHistoricIdentityLinkEntity> FindHistoricIdentityLinksByProcessInstanceId(string processInstanceId)
        {
            return historicIdentityLinkDataManager.FindHistoricIdentityLinksByProcessInstanceId(processInstanceId);
        }

        public virtual void DeleteHistoricIdentityLinksByTaskId(string taskId)
        {
            IList<IHistoricIdentityLinkEntity> identityLinks = FindHistoricIdentityLinksByTaskId(taskId);
            foreach (IHistoricIdentityLinkEntity identityLink in identityLinks)
            {
                Delete(identityLink);
            }
        }

        public virtual void DeleteHistoricIdentityLinksByProcInstance(string processInstanceId)
        {

            IList<IHistoricIdentityLinkEntity> identityLinks = historicIdentityLinkDataManager.FindHistoricIdentityLinksByProcessInstanceId(processInstanceId);

            foreach (IHistoricIdentityLinkEntity identityLink in identityLinks)
            {
                Delete(identityLink);
            }

        }

        public virtual IHistoricIdentityLinkDataManager HistoricIdentityLinkDataManager
        {
            get
            {
                return historicIdentityLinkDataManager;
            }
            set
            {
                this.historicIdentityLinkDataManager = value;
            }
        }


    }

}