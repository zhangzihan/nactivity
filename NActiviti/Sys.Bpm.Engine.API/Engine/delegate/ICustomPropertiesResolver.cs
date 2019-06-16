using Sys.Workflow.engine.impl.persistence.entity;
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
namespace Sys.Workflow.engine.@delegate
{

    /// <summary>
    /// Can be used to pass a custom properties <seealso cref="java.util.HashMap"/> to a <seealso cref="ITransactionDependentExecutionListener"/>
    /// or to a <seealso cref="ITransactionDependentTaskListener"/>
    /// 
    /// 
    /// </summary>
    public interface ICustomPropertiesResolver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        IDictionary<string, object> GetCustomPropertiesMap(IExecutionEntity execution);
    }

}