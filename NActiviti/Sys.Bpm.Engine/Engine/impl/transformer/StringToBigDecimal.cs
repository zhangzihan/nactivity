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
    /// Transforms a <seealso cref="String"/> to a <seealso cref="BigDecimal"/>
    /// 
    /// 
    /// </summary>
    public class StringToBigDecimal : AbstractTransformer
    {
        protected internal override object PrimTransform(object anObject)
        {
            if (decimal.TryParse(anObject?.ToString(), out var d))
            {
                return d;
            }

            return null;
        }
    }

}