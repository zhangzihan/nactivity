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
    /// An instance of <seealso cref="ItemDefinition"/>
    /// 
    /// 
    /// </summary>
    public class ItemInstance
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ItemDefinition item;

        /// <summary>
        /// 
        /// </summary>
        protected internal IStructureInstance structureInstance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="structureInstance"></param>
        public ItemInstance(ItemDefinition item, IStructureInstance structureInstance)
        {
            this.item = item;
            this.structureInstance = structureInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ItemDefinition Item
        {
            get
            {
                return this.item;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IStructureInstance StructureInstance
        {
            get
            {
                return this.structureInstance;
            }
        }

        private FieldBaseStructureInstance FieldBaseStructureInstance
        {
            get
            {
                return (FieldBaseStructureInstance)this.structureInstance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public virtual object GetFieldValue(string fieldName)
        {
            return this.FieldBaseStructureInstance.GetFieldValue(fieldName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public virtual void SetFieldValue(string fieldName, object value)
        {
            this.FieldBaseStructureInstance.SetFieldValue(fieldName, value);
        }
    }
}