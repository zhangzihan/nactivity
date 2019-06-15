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
    using System;

    /// <summary>
    /// Transforms a <seealso cref="String"/> to a <seealso cref="Date"/>
    /// 
    /// 
    /// </summary>
    public class StringToDate : AbstractTransformer
    {
        protected internal override object PrimTransform(object anObject)
        {
            if (DateTime.TryParse(anObject?.ToString(), out var d))
            {
                return d;
            }

            return null;
        }
    }

}