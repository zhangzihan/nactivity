using System;
using System.Text.RegularExpressions;

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
    /// Transforms a <seealso cref="String"/> to a <seealso cref="Boolean"/>
    /// 
    /// 
    /// </summary>
    public class StringToBoolean : AbstractTransformer
    {
        protected internal override object PrimTransform(object anObject)
        {
            string str = anObject?.ToString();
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            if ("true" == str.ToLower() || "1" == str || "是" == str)
            {
                return true;
            }

            return false;
        }
    }

}