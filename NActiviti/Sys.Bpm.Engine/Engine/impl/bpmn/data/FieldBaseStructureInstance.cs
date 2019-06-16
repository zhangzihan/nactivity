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
namespace Sys.Workflow.Engine.Impl.Bpmn.Datas
{

    /// <summary>
    /// An instance of <seealso cref="IFieldBaseStructureDefinition"/>
    /// 
    /// 
    /// </summary>
    public class FieldBaseStructureInstance : IStructureInstance
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal IFieldBaseStructureDefinition structureDefinition;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, object> fieldValues;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structureDefinition"></param>
        public FieldBaseStructureInstance(IFieldBaseStructureDefinition structureDefinition)
        {
            this.structureDefinition = structureDefinition;
            this.fieldValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public virtual object GetFieldValue(string fieldName)
        {
            return this.fieldValues[fieldName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public virtual void SetFieldValue(string fieldName, object value)
        {
            this.fieldValues[fieldName] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int FieldSize
        {
            get
            {
                return this.structureDefinition.FieldSize;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetFieldNameAt(int index)
        {
            return this.structureDefinition.GetFieldNameAt(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object[] ToArray()
        {
            int fieldSize = this.FieldSize;
            object[] arguments = new object[fieldSize];
            for (int i = 0; i < fieldSize; i++)
            {
                string fieldName = this.GetFieldNameAt(i);
                object argument = this.GetFieldValue(fieldName);
                arguments[i] = argument;
            }
            return arguments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        public virtual void LoadFrom(object[] array)
        {
            int fieldSize = this.FieldSize;
            for (int i = 0; i < fieldSize; i++)
            {
                string fieldName = this.GetFieldNameAt(i);
                object fieldValue = array[i];
                this.SetFieldValue(fieldName, fieldValue);
            }
        }
    }
}