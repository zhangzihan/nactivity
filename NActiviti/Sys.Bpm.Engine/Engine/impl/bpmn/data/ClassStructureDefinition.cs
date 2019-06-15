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
        /// <summary>
        /// 
        /// </summary>
        protected internal string id;

        /// <summary>
        /// 
        /// </summary>
        protected internal Type classStructure;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classStructure"></param>
        public ClassStructureDefinition(Type classStructure) : this(classStructure.FullName, classStructure)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="classStructure"></param>
        public ClassStructureDefinition(string id, Type classStructure)
        {
            this.id = id;
            this.classStructure = classStructure;
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
        public virtual int FieldSize
        {
            get
            {
                // TODO
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetFieldNameAt(int index)
        {
            // TODO
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual Type GetFieldTypeAt(int index)
        {
            // TODO
            return null;
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