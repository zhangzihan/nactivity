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

namespace org.activiti.engine.impl.persistence.entity
{


    /// 
    [Serializable]
    public class ProcessDefinitionInfoEntityImpl : AbstractEntity, IProcessDefinitionInfoEntity
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitionId;
        protected internal string infoJsonId;

        public ProcessDefinitionInfoEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                persistentState["processDefinitionId"] = this.processDefinitionId;
                persistentState["infoJsonId"] = this.infoJsonId;
                return persistentState;
            }
        }

        // getters and setters //////////////////////////////////////////////////////

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }


        public virtual string InfoJsonId
        {
            get
            {
                return infoJsonId;
            }
            set
            {
                this.infoJsonId = value;
            }
        }

    }
}