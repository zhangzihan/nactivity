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
namespace org.activiti.engine.impl.bpmn.data
{

    /// <summary>
    /// Represents a simple in memory structure
    /// 
    /// 
    /// </summary>
    public class SimpleStructureDefinition : IFieldBaseStructureDefinition
    {

        protected internal string id;

        protected internal IList<string> fieldNames;

        protected internal IList<Type> fieldTypes;

        public SimpleStructureDefinition(string id)
        {
            this.id = id;
            this.fieldNames = new List<string>();
            this.fieldTypes = new List<Type>();
        }

        public virtual int FieldSize
        {
            get
            {
                return this.fieldNames.Count;
            }
        }

        public virtual string Id
        {
            get
            {
                return this.id;
            }
        }

        public virtual void setFieldName(int index, string fieldName, Type type)
        {
            this.growListToContain(index, this.fieldNames);
            this.growListToContain(index, this.fieldTypes);
            this.fieldNames[index] = fieldName;
            this.fieldTypes[index] = type;
        }

        private void growListToContain<T1>(int index, IList<T1> list)
        {
            if (!(list.Count - 1 >= index))
            {
                for (int i = list.Count; i <= index; i++)
                {
                    list.Add(default(T1));
                }
            }
        }

        public virtual string getFieldNameAt(int index)
        {
            return this.fieldNames[index];
        }

        public virtual Type getFieldTypeAt(int index)
        {
            return this.fieldTypes[index];
        }

        public virtual IStructureInstance createInstance()
        {
            return new FieldBaseStructureInstance(this);
        }
    }

}