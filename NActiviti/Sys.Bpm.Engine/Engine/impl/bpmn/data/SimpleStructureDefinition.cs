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
namespace Sys.Workflow.engine.impl.bpmn.data
{
    /// <summary>
    /// Represents a simple in memory structure
    /// 
    /// 
    /// </summary>
    public class SimpleStructureDefinition : IFieldBaseStructureDefinition
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal string id;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<string> fieldNames;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<Type> fieldTypes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public SimpleStructureDefinition(string id)
        {
            this.id = id;
            this.fieldNames = new List<string>();
            this.fieldTypes = new List<Type>();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int FieldSize
        {
            get
            {
                return this.fieldNames.Count;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        public virtual void SetFieldName(int index, string fieldName, Type type)
        {
            this.GrowListToContain(index, this.fieldNames);
            this.GrowListToContain(index, this.fieldTypes);
            this.fieldNames[index] = fieldName;
            this.fieldTypes[index] = type;
        }

        ///
        private void GrowListToContain<T1>(int index, IList<T1> list)
        {
            if (!(list.Count - 1 >= index))
            {
                for (int i = list.Count; i <= index; i++)
                {
                    list.Add(default);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetFieldNameAt(int index)
        {
            return this.fieldNames[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual Type GetFieldTypeAt(int index)
        {
            return this.fieldTypes[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IStructureInstance CreateInstance()
        {
            return new FieldBaseStructureInstance(this);
        }
    }
}