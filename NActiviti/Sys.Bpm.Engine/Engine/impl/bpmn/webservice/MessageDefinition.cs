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
namespace org.activiti.engine.impl.bpmn.webservice
{
    using org.activiti.engine.impl.bpmn.data;

    /// <summary>
    /// Implementation of the BPMN 2.0 'message'
    /// 
    /// 
    /// </summary>
    public class MessageDefinition
    {

        protected internal string id;

        protected internal ItemDefinition itemDefinition;

        public MessageDefinition(string id)
        {
            this.id = id;
        }

        public virtual MessageInstance createInstance()
        {
            return new MessageInstance(this, this.itemDefinition.createInstance());
        }

        public virtual ItemDefinition ItemDefinition
        {
            get
            {
                return this.itemDefinition;
            }
            set
            {
                this.itemDefinition = value;
            }
        }

        public virtual IStructureDefinition StructureDefinition
        {
            get
            {
                return this.itemDefinition.StructureDefinition;
            }
        }


        public virtual string Id
        {
            get
            {
                return this.id;
            }
        }
    }

}