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
namespace org.activiti.engine.impl.bpmn.data
{
    /// <summary>
    /// Implementation of the BPMN 2.0 'itemDefinition'
    /// 
    /// 
    /// </summary>
    public class ItemDefinition
    {

        protected internal string id;

        protected internal IStructureDefinition structure;

        protected internal bool isCollection;

        protected internal ItemKind itemKind;

        private ItemDefinition()
        {
            this.isCollection = false;
            this.itemKind = ItemKind.Information;
        }

        public ItemDefinition(string id, IStructureDefinition structure) : this()
        {
            this.id = id;
            this.structure = structure;
        }

        public virtual ItemInstance createInstance()
        {
            return new ItemInstance(this, this.structure.createInstance());
        }

        public virtual IStructureDefinition StructureDefinition
        {
            get
            {
                return this.structure;
            }
        }

        public virtual bool Collection
        {
            get
            {
                return isCollection;
            }
            set
            {
                this.isCollection = value;
            }
        }


        public virtual ItemKind ItemKind
        {
            get
            {
                return itemKind;
            }
            set
            {
                this.itemKind = value;
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