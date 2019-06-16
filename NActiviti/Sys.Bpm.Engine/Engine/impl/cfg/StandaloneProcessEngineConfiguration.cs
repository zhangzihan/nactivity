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

namespace Sys.Workflow.engine.impl.cfg
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Sys.Workflow.engine.impl.asyncexecutor;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System;

    /// 
    public class StandaloneProcessEngineConfiguration : ProcessEngineConfigurationImpl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="historyService"></param>
        /// <param name="taskService"></param>
        /// <param name="dynamicBpmnService"></param>
        /// <param name="repositoryService"></param>
        /// <param name="runtimeService"></param>
        /// <param name="managementService"></param>
        /// <param name="asyncExecutor"></param>
        /// <param name="configuration"></param>
        public StandaloneProcessEngineConfiguration(IHistoryService historyService,
            ITaskService taskService,
            IDynamicBpmnService dynamicBpmnService,
            IRepositoryService repositoryService,
            IRuntimeService runtimeService,
            IManagementService managementService,
            IAsyncExecutor asyncExecutor,
            IConfiguration configuration) : base(historyService,
                taskService,
                dynamicBpmnService,
                repositoryService,
                runtimeService,
                managementService,
                asyncExecutor,
                configuration)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ICommandInterceptor CreateTransactionInterceptor()
        {
            return null;
        }
    }

}