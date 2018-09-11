using System;

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
    /// Represents a structure encapsulated in a class
    /// 
    /// 
    /// </summary>
    public class ClassStructureDefinition : IFieldBaseStructureDefinition
    {

        protected internal string id;

        protected internal Type classStructure;

        public ClassStructureDefinition(Type classStructure) : this(classStructure.FullName, classStructure)
        {
        }

        public ClassStructureDefinition(string id, Type classStructure)
        {
            this.id = id;
            this.classStructure = classStructure;
        }

        public virtual string Id
        {
            get
            {
                return this.id;
            }
        }

        public virtual int FieldSize
        {
            get
            {
                // TODO
                return 0;
            }
        }

        public virtual string getFieldNameAt(int index)
        {
            // TODO
            return null;
        }

        public virtual Type getFieldTypeAt(int index)
        {
            // TODO
            return null;
        }

        public virtual IStructureInstance createInstance()
        {
            return new FieldBaseStructureInstance(this);
        }
    }

}