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
namespace Sys.Workflow.engine.impl.transformer
{

    /// <summary>
    /// A Transformer is responsible of transforming an object into a different object
    /// 
    /// 
    /// </summary>
    public interface ITransformer
    {

        /// <summary>
        /// Transforms anObject into a different object
        /// </summary>
        /// <param name="anObject">
        ///          the object to be transformed </param>
        /// <returns> the transformed object </returns>
        /// <exception cref="ActivitiException">
        ///           if the transformation could not be applied </exception>
        object Transform(object anObject);
    }
}