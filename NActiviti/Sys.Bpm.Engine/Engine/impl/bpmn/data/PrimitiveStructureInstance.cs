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
    /// An instance of <seealso cref="PrimitiveStructureDefinition"/>
    /// 
    /// 
    /// </summary>
    public class PrimitiveStructureInstance : IStructureInstance
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal object primitive;

        /// <summary>
        /// 
        /// </summary>
        protected internal PrimitiveStructureDefinition definition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        public PrimitiveStructureInstance(PrimitiveStructureDefinition definition) : this(definition, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="primitive"></param>
        public PrimitiveStructureInstance(PrimitiveStructureDefinition definition, object primitive)
        {
            this.definition = definition;
            this.primitive = primitive;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual object Primitive
        {
            get
            {
                return this.primitive;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object[] ToArray()
        {
            return new object[] { this.primitive };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        public virtual void LoadFrom(object[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                object @object = array[i];
                if (this.definition.PrimitiveClass.IsInstanceOfType(@object))
                {
                    this.primitive = @object;
                    return;
                }
            }
        }
    }
}