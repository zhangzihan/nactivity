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
using System;

namespace org.activiti.engine.impl.transformer
{

    /// <summary>
    /// A Transformer is responsible of transforming an object into a different object
    /// 
    /// 
    /// </summary>
    public abstract class AbstractTransformer : ITransformer
    {

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual object Transform(object anObject)
        {
            try
            {
                return this.PrimTransform(anObject);
            }
            catch (Exception)
            {

                throw new ActivitiException("Error while executing transformation from object: " + anObject + " using transformer " + this);
            }
        }

        /// <summary>
        /// Transforms anObject into a different object
        /// </summary>
        /// <param name="anObject">
        ///          the object to be transformed </param>
        /// <returns> the transformed object </returns>
        protected internal abstract object PrimTransform(object anObject);
    }

}