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
namespace org.activiti.engine.impl.transformer
{
    /// <summary>
    /// Transforms a <seealso cref="Long"/> to a <seealso cref="Integer"/>
    /// 
    /// 
    /// </summary>
    public class LongToInteger : AbstractTransformer
    {

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        protected internal override object primTransform(object anObject)
        {
            return Convert.ToInt32(((long?)anObject).ToString());
        }
    }

}