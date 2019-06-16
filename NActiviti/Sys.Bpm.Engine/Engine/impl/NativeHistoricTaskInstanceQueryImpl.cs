using System;
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
namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Interceptor;

    [Serializable]
    public class NativeHistoricTaskInstanceQueryImpl : AbstractNativeQuery<INativeHistoricTaskInstanceQuery, IHistoricTaskInstance>, INativeHistoricTaskInstanceQuery
    {

        private const long serialVersionUID = 1L;

        public  NativeHistoricTaskInstanceQueryImpl(ICommandContext  commandContext) : base(commandContext)
        {
        }

        public NativeHistoricTaskInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public  override IList<IHistoricTaskInstance> ExecuteList(ICommandContext  commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.HistoricTaskInstanceEntityManager.FindHistoricTaskInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public  override long ExecuteCount(ICommandContext  commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.HistoricTaskInstanceEntityManager.FindHistoricTaskInstanceCountByNativeQuery(parameterMap);
        }

    }

}