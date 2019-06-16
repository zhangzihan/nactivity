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

using Microsoft.Extensions.Configuration;
using Sys.Workflow.engine.impl.asyncexecutor;

namespace Sys.Workflow.engine.impl.cfg
{
    /// 
    public class StandaloneInMemProcessEngineConfiguration : StandaloneProcessEngineConfiguration
    {
        public StandaloneInMemProcessEngineConfiguration(
            IHistoryService historyService,
            ITaskService taskService,
            IDynamicBpmnService dynamicBpmnService,
            IRepositoryService repositoryService,
            IRuntimeService runtimeService,
            IManagementService managementService,
            IAsyncExecutor asyncExecutor,
            IConfiguration configuration) : base(
                historyService,
                taskService,
                dynamicBpmnService,
                repositoryService,
                runtimeService,
                managementService,
                asyncExecutor,
                configuration)
        {
        }
    }

}