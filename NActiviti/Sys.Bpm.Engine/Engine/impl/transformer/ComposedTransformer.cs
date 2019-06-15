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
namespace org.activiti.engine.impl.transformer
{

    /// <summary>
    /// Applies a list of transformers to the input object
    /// 
    /// 
    /// </summary>
    public class ComposedTransformer : AbstractTransformer
    {

        protected internal IList<ITransformer> transformers;

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        protected internal override object PrimTransform(object anObject)
        {
            object current = anObject;
            foreach (ITransformer transformer in this.transformers)
            {
                current = transformer.Transform(current);
            }
            return current;
        }
    }

}