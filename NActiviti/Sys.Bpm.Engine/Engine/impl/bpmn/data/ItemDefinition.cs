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
namespace Sys.Workflow.engine.impl.bpmn.data
{
    /// <summary>
    /// Implementation of the BPMN 2.0 'itemDefinition'
    /// 
    /// 
    /// </summary>
    public class ItemDefinition
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal string id;

        /// <summary>
        /// 
        /// </summary>
        protected internal IStructureDefinition structure;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isCollection;

        /// <summary>
        /// 
        /// </summary>
        protected internal ItemKind itemKind;

        private ItemDefinition()
        {
            this.isCollection = false;
            this.itemKind = ItemKind.Information;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="structure"></param>
        public ItemDefinition(string id, IStructureDefinition structure) : this()
        {
            this.id = id;
            this.structure = structure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ItemInstance CreateInstance()
        {
            return new ItemInstance(this, this.structure.CreateInstance());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IStructureDefinition StructureDefinition
        {
            get
            {
                return this.structure;
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public virtual string Id
        {
            get
            {
                return this.id;
            }
        }
    }
}